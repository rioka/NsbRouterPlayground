using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NsbRouterPlayground.Integration.Messages.Events;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport.SQLServer;

namespace NsbRouterPlayground.AdazzleUpdater {

   static class Program {

      public static void Main(string[] args) {

         CreateHostBuilder(args).Build().Run();
      }

      static IHostBuilder CreateHostBuilder(string[] args) {

         return Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .UseNServiceBus(ctx => {

               var endpointConfiguration = new EndpointConfiguration("AdazzleUpdater");

               endpointConfiguration.EnableInstallers();
               endpointConfiguration.SendFailedMessagesTo("error");
               endpointConfiguration.AuditProcessedMessagesTo("audit");

               endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

               endpointConfiguration
                  .Conventions()
                  .DefiningCommandsAs(t => t.Namespace?.EndsWith("Messages.Commands") ?? false)
                  .DefiningEventsAs(t => t.Namespace?.EndsWith("Messages.Events") ?? false);

               var connectionString = ctx.Configuration.GetConnectionString("Nsb");

               #region Transport

               var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
               transport
                  .ConnectionString(connectionString)
                  // should not be required using a more recent version of NServiceBus.SqlServer
                  .Transactions(TransportTransactionMode.SendsAtomicWithReceive)
                  .DefaultSchema("nsb");

               var deliverySettings = transport.UseNativeDelayedDelivery();
               deliverySettings.DisableTimeoutManagerCompatibility();

               // avoid noise in profiler
               deliverySettings.ProcessingInterval(TimeSpan.FromMinutes(2));
               transport
                  .WithPeekDelay(TimeSpan.FromMinutes(2));

               #endregion

               #region Persistence

               var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
               persistence.ConnectionBuilder(() => new SqlConnection(connectionString));
               var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
               dialect.Schema("nsb");

               var subscriptions = persistence.SubscriptionSettings();
               subscriptions.CacheFor(TimeSpan.FromMinutes(20));

               // use Outbox to avoid processing messages more than once in case of errors 
               // at Router when forwarding a message
               var outboxSettings = endpointConfiguration.EnableOutbox();
               outboxSettings.KeepDeduplicationDataFor(TimeSpan.FromDays(3));
               outboxSettings.RunDeduplicationDataCleanupEvery(TimeSpan.FromMinutes(30));

               #endregion

               #region Routing

               var routerConnector = transport.Routing().ConnectToRouter("NsbRouterPlayground.Router");
               routerConnector.RegisterPublisher(
                  eventType: typeof(VendorCreated),
                  publisherEndpointName: "WebApi");

               #endregion

               return endpointConfiguration;
            });
      }

      static async Task OnCriticalError(ICriticalErrorContext context) {
         // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
         // https://docs.particular.net/nservicebus/hosting/critical-errors
         // and consider setting up service recovery
         // https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
         try {
            await context.Stop();
         }
         finally {
            FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
         }
      }

      static void FailFast(string message, Exception exception) {
         try {
            log.Fatal(message, exception);

            // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
            // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
         }
         finally {
            Environment.FailFast(message, exception);
         }
      }

      // TODO: optionally choose a custom logging library
      // https://docs.particular.net/nservicebus/logging/#custom-logging
      // LogManager.Use<TheLoggingFactory>();
      static readonly ILog log = LogManager.GetLogger(typeof(Program));
   }
}
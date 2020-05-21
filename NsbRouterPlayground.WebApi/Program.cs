using System;
using System.Data.SqlClient;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport.SQLServer;

namespace NsbRouterPlayground.WebApi {

   public class Program {
   
      public static void Main(string[] args) {
         
         CreateHostBuilder(args)
            .Build()
            .Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) {

         var builder = Host
            .CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());

         // since IMessageSession in injected in controller, must be called first
         // as hosted services are started sequentially according to the order
         // they were registered
         builder.UseNServiceBus(context => {

            var endpointConfiguration = new EndpointConfiguration("WebApi");

            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            endpointConfiguration
               .Conventions()
               .DefiningCommandsAs(t => t.Namespace?.EndsWith("Messages.Commands") ?? false)
               .DefiningEventsAs(t => t.Namespace?.EndsWith("Messages.Events") ?? false);

            var connectionString = context.Configuration.GetConnectionString("WebApi");

            #region Transport

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport
               .ConnectionString(connectionString)
               // should not be required using a more recent version of NServiceBus.SqlServer
               .Transactions(TransportTransactionMode.SendsAtomicWithReceive)
               .DefaultSchema("nsb");

            var deliverySettings = transport.NativeDelayedDelivery();
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

            #endregion

            return endpointConfiguration;
         });

         builder
            .ConfigureWebHostDefaults(webBuilder => {
               webBuilder.UseStartup<Startup>();
            });

         return builder;
      }
   }
}
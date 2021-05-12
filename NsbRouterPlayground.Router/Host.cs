using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Router;
using NServiceBus.Transport.SQLServer;

namespace NsbRouterPlayground.Router {

   internal class Host {

      #region Data

      // TODO: optionally choose a custom logging library
      // https://docs.particular.net/nservicebus/logging/#custom-logging
      // LogManager.Use<TheLoggingFactory>();

      private static readonly ILog Log = LogManager.GetLogger<Host>();
      private IRouter _endpoint;

      public string EndpointName => "NsbRouterPlayground.Router";

      public static readonly string WebApiInterface = "WebApi";
      public static readonly string BackendInterface = "Backend";

      #endregion

      #region Apis

      public async Task Start() {

         var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

         var routerConfig = new RouterConfiguration(EndpointName);
         // if we want to change to something other than "poison"
         // routerConfig.PoisonQueueName = "..."

         #region Configure WebApi interface 

         var webApiConnectionString = config.GetConnectionString("WebApi");
         var webApiInterface = routerConfig.AddInterface<SqlServerTransport>(WebApiInterface, t => {
            t.ConnectionString(webApiConnectionString);
            t.DefaultSchema("nsb");
            // two connection strings, would be escalated to distributed otherwise
            t.Transactions(TransportTransactionMode.ReceiveOnly);
         });

         #endregion

         #region Configure backend interface 

         var backendConnectionString = config.GetConnectionString("Nsb");

         var backendInterface = routerConfig.AddInterface<SqlServerTransport>(BackendInterface, t => {
            t.ConnectionString(backendConnectionString);
            t.DefaultSchema("nsb");
            // two connection strings, would be escalated to distributed otherwise
            //t.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
            t.Transactions(TransportTransactionMode.ReceiveOnly);
         });

         #endregion

         routerConfig.AutoCreateQueues();

         #region Routing

         var staticRouting = routerConfig.UseStaticRoutingProtocol();

         // shortcut to 
         // AddRoute((i, d) => i == incomingInterface), "Interface = " + incomingInterface, null, outgoingInterface);
         staticRouting.AddForwardRoute(WebApiInterface, BackendInterface);
         staticRouting.AddForwardRoute(BackendInterface, WebApiInterface);

         #endregion

         try {

            _endpoint = NServiceBus.Router.Router.Create(routerConfig);
            await _endpoint.Start();
         }
         catch (Exception ex) {

            FailFast("Failed to start.", ex);
         }
      }

      public async Task Stop() {

         try {
            // TODO: perform any further shutdown operations before or after stopping the endpoint
            await _endpoint?.Stop();
         }
         catch (Exception ex) {

            FailFast("Failed to stop correctly.", ex);
         }
      }

      #endregion

      #region Internals

      async Task OnCriticalError(ICriticalErrorContext context) {

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

      void FailFast(string message, Exception exception) {

         try {

            Log.Fatal(message, exception);

            // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
            // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
         }
         finally {

            Environment.FailFast(message, exception);
         }
      }

      #endregion
   }
}

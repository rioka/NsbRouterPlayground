using System.ServiceProcess;

namespace NsbRouterPlayground.Router {

   internal class WindowsService : ServiceBase {

      private readonly Host _host;

      public WindowsService(Host host) {
         _host = host;
      }

      protected override void OnStart(string[] args) =>
         _host.Start().GetAwaiter().GetResult();

      protected override void OnStop() =>
         _host.Stop().GetAwaiter().GetResult();
   }
}

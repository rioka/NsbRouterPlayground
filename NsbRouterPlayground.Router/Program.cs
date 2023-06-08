using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NsbRouterPlayground.Router.Internals;
using NsbRouterPlayground.Router.Internals.Configuration;

namespace NsbRouterPlayground.Router;

internal class Program
{
  public static async Task Main(string[] args)
  {
    Console.WriteLine("Hello, World!");
  }

  internal static IHostBuilder CreateHostBuilder(string[] args)
  {
    var hb = Host
      .CreateDefaultBuilder(args)
      /*
         Microsoft.Extensions.Hosting.HostBuilder.Build
         - calls CreateServiceProvider, which calls
           - serviceCollection.AddSingleton<IHostLifetime, ConsoleLifetime>();
         So "ConsoleLifetime" is the default.
         It is safe to call "UseWindowsService" anyway, because it is called after HostBuilder.Build,
         and possibly overrides IHostLifetime, *if*
         - app it's running in Windows
         - and it is running as a service (using parent process)
       */
      .UseWindowsService();

    // Set up dependency injection
    hb.ConfigureServices((ctx, serviceCollection) => {
      
      serviceCollection.AddSingleton(provider => {
        
        var logger = provider.GetService<Microsoft.Extensions.Logging.ILogger<RouterConfiguration>>();
        var routerConfig = RouterConfiguration.Build(ctx, logger);

        return NServiceBus.Router.Router.Create(routerConfig);
      });
      serviceCollection.AddSingleton<IHostedService, RouterHostedService>();
    });

    return hb;
  }
}
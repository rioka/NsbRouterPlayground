using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NsbRouterPlayground.Bootstrap;
using NsbRouterPlayground.Bootstrap.Infrastructure;
using NsbRouterPlayground.Common;
using NsbRouterPlayground.Common.Messages.Commands;
using NServiceBus;

namespace NsbRouterPlayground.Sender;

internal partial class Program
{
  public static async Task Main(string[] args)
  {
    var host = CreateHostBuilder(args)
      .Build();

    using (host)
    {
      var config = host.Services.GetRequiredService<IConfiguration>();
      await DbHelpers.EnsureDatabaseExists(config.GetConnectionString("Data"));
      
      await host.StartAsync();
      var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
      var session = host.Services.GetRequiredService<IMessageSession>();

      await SendMessages(session); 

      lifetime.StopApplication();
      await host.WaitForShutdownAsync();
    }
  }

  private static IHostBuilder CreateHostBuilder(string[] args)
  {
    var hb = Host
      .CreateDefaultBuilder(args)
      .UseConsoleLifetime()
      .UseNServiceBus(ctx => {

        var endpointConfig = Bootstrapper.Configure(Endpoints.Sender, ctx.Configuration.GetConnectionString("Data"), messages: new [] {
          typeof(CreateVendor)
        });
        return endpointConfig;
      });
    
    return hb;
  }
}
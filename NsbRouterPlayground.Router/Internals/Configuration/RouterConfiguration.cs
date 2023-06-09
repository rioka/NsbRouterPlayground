using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NsbRouterPlayground.Common;
using NServiceBus;
using NServiceBus.Router;

namespace NsbRouterPlayground.Router.Internals.Configuration;

internal class RouterConfiguration
{
  internal static NServiceBus.Router.RouterConfiguration Build(HostBuilderContext ctx, ILogger<RouterConfiguration> logger)
  {
    var routerConfig = new NServiceBus.Router.RouterConfiguration(Endpoints.Router);

    var config = ctx.Configuration;

    AddInterface(routerConfig, Endpoints.Receiver, "Receiver", config, logger, "nsb");
    AddInterface(routerConfig, Endpoints.Sender, "Sender", config, logger, "nsb");
    AddInterface(routerConfig, Endpoints.Notifier, "Notifier", config, logger, "nsb");
    
    // Do not do this in a production environment!
    routerConfig.AutoCreateQueues();
    
    var staticRouting = routerConfig.UseStaticRoutingProtocol();
    
    AddRoute(staticRouting, Endpoints.Receiver, "Receiver", config, logger);
    AddRoute(staticRouting, Endpoints.Sender, "Sender", config, logger);
    AddRoute(staticRouting, Endpoints.Notifier, "Notifier", config, logger);

    return routerConfig;
  }

  /// <summary>
  /// Adds interface for <paramref name="endpointName"/> to the router if <paramref name="connectionStringName"/> for the interface is present.
  /// <paramref name="config"/> is used to retrieve the actual connection string to use.
  /// </summary>
  /// <param name="routerConfig">Router configuration</param>
  /// <param name="endpointName">Endpoint being configured</param>
  /// <param name="connectionStringName">Name of the connection string used for the interface being configured</param>
  /// <param name="config">Application settings</param>
  /// <param name="logger">Logger</param>
  /// <param name="schemaName">Optional name of the database schema to use</param>
  internal static void AddInterface(
    NServiceBus.Router.RouterConfiguration routerConfig,
    string endpointName,
    string connectionStringName,
    IConfiguration config,
    ILogger<RouterConfiguration> logger,
    string schemaName = "dbo")
  {
    if (!string.IsNullOrWhiteSpace(config.GetConnectionString(connectionStringName))) 
    {
      routerConfig.AddInterface<SqlServerTransport>(endpointName, t => {
        t.ConnectionString(config.GetConnectionString(connectionStringName));
        t.DefaultSchema(schemaName);
        t.Transactions(TransportTransactionMode.ReceiveOnly);
      });
    }
    else 
    {
      logger.LogWarning("Interface for endpointName {Endpoint} can't be added as no connection string is set for {ConnectionStringName}", endpointName, connectionStringName);
    }
  }

  /// <summary>
  /// Adds the route to the router for <paramref name="endpointName"/> if <paramref name="connectionStringName"/> for the route is defined.
  /// <paramref name="config"/> is used to retrieve the actual connection string to use.
  /// </summary>
  /// <param name="staticRouting">Route table being configured</param>
  /// <param name="endpointName">The endpoint messages are routed to</param>
  /// <param name="connectionStringName">Name of the connection string used to verify if a route is defined</param>
  /// <param name="config">Application settings</param>
  /// <param name="logger">Logger</param>
  internal static void AddRoute(
    RouteTable staticRouting,
    string endpointName,
    string connectionStringName,
    IConfiguration config,
    ILogger<RouterConfiguration> logger)
  {

    if (!string.IsNullOrWhiteSpace(config.GetConnectionString(connectionStringName))) 
    {
      staticRouting.AddRoute(
        destinationFilter: (iface, destination) =>
          destination.Endpoint.Equals(endpointName, StringComparison.OrdinalIgnoreCase),
        destinationFilterDescription: $"To {endpointName}",
        gateway: null,
        iface: endpointName);
    }
    else 
    {
      logger.LogWarning("Route for endpointName {Endpoint} can't be added as no connection string is set for {ConnectionStringName}", endpointName, connectionStringName);
    }
  }
}
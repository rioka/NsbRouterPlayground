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

    // Keeping it simple - leaving interfaces and routes here for now until we know more about routing rules
    // We can then separate Interfaces and routes per Endpoint/Application
    // AddInterface(routerConfig, EndpointDirectory.MDM, EndpointConnectionString.MDM, config, logger, "nsb");
    // AddInterface(routerConfig, EndpointDirectory.FINANCIALBRIDGE, EndpointConnectionString.FINANCIALBRIDGE, config, logger, "nsb");
    // // AddInterface(routerConfig, EndpointDirectory.VIMM, EndpointConnectionString.VIMM, config, logger, "nsb");
    // AddInterface(routerConfig, EndpointDirectory.MEDIA_MODULE, EndpointConnectionString.MEDIA_MODULE, config, logger, "nsb");
    // AddInterface(routerConfig, EndpointDirectory.FDM, EndpointConnectionString.FDM, config, logger, "nsb");
    // AddInterface(routerConfig, EndpointDirectory.RDM, EndpointConnectionString.RDM, config, logger, "nsb");
    // AddInterface(routerConfig, EndpointDirectory.USER_STORE, EndpointConnectionString.UserStore, config, logger, "nsb");
    // AddInterface(routerConfig, EndpointDirectory.VENDOR_INBOX, EndpointConnectionString.VENDOR_INBOX, config, logger, "nsb");
    //
    // // The account will need to have permissions to create queues, otherwise we should have default scripts that need be added to projects to setup router tables.
    // // Question: does this make sense at all?
    // // i.e. do we want the account to have that privilege?
    // // Moreover, now all DB have router tables in their database projects
    // routerConfig.AutoCreateQueues();
    //
    // var staticRouting = routerConfig.UseStaticRoutingProtocol();
    //
    // AddRoute(staticRouting, EndpointDirectory.MDM, EndpointConnectionString.MDM, config, logger);
    // AddRoute(staticRouting, EndpointDirectory.FINANCIALBRIDGE, EndpointConnectionString.FINANCIALBRIDGE, config, logger);
    // // AddRoute(staticRouting, EndpointDirectory.VIMM, EndpointConnectionString.VIMM, config, logger);
    // AddRoute(staticRouting, EndpointDirectory.MEDIA_MODULE, EndpointConnectionString.MEDIA_MODULE, config, logger);
    // AddRoute(staticRouting, EndpointDirectory.FDM, EndpointConnectionString.FDM, config, logger);
    // AddRoute(staticRouting, EndpointDirectory.RDM, EndpointConnectionString.RDM, config, logger);
    // AddRoute(staticRouting, EndpointDirectory.USER_STORE, EndpointConnectionString.UserStore, config, logger);
    // AddRoute(staticRouting, EndpointDirectory.VENDOR_INBOX, EndpointConnectionString.VENDOR_INBOX, config, logger);

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
  internal static void AddInterface(NServiceBus.Router.RouterConfiguration routerConfig,
    string endpointName,
    string connectionStringName,
    IConfiguration config,
    ILogger<RouterConfiguration> logger,
    string schemaName = "dbo")
  {

    if (!string.IsNullOrWhiteSpace(config.GetConnectionString(connectionStringName))) {
      routerConfig.AddInterface<SqlServerTransport>(endpointName, t => {
        t.ConnectionString(config.GetConnectionString(connectionStringName));
        t.DefaultSchema(schemaName);
        t.Transactions(TransportTransactionMode.ReceiveOnly);
      });
    }
    else {
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
  internal static void AddRoute(RouteTable staticRouting,
    string endpointName,
    string connectionStringName,
    IConfiguration config,
    ILogger<RouterConfiguration> logger)
  {

    if (!string.IsNullOrWhiteSpace(config.GetConnectionString(connectionStringName))) {
      staticRouting.AddRoute(
        destinationFilter: (iface, destination) =>
          destination.Endpoint.Equals(endpointName, StringComparison.OrdinalIgnoreCase),
        destinationFilterDescription: $"To {endpointName}",
        gateway: null,
        iface: endpointName);
    }
    else {
      logger.LogWarning("Route for endpointName {Endpoint} can't be added as no connection string is set for {ConnectionStringName}", endpointName, connectionStringName);
    }
  }
}
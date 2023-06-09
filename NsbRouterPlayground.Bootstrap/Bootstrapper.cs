using System.Reflection;
using Microsoft.Data.SqlClient;
using NsbRouterPlayground.Common;
using NsbRouterPlayground.Common.Attributes;
using NServiceBus;

namespace NsbRouterPlayground.Bootstrap;

public class Bootstrapper
{
  public static Task<IEndpointInstance> Start(string endpointName, string connectionString) 
  {
    return Endpoint.Start(Configure(endpointName, connectionString));
  }

  public static EndpointConfiguration Configure(string endpointName,
    string connectionString,
    string nsbSchema = "nsb",
    string router = Endpoints.Router,
    IEnumerable<Type>? messages = null)
  {
    var config = new EndpointConfiguration(endpointName);

    ConfigureConventions(config);
    ConfigureTransport(config, connectionString, nsbSchema, router, messages);
    ConfigurePersistence(config, connectionString, nsbSchema);

    config.AuditProcessedMessagesTo("audit");
    config.SendFailedMessagesTo("error");
    
    config.EnableInstallers();

    return config;
  }

  #region Internals

  private static void ConfigureConventions(EndpointConfiguration config)
  {
    config.Conventions().DefiningCommandsAs(t => t.Namespace?.Contains("Messages.Commands") ?? false);
    config.Conventions().DefiningEventsAs(t => t.Namespace?.Contains("Messages.Events") ?? false);
    config.Conventions().DefiningMessagesAs(t => t.Namespace?.EndsWith("Messages") ?? false);
  }

  private static void ConfigureTransport(
    EndpointConfiguration config, 
    string connectionString,
    string nsbSchema,
    string router,
    IEnumerable<Type>? messages = null)
  {
    var transport = config.UseTransport<SqlServerTransport>();
    transport
      .Transactions(TransportTransactionMode.TransactionScope)
      .ConnectionString(connectionString);

    if (!string.IsNullOrWhiteSpace(nsbSchema))
    {
      transport.DefaultSchema(nsbSchema);
    }

    ConfigureRoutes(transport.Routing(), router, messages ?? Enumerable.Empty<Type>());
  }

  private static void ConfigureRoutes(
    RoutingSettings<SqlServerTransport> routing,
    string router,
    IEnumerable<Type> messages)
  {
    var settings = routing.ConnectToRouter(router);
    
    foreach (var type in messages)
    {
      var routingInfo = type.GetCustomAttribute<NsbCommandAttribute>() as Attribute ?? type.GetCustomAttribute<NsbEventAttribute>();
      if (routingInfo is null)
      {
        continue;
      }

      switch (routingInfo)
      {
        case NsbCommandAttribute command:
          settings.RouteToEndpoint(type, command.Recipient);
          break;
        case NsbEventAttribute @event:
          settings.RegisterPublisher(type, @event.Publisher);
          break;
      }
    }
  }

  private static void ConfigurePersistence(EndpointConfiguration config, string connectionString, string nsbSchema)
  {
    var persistence = config.UsePersistence<SqlPersistence>();
    persistence
      .ConnectionBuilder(() => new SqlConnection(connectionString));
    var sqlSettings = persistence
      .SqlDialect<SqlDialect.MsSqlServer>();
    
    if (!string.IsNullOrWhiteSpace(nsbSchema))
    {
      sqlSettings.Schema(nsbSchema);
    }
  }

  #endregion    
}
using Microsoft.Extensions.Logging;
using NsbRouterPlayground.Common.Messages.Events;
using NServiceBus;

namespace NsbRouterPlayground.Notifier.Handlers;

internal class VendorCreatedHandler : IHandleMessages<VendorCreated>
{
  private readonly ILogger<VendorCreatedHandler> _logger;

  public VendorCreatedHandler(ILogger<VendorCreatedHandler> logger)
  {
    _logger = logger;
  }
  
  /// <inheritdoc />
  public Task Handle(VendorCreated message, IMessageHandlerContext context)
  {
    _logger.LogInformation("Processing {MessageType} for {OrderId}", nameof(VendorCreated), message.Id);
    return Task.CompletedTask;
  }
}
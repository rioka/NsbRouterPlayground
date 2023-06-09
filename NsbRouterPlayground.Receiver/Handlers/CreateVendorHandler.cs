using Microsoft.Extensions.Logging;
using NsbRouterPlayground.Common.Messages;
using NsbRouterPlayground.Common.Messages.Commands;
using NsbRouterPlayground.Common.Messages.Events;
using NServiceBus;

namespace NsbRouterPlayground.Receiver.Handlers;

internal class CreateVendorHandler : IHandleMessages<CreateVendor>
{
  private readonly ILogger<CreateVendorHandler> _logger;

  public CreateVendorHandler(ILogger<CreateVendorHandler> logger)
  {
    _logger = logger;
  }
  
  /// <inheritdoc />
  public async Task Handle(CreateVendor message, IMessageHandlerContext context)
  {
    _logger.LogInformation("Processing {MessageType} for {VendorId}", nameof(CreateVendor), message.Id);

    await context.Publish(new VendorCreated() {
      Id = message.Id
    });
    await context.Reply(new CreateVendorResponse() {
      Id = message.Id,
      Notes = $"Created at {DateTime.UtcNow}"
    });
  }
}
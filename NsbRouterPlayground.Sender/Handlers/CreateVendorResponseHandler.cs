using Microsoft.Extensions.Logging;
using NsbRouterPlayground.Common.Messages;
using NServiceBus;

namespace NsbRouterPlayground.Sender.Handlers;

internal class CreateVendorResponseHandler : IHandleMessages<CreateVendorResponse>
{
  private readonly ILogger<CreateVendorResponseHandler> _logger;

  public CreateVendorResponseHandler(ILogger<CreateVendorResponseHandler> logger)
  {
    _logger = logger;
  }
  
  public Task Handle(CreateVendorResponse message, IMessageHandlerContext context)
  {
    _logger.LogInformation("Processing {MessageType} for {VendorId}", nameof(CreateVendorResponse), message.Id);

    Console.WriteLine($"Order {message.Id} was created{Environment.NewLine}\t{message.Notes ?? "<No notes found>"}");
    
    return Task.CompletedTask;
  }
}
using NsbRouterPlayground.Common.Attributes;

namespace NsbRouterPlayground.Common.Messages.Commands; 

[NsbCommand(Endpoints.Receiver)]
public class CreateVendor {

  public Guid Id { get; set; }
}
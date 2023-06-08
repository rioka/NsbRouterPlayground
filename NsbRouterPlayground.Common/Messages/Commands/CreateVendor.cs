using NsbRouterPlayground.Common.Attributes;

namespace NsbRouterPlayground.Common.Messages.Commands; 

[NsbCommand("Processor")]
public class CreateVendor {

  public Guid Id { get; set; }
}
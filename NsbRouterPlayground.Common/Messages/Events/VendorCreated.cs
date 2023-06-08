using NsbRouterPlayground.Common.Attributes;

namespace NsbRouterPlayground.Common.Messages.Events; 

[NsbEvent("Processor")]
public class VendorCreated {

  public Guid Uid { get; set; }

  public DateTime CreatedAt { get; set; }
}
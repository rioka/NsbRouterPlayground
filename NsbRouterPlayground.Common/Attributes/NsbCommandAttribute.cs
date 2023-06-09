namespace NsbRouterPlayground.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NsbCommandAttribute : Attribute
{
  public string Recipient { get; }

  public NsbCommandAttribute(string recipient)
  {
    Recipient = recipient;
  }
}
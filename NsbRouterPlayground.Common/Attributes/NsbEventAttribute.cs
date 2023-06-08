namespace NsbRouterPlayground.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NsbEventAttribute : Attribute
{
  public string Publisher { get; }

  public NsbEventAttribute(string publisher)
  {
    Publisher = publisher;
  }
}
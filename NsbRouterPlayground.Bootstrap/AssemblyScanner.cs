using System.Reflection;
using NServiceBus;

namespace NsbRouterPlayground.Bootstrap;

public class AssemblyScanner
{
  /// <summary>
  /// Scan <paramref name="assembly"/> searching for types a <see cref="IHandleMessages{T}"/> if found for. 
  /// </summary>
  /// <param name="assembly">An assembly</param>
  /// <returns>A list of types</returns>
  public static IEnumerable<Type> GetHandledMessages(Assembly assembly)
  {
    return assembly
      .GetTypes()
      // get concrete types implementing some interfaces
      .Where(t => !t.IsAbstract)
      .SelectMany(t => t.GetInterfaces())
      // consider only IHandleMessages<>
      .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
      // get the type parameter in IHandleMessages<>
      .Select(i => i.GenericTypeArguments[0])
      .Distinct();
  }
}
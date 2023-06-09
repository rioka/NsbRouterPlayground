TODO

Add details about how events are routed, and how the router knows which endpoints an event must be forwarded to.

Relevant parts of the code

- `SqlServerTransportInfrastructure`
  - method `ConfigureSendInfrastructure`, which uses `CachedSubscriptionStore`

It is not immediately clear how the router knows that; what I've understood so far (to be verified) is that the router leverage transport built-in features, via `InitializableRawEndpoint.Initialize`. The full chain of calls that leads to that point is (TODO: capture it again and save it here) 
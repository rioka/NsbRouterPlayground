using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus.Router;

namespace NsbRouterPlayground.Router.Internals;

internal class RouterHostedService : IHostedService
{
  private readonly IRouter _router;
  private readonly ILogger<RouterHostedService> _logger;

  public RouterHostedService(IRouter router, ILogger<RouterHostedService> logger)
  {
    _router = router;
    _logger = logger;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting router");
    await _router.Start().ConfigureAwait(false);
    _logger.LogInformation("Router started");
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Stopping router");
    await _router.Stop().ConfigureAwait(false);
    _logger.LogInformation("Router stopped");
  }
}
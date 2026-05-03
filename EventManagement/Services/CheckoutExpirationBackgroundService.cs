using EventManagement.Infrastructures;
using EventManagement.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace EventManagement.Services;

public class CheckoutExpirationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CheckoutOptions _options;

    public CheckoutExpirationBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<CheckoutOptions> options
    )
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = Math.Max(_options.ExpirationSweepIntervalSeconds, 1);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var checkoutService = scope.ServiceProvider.GetRequiredService<ICheckoutService>();
                await checkoutService.ProcessExpiredCheckoutsAsync(stoppingToken);
            }
            catch { }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}

using Carpool.Core.Interfaces.Services;

namespace Carpool.API.BackgroundServices;

/// <summary>
/// Hosted service that drives the time-based ride status transitions (spec §15, §16).
///
/// A <see cref="BackgroundService"/> is effectively a singleton, so it never holds a scoped
/// <c>DbContext</c>: every tick opens a fresh DI scope via <see cref="IServiceScopeFactory"/>
/// and resolves <see cref="IRideStatusService"/> inside it. The interval comes from
/// <c>RideStatusAutomation:IntervalSeconds</c> (default 30, floor 5). A failing tick is
/// logged and retried on the next interval; it never stops the loop.
/// </summary>
public sealed class RideStatusBackgroundService : BackgroundService
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RideStatusBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public RideStatusBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RideStatusBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var seconds = configuration.GetValue<int?>("RideStatusAutomation:IntervalSeconds") ?? 30;
        _interval = TimeSpan.FromSeconds(Math.Max(seconds, MinimumInterval.TotalSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Ride status automation started; interval {IntervalSeconds}s.", _interval.TotalSeconds);

        using var timer = new PeriodicTimer(_interval);

        try
        {
            do
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogError(exception, "Ride status automation tick failed; retrying next interval.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }

        _logger.LogInformation("Ride status automation stopped.");
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        // A fresh scope every tick — never hold a scoped DbContext in this singleton (spec §16).
        using var scope = _scopeFactory.CreateScope();
        var rideStatusService = scope.ServiceProvider.GetRequiredService<IRideStatusService>();

        var started = await rideStatusService.StartDueRidesAsync(cancellationToken);
        var completed = await rideStatusService.CompleteDueRidesAsync(cancellationToken);

        if (started > 0 || completed > 0)
        {
            _logger.LogInformation(
                "Ride status automation: {Started} ride(s) started, {Completed} ride(s) completed.",
                started, completed);
        }
    }
}

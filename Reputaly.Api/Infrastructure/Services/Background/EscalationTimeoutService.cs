using Reputaly.API.Infrastructure.Services.Background;

namespace Reputaly.API.Infrastructure.Services.Background;

public class EscalationTimeoutService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EscalationTimeoutService> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public EscalationTimeoutService(
        IServiceScopeFactory scopeFactory,
        ILogger<EscalationTimeoutService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Cada ejecución, su propio scope (el servicio es singleton,
                // el processor y el DbContext son scoped).
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider
                    .GetRequiredService<IEscalationTimeoutProcessor>();

                await processor.ProcessTimeoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EscalationTimeoutService: error en la revisión");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}

using Microsoft.Extensions.Hosting;

namespace Sales123.Sales.WebApi.Support;

public sealed class MetricsSummaryHostedService : BackgroundService
{
    private readonly RequestMetrics _metrics;
    private readonly ILogger<MetricsSummaryHostedService> _logger;
    private readonly int _intervalSec;
    private readonly int _alertP99Ms;

    public MetricsSummaryHostedService(
        RequestMetrics metrics,
        ILogger<MetricsSummaryHostedService> logger,
        IConfiguration cfg)
    {
        _metrics = metrics;
        _logger = logger;
        _intervalSec = int.TryParse(cfg["OBS_SUMMARY_SEC"], out var s) && s > 0 ? s : 60;
        _alertP99Ms = int.TryParse(cfg["OBS_ALERT_P99_MS"], out var a) && a > 0 ? a : 1000;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      
        await Task.Delay(TimeSpan.FromSeconds(Math.Min(10, _intervalSec)), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var (total, e4, e5) = _metrics.Totals();
                var top = _metrics.TopRoutesByP95(3);

              
                var topFmt = string.Join(" ; ",
                    top.Select(r => $"{r.Route} p95={r.P95Ms} p99={r.P99Ms} ({r.Count})"));

                _logger.LogInformation("Metrics window={Window}s total={Total} 4xx={E4} 5xx={E5} | top={Top}",
                    _intervalSec, total, e4, e5, topFmt);

            
                foreach (var r in top.Where(r => r.P99Ms >= _alertP99Ms))
                {
                    _logger.LogWarning("Metrics alert p99Ms={P99} route={Route} (count={Count}) threshold={Threshold}",
                        r.P99Ms, r.Route, r.Count, _alertP99Ms);
                }

                _metrics.ResetWindow();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Metrics summary failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSec), stoppingToken);
        }
    }
}

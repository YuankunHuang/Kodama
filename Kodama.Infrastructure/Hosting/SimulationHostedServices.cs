using Kodama.Application.Interfaces;
using Kodama.Shared.DTOs;
using Microsoft.Extensions.Hosting;

namespace Kodama.Infrastructure.Hosting;

public class SimulationHostedServices : BackgroundService
{
    private readonly ISimulationLoop _simulationLoop;
    private readonly IGameBroadcaster _broadcaster;
    private readonly ISimulationAnalytics _analytics;
    private readonly IAnalyticsReporter _analyticsReporter;
    
    // Time scale: 1.0 = normal, 2.0 = 2x speed, 0.5 = half speed
    private static float _timeScale = 1.0f;
    private static bool _isPaused;
    private const int BaseTickInterval = 100; // ms
    private const long ReportPeriod = 60000; // ms
    private readonly List<Task> _tasks = new(5);
    private long _nextReportTime;
    
    public static bool IsPaused
    {
        get => _isPaused;
        set => _isPaused = value;
    }
    
    public static float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Math.Clamp(value, 0.1f, 10f);
    }

    public SimulationHostedServices(ISimulationLoop simulationLoop, IGameBroadcaster broadcaster, ISimulationAnalytics analytics, IAnalyticsReporter analyticsReporter)
    {
        _simulationLoop = simulationLoop;
        _broadcaster = broadcaster;
        _analytics = analytics;
        _analyticsReporter = analyticsReporter;
    }
    
    private async Task ExportReportsAsync(AnalyticsReport report)
    {
        try
        {
            // Output to zip root (parent of Backend/) or fallback to BaseDirectory
            var baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var zipRoot = Directory.GetParent(baseDir)?.FullName ?? baseDir;
            var reportsDir = Path.Combine(zipRoot, "reports");
            Directory.CreateDirectory(reportsDir);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            
            await Task.WhenAll(
                File.WriteAllTextAsync(Path.Combine(reportsDir, $"report-{timestamp}.json"), _analyticsReporter.ExportJson(report)),
                File.WriteAllTextAsync(Path.Combine(reportsDir, $"report-{timestamp}.csv"), _analyticsReporter.ExportCsv(report)),
                File.WriteAllTextAsync(Path.Combine(reportsDir, $"report-{timestamp}.md"), _analyticsReporter.ExportMarkdown(report))
            );
            
            Console.WriteLine($"[Analytics] Reports exported to: {reportsDir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analytics] Failed to export reports: {ex.Message}");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const float maxDelayTolerance = 1000f;
        var nextTickTime = DateTime.UtcNow;
        _nextReportTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ReportPeriod;
        DateTime? pauseStartTime = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            // Handle pause state
            if (_isPaused)
            {
                // Record when pause started (to recover later)
                pauseStartTime ??= DateTime.UtcNow;
                _simulationLoop.SetTimeScale(0f);
                await Task.Delay(BaseTickInterval, stoppingToken);
                continue;
            }
            
            // Compensate timing after resume
            if (pauseStartTime.HasValue)
            {
                var pauseDuration = DateTime.UtcNow - pauseStartTime.Value;
                nextTickTime += pauseDuration;
                pauseStartTime = null;
            }
            
            // Adjust tick interval based on time scale
            float intervalMs = BaseTickInterval / _timeScale;
            float deltaTime = BaseTickInterval / 1000f;
            
            var interval = TimeSpan.FromMilliseconds(intervalMs);
            nextTickTime += interval;
            
            _simulationLoop.SetTimeScale(_timeScale);
            var snapshot = _simulationLoop.Tick(deltaTime);

            _tasks.Clear();
            _tasks.Add(_broadcaster.BroadcastToAllClients(snapshot));
            
            await Task.WhenAll(_tasks);
            
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= _nextReportTime)
            {
                var report = _analytics.GenerateReport();
                _analyticsReporter.PrintToConsole(report);
                
                // Export reports to files (fire-and-forget, non-blocking)
                _ = ExportReportsAsync(report);
                
                _nextReportTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ReportPeriod;
            }
            
            var delay = nextTickTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }
            else
            {
                if (delay < TimeSpan.FromMilliseconds(-maxDelayTolerance))
                {
                    nextTickTime = DateTime.UtcNow;
                    Console.WriteLine($"[Warning] Simulation fell behind, resetting time.");
                }
            }
        }
    }
}
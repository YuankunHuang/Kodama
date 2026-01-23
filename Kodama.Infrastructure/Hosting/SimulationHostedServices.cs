using Kodama.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Kodama.Infrastructure.Hosting;

public class SimulationHostedServices : BackgroundService
{
    private readonly ISimulationLoop _simulationLoop;
    private readonly IGameBroadcaster _broadcaster;
    
    // Time scale: 1.0 = normal, 2.0 = 2x speed, 0.5 = half speed
    private static float _timeScale = 1.0f;
    private const float BaseTickInterval = 100f; // ms
    
    public static float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Math.Clamp(value, 0.1f, 10f);
    }

    public SimulationHostedServices(ISimulationLoop simulationLoop, IGameBroadcaster broadcaster)
    {
        _simulationLoop = simulationLoop;
        _broadcaster = broadcaster;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const float maxDelayTolerance = 1000f;
        var nextTickTime = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            // Adjust tick interval based on time scale
            float intervalMs = BaseTickInterval / _timeScale;
            float deltaTime = BaseTickInterval / 1000f; // Always simulate 100ms of game time per tick
            
            var interval = TimeSpan.FromMilliseconds(intervalMs);
            nextTickTime += interval;
            
            _simulationLoop.SetTimeScale(_timeScale);
            var snapshot = _simulationLoop.Tick(deltaTime);
            await _broadcaster.BroadcastToAllClients(snapshot);
            
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
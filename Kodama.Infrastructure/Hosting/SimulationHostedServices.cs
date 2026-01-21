using Kodama.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Kodama.Infrastructure.Hosting;

public class SimulationHostedServices : BackgroundService
{
    private readonly ISimulationLoop _simulationLoop;
    private readonly IGameBroadcaster _broadcaster;

    public SimulationHostedServices(ISimulationLoop simulationLoop, IGameBroadcaster broadcaster)
    {
        _simulationLoop = simulationLoop;
        _broadcaster = broadcaster;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const float deltaTime = 0.1f;
        const float maxDelayTolerance = 1000f;
        TimeSpan interval = TimeSpan.FromMilliseconds(100);
        var nextTickTime = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            nextTickTime += interval;
            var snapshot = _simulationLoop.Tick(deltaTime);
            await _broadcaster.BroadcastToAllClients(snapshot);
            var delay = nextTickTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }
            else
            {
                // delayed...
                if (delay < TimeSpan.FromMilliseconds(-maxDelayTolerance)) // too much! can't catch up. reset tick time
                {
                    nextTickTime = DateTime.UtcNow;
                    Console.WriteLine($"[Warning] Simulation fell behind, resetting time.");
                }
            }
        }
    }
}
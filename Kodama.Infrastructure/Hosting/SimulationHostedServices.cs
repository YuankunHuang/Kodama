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

        while (!stoppingToken.IsCancellationRequested)
        {
            var snapshot = _simulationLoop.Tick(deltaTime);
            await _broadcaster.BroadcastToAllClients(snapshot);
            await Task.Delay(100, stoppingToken);
        }
    }
}
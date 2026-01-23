using Kodama.Application.Interfaces;
using Kodama.Infrastructure.Hosting;
using Microsoft.AspNetCore.SignalR;

namespace Kodama.Infrastructure.Hubs;

public class GameHub : Hub<IGameClient>
{
    private readonly ISimulationLoop _simulationLoop;

    public GameHub(ISimulationLoop simulationLoop)
    {
        _simulationLoop = simulationLoop;
    }

    public void SetTimeScale(float scale)
    {
        SimulationHostedServices.TimeScale = scale;
        Console.WriteLine($"[GameHub] TimeScale set to {SimulationHostedServices.TimeScale:F1}x");
    }
    
    public float GetTimeScale()
    {
        return SimulationHostedServices.TimeScale;
    }

    public void Restart()
    {
        _simulationLoop.Restart();
        Console.WriteLine($"[GameHub] Simulation restarted by client");
    }
}

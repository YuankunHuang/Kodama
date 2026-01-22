using Kodama.Infrastructure.Hosting;
using Microsoft.AspNetCore.SignalR;

namespace Kodama.Infrastructure.Hubs;

public class GameHub : Hub<IGameClient>
{
    public void SetTimeScale(float scale)
    {
        SimulationHostedServices.TimeScale = scale;
        Console.WriteLine($"[GameHub] TimeScale set to {SimulationHostedServices.TimeScale:F1}x");
    }
    
    public float GetTimeScale()
    {
        return SimulationHostedServices.TimeScale;
    }
}

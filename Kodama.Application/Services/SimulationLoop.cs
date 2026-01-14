using Kodama.Shared.DTOs;
using Kodama.Application.Interfaces;

namespace Kodama.Application.Services;

public class SimulationLoop : ISimulationLoop
{
    private readonly Guid _agentId = Guid.NewGuid();
    private float _angle = 0f;

    private const float Radius = 5f;
    private const float AngularSpeed = 1f;
    private const float CenterX = 0;
    private const float CenterY = 0;

    public SnapshotData Tick(float deltaTime)
    {
        _angle += AngularSpeed * deltaTime;
        float x = CenterX + Radius * MathF.Cos(_angle);
        float y = CenterY + Radius * MathF.Sin(_angle);
        var agent = new AgentSnapshot(_agentId, x, y);
        var snapShotData = new SnapshotData(
            new AgentSnapshot[] {agent},
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        );
        return snapShotData;
    }
}
using Kodama.Shared.DTOs;
using Kodama.Application.Interfaces;
using Kodama.Application.States;
using Kodama.Domain.Entities;
using Kodama.Domain.ValueObjects;

namespace Kodama.Application.Services;

public class SimulationLoop : ISimulationLoop
{
    private readonly WorldState _worldState;
    private readonly AgentBehaviourService _agentBehaviourService;

    public SimulationLoop(WorldState worldState, AgentBehaviourService agentBehaviourService)
    {
        _worldState = worldState;
        _agentBehaviourService = agentBehaviourService;

        InitializeWorld();
    }

    private void InitializeWorld()
    {
        // Tree is already created in WorldState at (0,0)
        var treePos = _worldState.Tree.Position;

        // Create 3 Agents at Tree position
        for (int i = 0; i < 3; i++)
        {
            var agent = Agent.Create(treePos);
            _worldState.SetAgent(agent);
        }

        // Create Resources at various positions
        var resourcePositions = new[]
        {
            new Position(3, -3),   // top-right
            new Position(-3, 3),   // bottom-left
            new Position(4, 0),    // right
            new Position(-4, 0),   // left
            new Position(0, 4),    // bottom
            new Position(0, -4),   // top
        };

        foreach (var pos in resourcePositions)
        {
            var resource = Resource.Create(Guid.NewGuid(), pos, amount: 100);
            _worldState.SetResource(resource);
        }

        Console.WriteLine($"[SimulationLoop] World initialized: {_worldState.GetAgentCount()} agents, {_worldState.GetResourceCount()} resources");
    }

    public SnapshotData Tick(float deltaTime)
    {
        List<Guid>? toRemove = null;

        foreach (var agent in _worldState.GetAllAgents())
        {
            _agentBehaviourService.Process(agent, _worldState, deltaTime);
            if (agent.State == Domain.Enums.AgentState.Dead)
            {
                toRemove ??= new List<Guid>();
                toRemove.Add(agent.Id);
            }
        }

        if (toRemove != null)
        {
            foreach (var id in toRemove)
            {
                _worldState.RemoveAgent(id);
            }
        }

        return GenerateSnapshot();
    }

    private SnapshotData GenerateSnapshot()
    {
        var agentSnapshots = new AgentSnapshot[_worldState.GetAgentCount()];
        var i = 0;
        foreach (var agent in _worldState.GetAllAgents())
        {
            var agentSnapshot = new AgentSnapshot()
            {
                Id = agent.Id,
                Q = agent.CurrentPosition.Q,
                R = agent.CurrentPosition.R,
            };
            agentSnapshots[i++] = agentSnapshot;

            Console.WriteLine($"Id: {agentSnapshot.Id} | Q: {agentSnapshot.Q} | R: {agentSnapshot.R}");
        }

        var snapShotData = new SnapshotData()
        {
            Agents = agentSnapshots,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
        return snapShotData;
    }
}
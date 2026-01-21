using Kodama.Shared.DTOs;
using Kodama.Application.Interfaces;
using Kodama.Application.States;
using Kodama.Domain.Entities;
using Kodama.Domain.ValueObjects;
using System.Diagnostics;

namespace Kodama.Application.Services;

public class SimulationLoop : ISimulationLoop
{
    private readonly WorldState _worldState;
    private readonly AgentBehaviourService _agentBehaviourService;
    private readonly List<AgentSnapshot> _agentSnapshots;
    private readonly List<Guid> _agentsToRemove;

    private const int InitialAgentCount = 10000;

    public SimulationLoop(WorldState worldState, AgentBehaviourService agentBehaviourService)
    {
        _worldState = worldState;
        _agentBehaviourService = agentBehaviourService;
        _agentSnapshots = new(InitialAgentCount);
        _agentsToRemove = new(128);

        InitializeWorld();
    }

    private void InitializeWorld()
    {
        // Tree is already created in WorldState at (0,0)
        var treePos = _worldState.Tree.Position;

        // Create 3 Agents at Tree position
        for (int i = 0; i < InitialAgentCount; i++)
        {
            var agent = Agent.Create(treePos);
            _worldState.SetAgent(agent);
        }

        // Create Resources at various positions (pointy-top)
        var radius = 8;
        var resourcePositions = new Position[]
        {
            new (radius, -radius), // top right
            new (radius, 0), // right
            new (0, radius), // bottom right
            new (-radius, radius), // bottom left
            new (-radius, 0), // left
            new (0, -radius), // top left
        };

        foreach (var pos in resourcePositions)
        {
            var resource = Resource.Create(Guid.NewGuid(), pos, amount: long.MaxValue);
            _worldState.SetResource(resource);
        }

        Console.WriteLine($"[SimulationLoop] World initialized: {_worldState.GetAgentCount()} agents, {_worldState.GetResourceCount()} resources");
    }

    public SnapshotData Tick(float deltaTime)
    {
        var sw = Stopwatch.StartNew();

        _agentsToRemove.Clear();

        foreach (var agent in _worldState.GetAllAgents())
        {
            _agentBehaviourService.Process(agent, _worldState, deltaTime);
            if (agent.State == Domain.Enums.AgentState.Dead)
            {
                _agentsToRemove.Add(agent.Id);
            }
        }

        if (_agentsToRemove.Count > 0)
        {
            foreach (var id in _agentsToRemove)
            {
                _worldState.RemoveAgent(id);
            }
        }

        var snapshotData = GenerateSnapshot();

        sw.Stop();
        Console.WriteLine($"Tick Time: {sw.Elapsed.TotalMilliseconds:F2}ms");

        return snapshotData;
    }

    private SnapshotData GenerateSnapshot()
    {
        _agentSnapshots.Clear();
        foreach (var agent in _worldState.GetAllAgents())
        {
            var agentSnapshot = new AgentSnapshot()
            {
                Id = agent.Id,
                Q = agent.CurrentPosition.Q,
                R = agent.CurrentPosition.R,
            };
            _agentSnapshots.Add(agentSnapshot);
        }

        var snapShotData = new SnapshotData()
        {
            Agents = _agentSnapshots,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
        return snapShotData;
    }
}
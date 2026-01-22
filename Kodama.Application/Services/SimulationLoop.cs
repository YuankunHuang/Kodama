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
    private readonly List<ResourceSnapshot> _resourceSnapshots;
    private readonly List<int> _agentsToRemove;
    private readonly Stopwatch _stopwatch;

    // Configurable: Agent count determines map scale
    private const int InitialAgentCount = 10000;
    private const int ResourcesPerRing = 12; // Resources per radius ring
    private const int MapRadius = 50; // Max hex distance from center

    public SimulationLoop(WorldState worldState, AgentBehaviourService agentBehaviourService)
    {
        _worldState = worldState;
        _agentBehaviourService = agentBehaviourService;
        _agentSnapshots = new(InitialAgentCount);
        _resourceSnapshots = new(ResourcesPerRing * MapRadius);
        _agentsToRemove = new(128);
        _stopwatch = new();

        InitializeWorld();
    }

    private void InitializeWorld()
    {
        var treePos = _worldState.Tree.Position;

        // Spawn agents at tree position (center of the world)
        for (int i = 0; i < InitialAgentCount; i++)
        {
            var agent = Agent.Create(_worldState.AllocateAgentId(), treePos);
            _worldState.SetAgent(agent);
        }

        // Generate resources in hexagonal ring pattern
        // Each ring is a perfect hexagon at distance 'radius' from center
        const long AmountPerResource = 10000; // Large amount so resources don't deplete quickly

        for (var radius = 8; radius <= MapRadius; radius += 3)
        {
            // Hexagonal ring has exactly 6*radius positions
            var totalPositionsInRing = radius * 6;
            
            // Scale resource count with ring size (bigger rings = more resources)
            var resourceCount = Math.Max(6, radius / 2);
            
            // Distribute resources evenly around the ring
            for (var i = 0; i < resourceCount; i++)
            {
                // Evenly sample positions around the ring
                var ringIndex = (totalPositionsInRing * i) / resourceCount;
                
                Position resourcePos = GetHexRingPosition(radius, ringIndex);
                var resource = Resource.Create(_worldState.AllocateResourceId(), resourcePos, AmountPerResource);
                
                _worldState.SetResource(resource);
            }
        }

        Console.WriteLine($"[SimulationLoop] World initialized: {_worldState.GetAgentCount()} agents, {_worldState.GetResourceCount()} resources");
    }

    private Position GetHexRingPosition(int radius, int index)
    {
        // Pointy-top hex
        (int dq, int dr)[] directions = 
        {
            (1, 0),   // Right
            (0, 1),   // BottomRight
            (-1, 1),  // BottomLeft
            (-1, 0),  // Left
            (0, -1),  // TopLeft
            (1, -1)   // TopRight
        };
        
        int side = index / radius;
        int offset = index % radius;
        
        if (side >= 6) side = 5; // protect bound
        
        int q = directions[side].dq * radius;
        int r = directions[side].dr * radius;
        
        int nextSide = (side + 2) % 6;
        q += directions[nextSide].dq * offset;
        r += directions[nextSide].dr * offset;
        
        return new Position(q, r);
    }

    public SnapshotData Tick(float deltaTime)
    {
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        _stopwatch.Restart();

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
        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        var allocated = allocatedAfter - allocatedBefore;

        Console.WriteLine($"{_worldState.GetAgentCount()} agents | TickTime: {_stopwatch.ElapsedMilliseconds:F2}ms | Alloc: {allocated} bytes");

        return snapshotData;
    }

    private SnapshotData GenerateSnapshot()
    {
        // Agents
        _agentSnapshots.Clear();
        foreach (var agent in _worldState.GetAllAgents())
        {
            _agentSnapshots.Add(new AgentSnapshot
            {
                Id = agent.Id,
                Q = agent.CurrentPosition.Q,
                R = agent.CurrentPosition.R,
            });
        }

        // Tree
        var tree = _worldState.Tree;
        var treeSnapshot = new TreeSnapshot
        {
            Id = tree.Id,
            Q = tree.Position.Q,
            R = tree.Position.R,
        };

        // Resources
        _resourceSnapshots.Clear();
        foreach (var resource in _worldState.GetAllResources())
        {
            _resourceSnapshots.Add(new ResourceSnapshot
            {
                Id = resource.Id,
                Q = resource.Position.Q,
                R = resource.Position.R,
            });
        }

        return new SnapshotData
        {
            Agents = _agentSnapshots,
            Tree = treeSnapshot,
            Resources = _resourceSnapshots,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
    }
}
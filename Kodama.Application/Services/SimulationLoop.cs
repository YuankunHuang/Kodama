using Kodama.Shared.DTOs;
using Kodama.Application.Interfaces;
using Kodama.Application.States;
using Kodama.Domain.Entities;
using Kodama.Domain.Enums;
using Kodama.Domain.ValueObjects;
using System.Diagnostics;

namespace Kodama.Application.Services;

public class SimulationLoop : ISimulationLoop
{
    private readonly WorldState _worldState;
    private readonly AgentBehaviourSystem _agentBehaviourSystem;
    private readonly List<AgentSnapshot> _agentSnapshots;
    private readonly List<ResourceSnapshot> _resourceSnapshots;
    private readonly Stopwatch _stopwatch;
    private readonly ISimulationAnalytics _analytics;

    // Configurable: Agent count determines map scale
    private const int InitialAgentCount = WorldState.MaxAgents;
    private const int ResourcesPerRing = 12; // Resources per radius ring
    private const int MapRadius = 50; // Max hex distance from center

    public SimulationLoop(WorldState worldState, AgentBehaviourSystem agentBehaviourSystem, ISimulationAnalytics analytics)
    {
        _worldState = worldState;
        _agentBehaviourSystem = agentBehaviourSystem;
        _analytics = analytics;

        _agentSnapshots = new(InitialAgentCount);
        _resourceSnapshots = new(ResourcesPerRing * MapRadius);
        _stopwatch = new();

        InitializeWorld();
    }

    private void InitializeWorld()
    {
        var treePos = _worldState.Tree.Position;

        // Spawn agents at tree position (center of the world)
        for (int i = 0; i < InitialAgentCount; i++)
        {
            _worldState.Agents.Add(treePos);
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

    // Store last tick metrics for stats
    private float _lastTickTimeMs;
    private long _lastAllocBytes;
    private float _timeScale = 1.0f;
    
    public void SetTimeScale(float scale) => _timeScale = scale;

    public void Restart()
    {
        // Clear existing state
        _worldState.Clear();
        
        // Re-initialize the world
        InitializeWorld();
        
        Console.WriteLine("[SimulationLoop] Simulation restarted!");
    }

    public SnapshotData Tick(float deltaTime)
    {
        _analytics.Tick(deltaTime);
        
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        _stopwatch.Restart();

        _agentBehaviourSystem.Tick(_worldState.Agents, _worldState, deltaTime);

        _stopwatch.Stop();
        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        
        _lastTickTimeMs = (float)_stopwatch.Elapsed.TotalMilliseconds;
        _lastAllocBytes = allocatedAfter - allocatedBefore;

        var snapshotData = GenerateSnapshot();

        if (Environment.GetEnvironmentVariable("KODAMA_TICK_LOG") == "1")
        {
            Console.WriteLine($"{_worldState.GetAgentCount()} agents | Idle:{_worldState.Agents.GetStateCount(AgentState.Idle)} Find:{_worldState.Agents.GetStateCount(AgentState.FindingResource)} Move:{_worldState.Agents.GetStateCount(AgentState.MovingToResource)} Col:{_worldState.Agents.GetStateCount(AgentState.Collecting)} Ret:{_worldState.Agents.GetStateCount(AgentState.ReturningToBase)} Dep:{_worldState.Agents.GetStateCount(AgentState.Depositing)} | Tree:{_worldState.Tree.Matter} | TickTime: {_lastTickTimeMs:F2}ms | Alloc: {_lastAllocBytes} bytes");
        }

        return snapshotData;
    }

    private SnapshotData GenerateSnapshot()
    {
        var store = _worldState.Agents;

        // Agents — iterate the live dense set; state counts come from the
        // per-state sets in O(1) instead of a per-agent switch.
        _agentSnapshots.Clear();
        foreach (var id in store.LiveAgents)
        {
            _agentSnapshots.Add(new AgentSnapshot
            {
                Id = id,
                Q = store.Q[id],
                R = store.R[id],
                State = (byte)store.States[id],
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
        int resourcesOccupied = 0;
        _resourceSnapshots.Clear();
        foreach (var resource in _worldState.GetAllResources())
        {
            bool isOccupied = resource.Owner != null;
            if (isOccupied) resourcesOccupied++;
            
            _resourceSnapshots.Add(new ResourceSnapshot
            {
                Id = resource.Id,
                Q = resource.Position.Q,
                R = resource.Position.R,
                IsBeingCollected = isOccupied,
            });
        }

        // Stats
        var stats = new SimulationStats
        {
            AgentCount = store.Count,
            ResourceCount = _resourceSnapshots.Count,
            TickTimeMs = _lastTickTimeMs,
            MemoryAllocBytes = _lastAllocBytes,
            TimeScale = _timeScale,
            AgentsIdle = store.GetStateCount(AgentState.Idle),
            AgentsFinding = store.GetStateCount(AgentState.FindingResource),
            AgentsMoving = store.GetStateCount(AgentState.MovingToResource),
            AgentsCollecting = store.GetStateCount(AgentState.Collecting),
            AgentsReturning = store.GetStateCount(AgentState.ReturningToBase),
            AgentsDepositing = store.GetStateCount(AgentState.Depositing),
            TreeEnergy = tree.Matter,
            ResourcesOccupied = resourcesOccupied,
            ResourcesAvailable = _resourceSnapshots.Count - resourcesOccupied,
        };

        return new SnapshotData
        {
            Agents = _agentSnapshots,
            Tree = treeSnapshot,
            Resources = _resourceSnapshots,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Stats = stats,
        };
    }
}

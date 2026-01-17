using Kodama.Domain.Entities;
using Kodama.Domain.ValueObjects;

namespace Kodama.Application.States;

public class WorldState
{
    public Tree Tree { get; private set; } = Tree.Create(Guid.NewGuid(), new Position(0, 0), 0);

    // id
    private readonly Dictionary<Guid, Agent> _agents = new();
    private readonly Dictionary<Guid, Resource> _resources = new();
    
    // position
    private readonly Dictionary<Position, HashSet<Agent>> _agentsByPosition = new();
    private readonly Dictionary<Position, HashSet<Resource>> _resourcesByPosition = new();

    #region Getter
    public int GetAgentCount()
    {
        return _agents.Count;
    }

    public int GetResourceCount()
    {
        return _resources.Count;
    }

    public Agent? GetAgent(Guid id)
    {
        if (_agents.TryGetValue(id, out var agent))
        {
            return agent;
        }
        return null;
    }

    public Resource? GetResource(Guid id)
    {
        if (_resources.TryGetValue(id, out var resource))
        {
            return resource;
        }
        return null;
    }

    public IEnumerable<Agent> GetAllAgents() => _agents.Values;
    public IEnumerable<Resource> GetAllResources() => _resources.Values;

    public IReadOnlyCollection<Agent>? GetAgentsByPosition(Position position)
    {
        if (_agentsByPosition.TryGetValue(position, out var agents))
        {
            return agents;
        }
        return null;
    }

    public IReadOnlyCollection<Resource>? GetResourcesByPosition(Position position)
    {
        if (_resourcesByPosition.TryGetValue(position, out var resources))
        {
            return resources;
        }
        return null;
    }
    #endregion

    #region Modifier
    public void MoveAgent(Guid id, Position newPos)
    {
        var agent = GetAgent(id);
        if (agent == null)
        {
            return;
        }
        if (_agentsByPosition.TryGetValue(agent.CurrentPosition, out var oldSet))
        {
            oldSet.Remove(agent);
        }
        agent.MoveTo(newPos);
        _agents[id] = agent;
        if (!_agentsByPosition.TryGetValue(newPos, out var set))
        {
            set = new();
            _agentsByPosition[newPos] = set;
        }
        set.Add(agent);
    }

    public void SetAgent(Agent agent)
    {
        if (agent == null)
        {
            return;
        }

        if (_agents.TryGetValue(agent.Id, out var oldAgent))
        {
            if (_agentsByPosition.TryGetValue(oldAgent.CurrentPosition, out var oldSet))
            {
                oldSet.Remove(oldAgent);
            }
        }

        _agents[agent.Id] = agent;
        if (!_agentsByPosition.TryGetValue(agent.CurrentPosition, out var set))
        {
            set = new();
            _agentsByPosition[agent.CurrentPosition] = set;
        }
        set.Add(agent);
    }

    public void SetResource(Resource resource)
    {
        if (resource == null)
        {
            return;
        }

        if (_resources.TryGetValue(resource.Id, out var oldResource))
        {
            if (_resourcesByPosition.TryGetValue(oldResource.Position, out var oldSet))
            {
                oldSet.Remove(oldResource);
            }
        }

        _resources[resource.Id] = resource;
        if (!_resourcesByPosition.TryGetValue(resource.Position, out var set))
        {
            set = new();
            _resourcesByPosition[resource.Position] = set;
        }
        set.Add(resource);
    }

    public void RemoveAgent(Guid id)
    {
        if (_agents.TryGetValue(id, out var agent))
        {
            if (_agentsByPosition.TryGetValue(agent.CurrentPosition, out var set))
            {
                set.Remove(agent);
            }

            _agents.Remove(id);
        }
    }

    public void RemoveResource(Guid id)
    {
        if (_resources.TryGetValue(id, out var resource))
        {
            if (_resourcesByPosition.TryGetValue(resource.Position, out var set))
            {
                set.Remove(resource);
            }

            _resources.Remove(id);
        }
    }
    #endregion

    // BFS
    public Resource? FindNearestAvailableResource(Position from, int maxDistance = 50)
    {
        var visited = new HashSet<Position>();
        var q = new Queue<Position>();
        q.Enqueue(from);
        visited.Add(from);
        
        while (q.Count > 0)
        {
            var pos = q.Dequeue();
            if (_resourcesByPosition.TryGetValue(pos, out var resources))
            {
                foreach (var resource in resources)
                {
                    if (resource.IsAvailable)
                    {
                        return resource;
                    }
                }
            }

            foreach (var neighbour in pos.GetNeighbors())
            {
                if (from.DistanceTo(neighbour) <= maxDistance &&
                    visited.Add(neighbour))
                {
                    q.Enqueue(neighbour);
                }
            }
        }

        return null;
    }
}
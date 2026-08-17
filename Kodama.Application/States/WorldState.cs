using Kodama.Domain.Entities;
using Kodama.Domain.ValueObjects;

namespace Kodama.Application.States;

public class WorldState
{
    public const int MaxAgents = 10000;

    private int _nextResourceId = 1;

    public int AllocateResourceId() => _nextResourceId++;

    public Tree Tree { get; private set; } = Tree.Create(1, new Position(0, 0), 0);

    /// <summary>Sparse-set SoA storage for all agents (the simulation hot path).</summary>
    public AgentStore Agents { get; } = new(MaxAgents);

    private readonly HashSet<Resource> _availableResources = new();

    // id
    private readonly Dictionary<int, Resource> _resources = new();

    // position
    private readonly Dictionary<Position, HashSet<Resource>> _resourcesByPosition = new();

    // Uniform-grid spatial partition for nearest-resource queries.
    private readonly ResourceSpatialGrid _spatialGrid = new();

    #region Getter
    public HashSet<Resource> GetAvailableResources() => _availableResources;

    public int GetAgentCount() => Agents.Count;

    public int GetResourceCount()
    {
        return _resources.Count;
    }

    public Resource? GetResource(int id)
    {
        if (_resources.TryGetValue(id, out var resource))
        {
            return resource;
        }
        return null;
    }

    public Dictionary<int, Resource>.ValueCollection GetAllResources() => _resources.Values;

    public HashSet<Resource>? GetResourcesByPosition(Position position)
    {
        if (_resourcesByPosition.TryGetValue(position, out var resources))
        {
            return resources;
        }
        return null;
    }
    #endregion

    #region Modifier
    public void SetResource(Resource resource)
    {
        if (resource == null)
        {
            return;
        }

        if (_resources.TryGetValue(resource.Id, out var oldResource))
        {
            _spatialGrid.Remove(oldResource);
            if (_resourcesByPosition.TryGetValue(oldResource.Position, out var oldSet))
            {
                oldSet.Remove(oldResource);
            }
        }

        _resources[resource.Id] = resource;
        _spatialGrid.Insert(resource);
        if (!_resourcesByPosition.TryGetValue(resource.Position, out var set))
        {
            set = new();
            _resourcesByPosition[resource.Position] = set;
        }
        set.Add(resource);

        if (resource.IsAvailable)
        {
            _availableResources.Add(resource);
        }
    }

    public void RemoveResource(int id)
    {
        if (_resources.TryGetValue(id, out var resource))
        {
            _availableResources.Remove(resource);
            _spatialGrid.Remove(resource);

            if (_resourcesByPosition.TryGetValue(resource.Position, out var set))
            {
                set.Remove(resource);
            }

            _resources.Remove(id);
        }
    }
    #endregion

    public void MarkResourceUnavailable(Resource resource)
    {
        _availableResources.Remove(resource);
    }

    public void MarkResourceAvailable(Resource resource)
    {
        if (resource.IsAvailable)
        {
            _availableResources.Add(resource);
        }
    }

    public void Clear()
    {
        Agents.Clear();
        _resources.Clear();
        _resourcesByPosition.Clear();
        _spatialGrid.Clear();
        _availableResources.Clear();
        _nextResourceId = 1;
        Tree = Tree.Create(1, new Position(0, 0), 0);
    }

    public Resource? FindNearestAvailableResource(Position from)
    {
        // Uniform-grid spatial partition: walk outward in Chebyshev rings from
        // the query cell instead of scanning every available resource.
        return _spatialGrid.FindNearestAvailable(from);
    }
}

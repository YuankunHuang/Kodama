using Kodama.Domain.Enums;
using Kodama.Domain.ValueObjects;

namespace Kodama.Domain.Entities;

/// <summary>
/// Sparse-set, structure-of-arrays storage for agents (ECS-style).
/// Component arrays are indexed by entity id; the live set and per-state
/// sets are sparse sets (dense array + sparse lookup) with O(1)
/// add/remove/contains and cache-linear iteration over dense arrays.
/// </summary>
public sealed class AgentStore
{
    public const long InventoryCapacity = 100;

    // 6 sequential states (0..5) + Dead (99) mapped to slot 6.
    private const int StateSlots = 7;
    private const int DeadSlot = 6;

    private readonly int _capacity;
    private int _nextId = 1;

    // Live set (all alive agents)
    private readonly int[] _dense;   // dense index -> entity id
    private readonly int[] _sparse;  // entity id -> dense index (-1 = absent)
    public int Count { get; private set; }

    // Component arrays, indexed by entity id (ids are 1.._capacity)
    public readonly int[] Q;
    public readonly int[] R;
    public readonly AgentState[] States;
    public readonly long[] Inventories;
    public readonly int[] HarvestingResourceIds; // -1 = none

    // Per-state sparse sets
    private readonly int[][] _stateDense;
    private readonly int[][] _stateSparse;
    private readonly int[] _stateCounts;

    public AgentStore(int capacity)
    {
        _capacity = capacity;

        _dense = new int[capacity];
        _sparse = new int[capacity + 1];
        Array.Fill(_sparse, -1);

        Q = new int[capacity + 1];
        R = new int[capacity + 1];
        States = new AgentState[capacity + 1];
        Inventories = new long[capacity + 1];
        HarvestingResourceIds = new int[capacity + 1];
        Array.Fill(HarvestingResourceIds, -1);

        _stateDense = new int[StateSlots][];
        _stateSparse = new int[StateSlots][];
        _stateCounts = new int[StateSlots];
        for (var s = 0; s < StateSlots; s++)
        {
            _stateDense[s] = new int[capacity];
            _stateSparse[s] = new int[capacity + 1];
            Array.Fill(_stateSparse[s], -1);
        }
    }

    private static int StateSlot(AgentState state) =>
        state == AgentState.Dead ? DeadSlot : (int)state;

    /// <summary>Dense view of all live agent ids.</summary>
    public ReadOnlySpan<int> LiveAgents => _dense.AsSpan(0, Count);

    public bool Contains(int id) => id > 0 && id <= _capacity && _sparse[id] != -1;

    public int GetStateCount(AgentState state) => _stateCounts[StateSlot(state)];

    /// <summary>Copies the ids currently in <paramref name="state"/> into <paramref name="buffer"/> at <paramref name="offset"/>; returns the count copied.</summary>
    public int CopyState(AgentState state, int[] buffer, int offset)
    {
        var slot = StateSlot(state);
        var count = _stateCounts[slot];
        Array.Copy(_stateDense[slot], 0, buffer, offset, count);
        return count;
    }

    /// <summary>Spawns a new agent at <paramref name="position"/> in the Idle state; returns its id.</summary>
    public int Add(Position position)
    {
        if (_nextId > _capacity)
        {
            throw new InvalidOperationException($"AgentStore capacity ({_capacity}) exhausted.");
        }

        var id = _nextId++;

        _sparse[id] = Count;
        _dense[Count] = id;
        Count++;

        Q[id] = position.Q;
        R[id] = position.R;
        States[id] = AgentState.Idle;
        Inventories[id] = 0;
        HarvestingResourceIds[id] = -1;

        StateSetAdd(StateSlot(AgentState.Idle), id);
        return id;
    }

    /// <summary>Removes an agent from the live set and its state set (swap-remove).</summary>
    public void Remove(int id)
    {
        var denseIndex = _sparse[id];
        if (denseIndex == -1)
        {
            return;
        }

        StateSetRemove(StateSlot(States[id]), id);

        var lastId = _dense[Count - 1];
        _dense[denseIndex] = lastId;
        _sparse[lastId] = denseIndex;
        Count--;
        _sparse[id] = -1;
    }

    public void ChangeState(int id, AgentState newState)
    {
        var oldState = States[id];
        if (oldState == newState)
        {
            return;
        }

        StateSetRemove(StateSlot(oldState), id);
        StateSetAdd(StateSlot(newState), id);
        States[id] = newState;
    }

    public Position GetPosition(int id) => new(Q[id], R[id]);

    public void MoveTo(int id, Position position)
    {
        Q[id] = position.Q;
        R[id] = position.R;
    }

    public long GetRemainingCapacity(int id) => InventoryCapacity - Inventories[id];

    public bool IsFull(int id) => Inventories[id] >= InventoryCapacity;

    public void AddInventory(int id, long amount)
    {
        Inventories[id] = Math.Max(0, Inventories[id] + amount);
    }

    public void ClearInventory(int id) => Inventories[id] = 0;

    public void SetHarvestTarget(int id, int resourceId) => HarvestingResourceIds[id] = resourceId;

    public void ClearHarvestTarget(int id) => HarvestingResourceIds[id] = -1;

    /// <summary>Resets the store to empty (used by simulation Restart).</summary>
    public void Clear()
    {
        foreach (var id in LiveAgents)
        {
            _sparse[id] = -1;
        }
        Count = 0;
        _nextId = 1;

        for (var s = 0; s < StateSlots; s++)
        {
            for (var i = 0; i < _stateCounts[s]; i++)
            {
                _stateSparse[s][_stateDense[s][i]] = -1;
            }
            _stateCounts[s] = 0;
        }
    }

    private void StateSetAdd(int slot, int id)
    {
        var dense = _stateDense[slot];
        var sparse = _stateSparse[slot];
        sparse[id] = _stateCounts[slot];
        dense[_stateCounts[slot]] = id;
        _stateCounts[slot]++;
    }

    private void StateSetRemove(int slot, int id)
    {
        var dense = _stateDense[slot];
        var sparse = _stateSparse[slot];
        var index = sparse[id];
        if (index == -1)
        {
            return;
        }

        var lastId = dense[_stateCounts[slot] - 1];
        dense[index] = lastId;
        sparse[lastId] = index;
        _stateCounts[slot]--;
        sparse[id] = -1;
    }
}

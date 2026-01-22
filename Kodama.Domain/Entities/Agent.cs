using Kodama.Domain.Enums;
using Kodama.Domain.ValueObjects;

namespace Kodama.Domain.Entities;

public class Agent
{
    public int Id { get; private set; }
    public Position CurrentPosition { get; private set; }
    public AgentState State { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public long Inventory { get; private set; }
    public int? HarvestingResourceId { get; private set; }

    public bool IsFull => Inventory >= Capacity;

    private const long Capacity = 100;

    private Agent()
    {
        // Used by EF(EntityFramework) Core only - not exposed
    }

    // factory
    public static Agent Create(int id, Position startPosition)
    {
        return new Agent
        {
            Id = id,
            CurrentPosition = startPosition,
            State = AgentState.Idle,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void SetHarvestTarget(int resourceId)
    {
        HarvestingResourceId = resourceId;
    }

    public void ClearHarvestTarget()
    {
        HarvestingResourceId = null;
    }

    public long GetRemainingCapacity()
    {
        return Capacity - Inventory;
    }

    public void AddInventory(long amount)
    {
        Inventory = Math.Max(0, Inventory + amount);
    }

    public void ClearInventory()
    {
        Inventory = 0;
    }

    public void MoveTo(Position target)
    {
        if (State == AgentState.Dead)
        {
            throw new InvalidOperationException("Dead agent cannot move");
        }

        CurrentPosition = target;
    }

    public void ChangeState(AgentState newState)
    {
        if (State == AgentState.Dead && newState != AgentState.Dead)
        {
            throw new InvalidOperationException("Cannot revive dead agents.");
        }

        State = newState;
    }

    public void Die()
    {
        State = AgentState.Dead;
    }
}
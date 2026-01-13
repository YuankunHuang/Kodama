using Kodama.Domain.Enums;
using Kodama.Domain.ValueObjects;

namespace Kodama.Domain.Entities;

public class Agent
{
    public Guid Id { get; private set; }
    public Position CurrentPosition { get; private set; } = null!;
    public AgentState State { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Agent()
    {
        // Used by EF Core only - not exposed
    }

    // factory
    public static Agent Create(Position startPosition)
    {
        if (startPosition == null)
        {
            throw new ArgumentNullException(nameof(startPosition));
        }

        return new Agent()
        {
            Id = Guid.NewGuid(),
            CurrentPosition = startPosition,
            State = AgentState.Idle,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void MoveTo(Position target)
    {
        if (State == AgentState.Dead)
        {
            throw new InvalidOperationException("Dead agent cannot move");
        }

        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
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
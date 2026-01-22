using Kodama.Domain.ValueObjects;

namespace Kodama.Domain.Entities;

public class Resource
{
    public int Id { get; private set; }
    public Position Position { get; private set; }
    public long Amount { get; private set; }
    public int? Owner { get; private set; }

    public bool IsDepleted => Amount <= 0;
    public bool IsAvailable => Owner == null && !IsDepleted;

    private Resource()
    {
        // Used by EF Core only
    }

    public static Resource Create(int id, Position position, long amount)
    {
        return new Resource
        {
            Id = id,
            Position = position,
            Amount = amount,
            Owner = null
        };
    }

    public bool Claim(int agentId)
    {
        if (!IsAvailable)
        {
            return false;
        }
        Owner = agentId;
        return true;
    }

    public long Extract(long requested)
    {
        var actual = Math.Min(requested, Amount);
        Amount -= actual;
        return actual;
    }

    public void Release()
    {
        Owner = null;
    }
}
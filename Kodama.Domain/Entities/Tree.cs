using Kodama.Domain.ValueObjects;

namespace Kodama.Domain.Entities;

public class Tree
{
    public Guid Id { get; private set; }
    public Position Position { get; private set; }
    public long Matter { get; private set; }

    private Tree()
    {
        // Used by EF Core only
    }

    public static Tree Create(Guid id, Position position, long matter = 0)
    {
        return new Tree
        {
            Id = id,
            Position = position,
            Matter = matter
        };
    }

    public void Deposit(long amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException($"Cannot deposit {amount} into Tree.");
        }

        Matter += amount;
    }
}
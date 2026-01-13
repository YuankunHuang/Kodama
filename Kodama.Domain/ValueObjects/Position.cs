namespace Kodama.Domain.ValueObjects;

/// <summary>
/// Axial Coordinates
/// </summary>
/// https://www.redblobgames.com/grids/hexagons/#coordinates-axial
public record Position(int Q, int R)
{
    public int DistanceTo(Position other)
    {
        return (Math.Abs(Q - other.Q) + Math.Abs(Q + R - other.Q - other.R) + Math.Abs(R - other.R));
    }

    public IEnumerable<Position> GetNeighbors()
    {
        yield return new Position(Q + 1, R);
        yield return new Position(Q + 1, R - 1);
        yield return new Position(Q, R - 1);
        yield return new Position(Q - 1, R);
        yield return new Position(Q - 1, R + 1);
        yield return new Position(Q, R + 1);
    }
}
namespace Kodama.Domain.ValueObjects;

/// <summary>
/// Axial Coordinates
/// </summary>
/// https://www.redblobgames.com/grids/hexagons/#coordinates-axial
public readonly record struct Position(int Q, int R)
{
    public int DistanceTo(Position other)
    {
        // deltaQ + deltaR + deltaS (delta<Q+R>)
        return (Math.Abs(Q - other.Q) + Math.Abs(Q + R - other.Q - other.R) + Math.Abs(R - other.R));
    }

    public NeighboursEnumerator GetNeighbors() => new(this);

    public struct NeighboursEnumerator
    {
        private readonly Position _center;
        private int _index;

        public NeighboursEnumerator(Position center)
        {
            _center = center;
            _index = -1;
        }

        public Position Current => _index switch
        {
            0 => new(_center.Q + 1, _center.R),
            1 => new(_center.Q + 1, _center.R - 1),
            2 => new(_center.Q, _center.R - 1),
            3 => new(_center.Q - 1, _center.R),
            4 => new(_center.Q - 1, _center.R + 1),
            5 => new(_center.Q, _center.R + 1),
            _ => default
        };

        public bool MoveNext() => ++_index < 6;

        public NeighboursEnumerator GetEnumerator() => this;
    }
}
using Kodama.Domain.Entities;
using Kodama.Domain.ValueObjects;

namespace Kodama.Application.States;

/// <summary>
/// Uniform-grid spatial partition over hex axial coordinates, used to answer
/// "nearest available resource" without a world scan. Resources are bucketed
/// into cells of fixed size (in hex-coordinate units); a query walks outward in
/// Chebyshev rings until the ring's lower distance bound exceeds the best hit,
/// so the scan is local instead of O(all resources).
///
/// Allocation-free on the query path: cell keys are stack <c>ValueTuple</c>s,
/// buckets are plain <c>List&lt;Resource&gt;</c>, and ring iteration is nested
/// integer loops (no LINQ, no closure).
/// </summary>
public sealed class ResourceSpatialGrid
{
    private const int DefaultCellSize = 5;

    private readonly int _cellSize;
    private readonly Dictionary<(int Q, int R), List<Resource>> _cells = new();

    // Bounding box of non-empty cells (in cell coordinates). Used only to bound
    // the ring expansion so an empty (or nearly empty) grid terminates without
    // iterating to infinity. It is expanded on insert and never shrunk on
    // remove — a stale bound only enlarges the (finite) ring scan, never breaks
    // correctness.
    private int _minCQ = int.MaxValue, _maxCQ = int.MinValue;
    private int _minCR = int.MaxValue, _maxCR = int.MinValue;

    public ResourceSpatialGrid(int cellSize = DefaultCellSize)
    {
        if (cellSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell size must be >= 1.");
        }
        _cellSize = cellSize;
    }

    public int CellSize => _cellSize;

    public void Insert(Resource resource)
    {
        var (cq, cr) = CellOf(resource.Position);
        if (!_cells.TryGetValue((cq, cr), out var bucket))
        {
            bucket = new List<Resource>();
            _cells[(cq, cr)] = bucket;
        }
        bucket.Add(resource);
        ExpandBounds(cq, cr);
    }

    public void Remove(Resource resource)
    {
        var (cq, cr) = CellOf(resource.Position);
        if (_cells.TryGetValue((cq, cr), out var bucket))
        {
            bucket.Remove(resource);
            if (bucket.Count == 0)
            {
                _cells.Remove((cq, cr));
            }
        }
    }

    public Resource? FindNearestAvailable(Position from)
    {
        var (cq, cr) = CellOf(from);
        int maxRing = MaxRingFrom(cq, cr);

        int bestDist = int.MaxValue;
        Resource? best = null;

        for (int ring = 0; ring <= maxRing; ring++)
        {
            // Lower bound: a resource in Chebyshev ring `ring` is at least
            // (ring - 1) * cellSize + 1 hex units away (ring >= 1) — it must
            // cross `ring - 1` full cells, then enter the ring's cell. Since
            // Position.DistanceTo returns 2x the hex distance (it omits the /2),
            // the bound in DistanceTo units is 2 * ((ring - 1) * cellSize + 1).
            // Once that exceeds the best hit, nothing farther can win.
            if (ring >= 1 && 2 * ((ring - 1) * _cellSize + 1) > bestDist)
            {
                break;
            }

            // Enumerate the Chebyshev shell (max(|dc|,|dr|) == ring).
            for (int dc = -ring; dc <= ring; dc++)
            {
                for (int dr = -ring; dr <= ring; dr++)
                {
                    if (Math.Max(Math.Abs(dc), Math.Abs(dr)) != ring)
                    {
                        continue;
                    }
                    if (!_cells.TryGetValue((cq + dc, cr + dr), out var bucket))
                    {
                        continue;
                    }
                    foreach (var resource in bucket)
                    {
                        if (!resource.IsAvailable)
                        {
                            continue;
                        }
                        int d = from.DistanceTo(resource.Position);
                        if (d < bestDist)
                        {
                            bestDist = d;
                            best = resource;
                        }
                    }
                }
            }
        }

        return best;
    }

    public void Clear()
    {
        _cells.Clear();
        _minCQ = int.MaxValue; _maxCQ = int.MinValue;
        _minCR = int.MaxValue; _maxCR = int.MinValue;
    }

    private (int Q, int R) CellOf(Position position) =>
        (FloorDiv(position.Q, _cellSize), FloorDiv(position.R, _cellSize));

    private static int FloorDiv(int value, int divisor)
    {
        int quotient = value / divisor;
        if (value % divisor != 0 && (value < 0) != (divisor < 0))
        {
            quotient--;
        }
        return quotient;
    }

    private void ExpandBounds(int cq, int cr)
    {
        if (cq < _minCQ) _minCQ = cq;
        if (cq > _maxCQ) _maxCQ = cq;
        if (cr < _minCR) _minCR = cr;
        if (cr > _maxCR) _maxCR = cr;
    }

    private int MaxRingFrom(int cq, int cr)
    {
        if (_cells.Count == 0)
        {
            return 0;
        }
        int maxDq = Math.Max(Math.Abs(cq - _minCQ), Math.Abs(cq - _maxCQ));
        int maxDr = Math.Max(Math.Abs(cr - _minCR), Math.Abs(cr - _maxCR));
        return Math.Max(maxDq, maxDr);
    }
}

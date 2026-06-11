using Kodama.Application.Interfaces;
using Kodama.Application.States;
using Kodama.Domain.Entities;
using Kodama.Domain.Enums;
using Kodama.Domain.ValueObjects;

namespace Kodama.Application.Services;

/// <summary>
/// Data-oriented agent FSM. Each state is processed as a batch over the
/// AgentStore's per-state dense sets, so every system iterates only the
/// agents in its state — no per-agent virtual dispatch, no object graphs.
/// </summary>
public sealed class AgentBehaviourSystem
{
    private readonly ISimulationAnalytics _analytics;

    // State segments are snapshotted into this scratch buffer at the start of
    // each tick: transitions mutate the per-state dense sets while we iterate,
    // and the snapshot also guarantees each agent is stepped exactly once per
    // tick (an agent that goes Idle -> Finding is not re-processed until the
    // next tick), matching the semantics of the original per-agent switch.
    private readonly int[] _scratch;

    private static readonly AgentState[] ProcessingOrder =
    {
        AgentState.Idle,
        AgentState.FindingResource,
        AgentState.MovingToResource,
        AgentState.Collecting,
        AgentState.ReturningToBase,
        AgentState.Depositing,
        AgentState.Dead,
    };

    public AgentBehaviourSystem(ISimulationAnalytics analytics)
    {
        _analytics = analytics;
        _scratch = new int[WorldState.MaxAgents];
    }

    public void Tick(AgentStore store, WorldState world, float deltaTime)
    {
        // Snapshot every state segment up front.
        Span<int> segmentStarts = stackalloc int[ProcessingOrder.Length];
        Span<int> segmentCounts = stackalloc int[ProcessingOrder.Length];
        var offset = 0;
        for (var s = 0; s < ProcessingOrder.Length; s++)
        {
            segmentStarts[s] = offset;
            segmentCounts[s] = store.CopyState(ProcessingOrder[s], _scratch, offset);
            offset += segmentCounts[s];
        }

        for (var s = 0; s < ProcessingOrder.Length; s++)
        {
            var ids = _scratch.AsSpan(segmentStarts[s], segmentCounts[s]);
            switch (ProcessingOrder[s])
            {
                case AgentState.Idle: ProcessIdle(store, ids); break;
                case AgentState.FindingResource: ProcessFinding(store, world, ids); break;
                case AgentState.MovingToResource: ProcessMoving(store, world, ids); break;
                case AgentState.Collecting: ProcessCollecting(store, world, ids); break;
                case AgentState.ReturningToBase: ProcessReturning(store, world, ids); break;
                case AgentState.Depositing: ProcessDepositing(store, world, ids); break;
                case AgentState.Dead: ProcessDead(store, world, ids); break;
            }
        }

        // Dead agents have released their resources above; remove them now.
        var deadSlot = ProcessingOrder.Length - 1;
        var dead = _scratch.AsSpan(segmentStarts[deadSlot], segmentCounts[deadSlot]);
        foreach (var id in dead)
        {
            store.Remove(id);
        }
    }

    private static void ProcessIdle(AgentStore store, ReadOnlySpan<int> ids)
    {
        foreach (var id in ids)
        {
            store.ChangeState(id, AgentState.FindingResource);
        }
    }

    private void ProcessFinding(AgentStore store, WorldState world, ReadOnlySpan<int> ids)
    {
        foreach (var id in ids)
        {
            if (world.GetAvailableResources().Count < 1) // no available resource, wait
            {
                continue;
            }

            var res = world.FindNearestAvailableResource(store.GetPosition(id));
            if (res != null && res.Claim(id))
            {
                world.MarkResourceUnavailable(res);
                store.SetHarvestTarget(id, res.Id);
                store.ChangeState(id, AgentState.MovingToResource);
                _analytics.RecordQueueChange(res.Id, 1); // Resource occupied
            }
        }
    }

    private static void ProcessMoving(AgentStore store, WorldState world, ReadOnlySpan<int> ids)
    {
        foreach (var id in ids)
        {
            var targetId = store.HarvestingResourceIds[id];
            if (targetId == -1)
            {
                store.ChangeState(id, AgentState.ReturningToBase);
                continue;
            }

            var res = world.GetResource(targetId);
            if (res == null)
            {
                store.ChangeState(id, AgentState.ReturningToBase);
                continue;
            }

            var position = store.GetPosition(id);
            if (res.Position == position) // reached, stop there and harvest
            {
                store.ChangeState(id, AgentState.Collecting);
                continue;
            }

            store.MoveTo(id, GetNextStep(position, res.Position));
        }
    }

    private void ProcessCollecting(AgentStore store, WorldState world, ReadOnlySpan<int> ids)
    {
        foreach (var id in ids)
        {
            var targetId = store.HarvestingResourceIds[id];
            if (targetId == -1) // target resource gone
            {
                store.ChangeState(id, AgentState.ReturningToBase);
                continue;
            }

            var res = world.GetResource(targetId);
            if (res == null || res.IsDepleted)
            {
                // resource gone, erase it & agent will return to Tree & deposit
                if (res != null)
                {
                    _analytics.RecordQueueChange(res.Id, 0); // Resource released
                    world.RemoveResource(res.Id);
                }
                store.ClearHarvestTarget(id);
                store.ChangeState(id, AgentState.ReturningToBase);
                continue;
            }

            // stay there and keep collecting
            const long expected = 10;
            var remainingCapacity = store.GetRemainingCapacity(id);
            var actual = res.Extract(Math.Min(expected, remainingCapacity));
            store.AddInventory(id, actual);

            // check once again (after collecting)
            if (res.IsDepleted)
            {
                res.Release();
                _analytics.RecordQueueChange(res.Id, 0); // Resource released
                world.RemoveResource(res.Id);
                store.ClearHarvestTarget(id);
                store.ChangeState(id, AgentState.ReturningToBase);
            }
            else if (store.IsFull(id))
            {
                res.Release();
                _analytics.RecordQueueChange(res.Id, 0); // Resource released
                world.MarkResourceAvailable(res);
                store.ClearHarvestTarget(id);
                store.ChangeState(id, AgentState.ReturningToBase);
            }
        }
    }

    private static void ProcessReturning(AgentStore store, WorldState world, ReadOnlySpan<int> ids)
    {
        var treePosition = world.Tree.Position;
        foreach (var id in ids)
        {
            var position = store.GetPosition(id);
            if (position == treePosition) // reached base
            {
                store.ChangeState(id, AgentState.Depositing);
            }
            else
            {
                store.MoveTo(id, GetNextStep(position, treePosition));
            }
        }
    }

    private void ProcessDepositing(AgentStore store, WorldState world, ReadOnlySpan<int> ids)
    {
        foreach (var id in ids)
        {
            world.Tree.Deposit(store.Inventories[id]);
            store.ClearInventory(id);
            store.ChangeState(id, AgentState.Idle);

            _analytics.RecordTaskCompleted();
        }
    }

    private void ProcessDead(AgentStore store, WorldState world, ReadOnlySpan<int> ids)
    {
        foreach (var id in ids)
        {
            // try release resource
            var targetId = store.HarvestingResourceIds[id];
            if (targetId != -1)
            {
                var res = world.GetResource(targetId);
                if (res != null)
                {
                    res.Release();
                    world.MarkResourceAvailable(res);
                    _analytics.RecordQueueChange(res.Id, 0); // Resource released
                }
            }

            store.ClearHarvestTarget(id);
        }
    }

    private static Position GetNextStep(Position from, Position to)
    {
        if (from == to)
        {
            return from;
        }

        var best = from;
        var bestDistance = from.DistanceTo(to);
        foreach (var neighbour in from.GetNeighbors())
        {
            var dist = neighbour.DistanceTo(to);
            if (dist < bestDistance)
            {
                best = neighbour;
                bestDistance = dist;
            }
        }

        return best;
    }
}

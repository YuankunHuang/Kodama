using Kodama.Application.Interfaces;
using Kodama.Application.States;
using Kodama.Domain.Entities;
using Kodama.Domain.Enums;
using Kodama.Domain.ValueObjects;

namespace Kodama.Application.Services;

public class AgentBehaviourService
{
    private readonly ISimulationAnalytics _analytics;

    public AgentBehaviourService(ISimulationAnalytics analytics)
    {
        _analytics = analytics;
    }
    
    public void Process(Agent agent, WorldState worldState, float deltaTime)
    {
        if (agent == null)
        {
            throw new ArgumentNullException("Cannot process null agent.");
        }

        switch (agent.State)
        {
            case AgentState.Idle:
                ProcessIdle(agent, worldState);
                break;
            case AgentState.FindingResource:
                ProcessFindingResource(agent, worldState);
                break;
            case AgentState.MovingToResource:
                ProcessMovingToResource(agent, worldState);
                break;
            case AgentState.Collecting:
                ProcessCollecting(agent, worldState);
                break;
            case AgentState.ReturningToBase:
                ProcessReturningToBase(agent, worldState);
                break;
            case AgentState.Depositing:
                ProcessDepositing(agent, worldState);
                break;
            case AgentState.Dead:
                ProcessDead(agent, worldState);
                break;
            default:
                throw new NotImplementedException($"No defined processor for AgentState: {agent.State}");
        }
    }

    private void ProcessDead(Agent agent, WorldState worldState)
    {
        // try release resource
        if (agent.HarvestingResourceId != null)
        {
            var res = worldState.GetResource((int)agent.HarvestingResourceId);
            if (res != null)
            {
                worldState.MarkResourceAvailable(res);
                res.Release();
            }
        }

        // clear agent, do not remove (avoid modifying collection)
        agent.ClearHarvestTarget();
    }

    private void ProcessDepositing(Agent agent, WorldState worldState)
    {
        worldState.Tree.Deposit(agent.Inventory);
        agent.ClearInventory();
        agent.ChangeState(AgentState.Idle);
        
        _analytics.RecordTaskCompleted();
    }

    private void ProcessReturningToBase(Agent agent, WorldState worldState)
    {
        if (agent.CurrentPosition == worldState.Tree.Position) // reached base
        {
            agent.ChangeState(AgentState.Depositing);
        }
        else
        {
            worldState.MoveAgent(agent.Id, GetNextStep(agent.CurrentPosition, worldState.Tree.Position));
        }
    }

    private void ProcessCollecting(Agent agent, WorldState worldState)
    {
        // target resource gone
        if (agent.HarvestingResourceId == null)
        {
            agent.ChangeState(AgentState.ReturningToBase);
            return;
        }

        // res became unavailable
        var res = worldState.GetResource((int)agent.HarvestingResourceId);
        if (res == null || res.IsDepleted)
        {
            // resource gone, erase it & agent will return to Tree & deposit
            agent.ClearHarvestTarget();
            agent.ChangeState(AgentState.ReturningToBase);
            if (res != null)
            {
                worldState.RemoveResource(res.Id);
            }
            return;
        }

        // stay there and keep collecting
        var expected = 10;
        var remainingCapacity = agent.GetRemainingCapacity();
        var actual = res.Extract(Math.Min(expected, remainingCapacity));
        agent.AddInventory(actual);

        // check once again (after collecting)
        if (res.IsDepleted)
        {
            res.Release();
            worldState.RemoveResource(res.Id);
            agent.ClearHarvestTarget();
            agent.ChangeState(AgentState.ReturningToBase);
        }
        else if (agent.IsFull)
        {
            res.Release();
            worldState.MarkResourceAvailable(res);
            agent.ClearHarvestTarget();
            agent.ChangeState(AgentState.ReturningToBase);
        }
    }

    private void ProcessMovingToResource(Agent agent, WorldState worldState)
    {
        // resource gone?
        if (agent.HarvestingResourceId == null)
        {
            agent.ChangeState(AgentState.ReturningToBase);
            return;
        }

        var res = worldState.GetResource((int)agent.HarvestingResourceId);
        if (res == null)
        {
            agent.ChangeState(AgentState.ReturningToBase);
            return;
        }

        // valid resource
        if (res.Position == agent.CurrentPosition) // reached, stop there and harvest
        {
            agent.ChangeState(AgentState.Collecting);
            return;
        }

        // keep moving to resource
        var nextStep = GetNextStep(agent.CurrentPosition, res.Position);
        worldState.MoveAgent(agent.Id, nextStep);
    }

    private void ProcessFindingResource(Agent agent, WorldState worldState)
    {
        if (worldState.GetAvailableResources().Count < 1) // no available resource, do nothing
        {
            return;
        }

        var res = worldState.FindNearestAvailableResource(agent.CurrentPosition);
        if (res != null && res.Claim(agent.Id))
        {
            worldState.MarkResourceUnavailable(res);
            agent.SetHarvestTarget(res.Id);
            agent.ChangeState(Domain.Enums.AgentState.MovingToResource);
        }
        else
        {
            // do nothing, maybe move randomly?
        }
    }

    private void ProcessIdle(Agent agent, WorldState worldState)
    {
        // try to find resource
        agent.ChangeState(Domain.Enums.AgentState.FindingResource);
    }

    private Position GetNextStep(Position from, Position to)
    {
        if (from == to)
        {
            return from;
        }

        Position best = from;
        int bestDistance = from.DistanceTo(to);
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
namespace Kodama.Domain.Enums;

public enum AgentState
{
    Idle = 0,
    FindingResource = 1,
    MovingToResource = 2,
    Collecting = 3,
    ReturningToBase = 4,
    Depositing = 5,
    Dead = 99,
}
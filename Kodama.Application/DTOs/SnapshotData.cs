namespace Kodama.Application.DTOs;

public readonly record struct SnapshotData(AgentSnapshot[] Agents, long CreatedAt);
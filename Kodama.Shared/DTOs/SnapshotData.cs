using System;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    public struct SnapshotData
    {
        public AgentSnapshot[] Agents { get; set; }
        public long CreatedAt { get; set; }

        public SnapshotData(AgentSnapshot[] agents, long createdAt)
        {
            Agents = agents;
            CreatedAt = createdAt;
        }
    }
}

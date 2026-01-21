using System;
using System.Collections.Generic;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    public struct SnapshotData
    {
        public List<AgentSnapshot> Agents { get; set; }
        public long CreatedAt { get; set; }

        public SnapshotData(List<AgentSnapshot> agents, long createdAt)
        {
            Agents = agents;
            CreatedAt = createdAt;
        }
    }
}

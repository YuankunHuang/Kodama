using System;
using System.Collections.Generic;
using MessagePack;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    [MessagePackObject]
    public struct SnapshotData
    {
        [Key(0)]
        public List<AgentSnapshot> Agents { get; set; }
        [Key(1)]
        public TreeSnapshot Tree { get; set; }
        [Key(2)]
        public List<ResourceSnapshot> Resources { get; set; }
        [Key(3)]
        public long CreatedAt { get; set; }
    }
}

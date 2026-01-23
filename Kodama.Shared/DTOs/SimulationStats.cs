using System;
using MessagePack;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    [MessagePackObject]
    public struct SimulationStats
    {
        [Key(0)]
        public int AgentCount { get; set; }
        
        [Key(1)]
        public int ResourceCount { get; set; }
        
        [Key(2)]
        public float TickTimeMs { get; set; }
        
        [Key(3)]
        public long MemoryAllocBytes { get; set; }
        
        [Key(4)]
        public float TimeScale { get; set; }
        
        // Agent state counts
        [Key(5)]
        public int AgentsIdle { get; set; }
        
        [Key(6)]
        public int AgentsFinding { get; set; }
        
        [Key(7)]
        public int AgentsMoving { get; set; }
        
        [Key(8)]
        public int AgentsCollecting { get; set; }
        
        [Key(9)]
        public int AgentsReturning { get; set; }
        
        [Key(10)]
        public int AgentsDepositing { get; set; }
        
        [Key(11)]
        public long TreeEnergy { get; set; }
        
        // Resource stats
        [Key(12)]
        public int ResourcesOccupied { get; set; }
        
        [Key(13)]
        public int ResourcesAvailable { get; set; }
    }
}

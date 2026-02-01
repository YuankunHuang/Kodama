using System;
using System.Collections.Generic;
using MessagePack;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    [MessagePackObject]
    public struct AnalyticsReport
    {
        [Key(0)]
        public float ElapsedSeconds { get; set; }
        
        [Key(1)]
        public int TotalTasksCompleted { get; set; }
        
        [Key(2)]
        public float Throughput { get; set; }
        
        [Key(3)]
        public List<StationMetrics> Stations { get; set; }
        
        [Key(4)]
        public List<string> Recommendations { get; set; }
        
        [Key(5)]
        public long GeneratedAtUnixMs { get; set; }
    }

    [Serializable]
    [MessagePackObject]
    public struct StationMetrics
    {
        [Key(0)]
        public int StationId { get; set; }
        
        [Key(1)]
        public float Utilization { get; set; } // 0.0 ~ 1.0
        
        [Key(2)]
        public int CurrentQueue { get; set; }
        
        [Key(3)]
        public int PeakQueue { get; set; }
        
        [Key(4)]
        public float AveWaitTimeSeconds { get; set; }

        [IgnoreMember] public bool IsBottleneck => Utilization > 0.85f || PeakQueue > 10;
        [IgnoreMember] public bool IsUnderutilized => Utilization < 0.3f;
    }
}
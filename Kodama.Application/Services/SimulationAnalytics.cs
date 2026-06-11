using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Kodama.Application.Interfaces;
using Kodama.Shared.DTOs;

namespace Kodama.Application.Services;

public class SimulationAnalytics : ISimulationAnalytics
{
    private readonly ConcurrentDictionary<int, StationStats> _stationStats = new(
        concurrencyLevel:Environment.ProcessorCount,
        capacity:100
    );

    private readonly List<StationMetrics> _stationsBuffer;
    private readonly List<string> _recommendationsBuffer;
    
    private float _elapsedTime;
    private int _taskCompleted;

    private const float ThroughputThreshold = 10f;
    private const float BottleneckUtilization = 0.85f;
    private const float UnderutilizedThreshold = 0.3f;
    private const int BottleneckPeakQueue = 10;
    
    public SimulationAnalytics()
    {
        _stationStats = new ConcurrentDictionary<int, StationStats>(
            concurrencyLevel: Environment.ProcessorCount,
            capacity: 128
        );
        _stationsBuffer = new List<StationMetrics>(128);
        _recommendationsBuffer = new List<string>(4);
        
        Reset();
    }
    
    public void Tick(float deltaTime)
    {
        var elap = Volatile.Read(ref _elapsedTime);
        Volatile.Write(ref _elapsedTime, elap + deltaTime);
    }

    public void RecordTaskCompleted()
    {
        Interlocked.Increment(ref _taskCompleted);
    }

    public void RecordQueueChange(int stationId, int queueLength)
    {
        _stationStats.AddOrUpdate(
            stationId,
            (_, len) => new StationStats
            {
                CurrentQueue = len,
                PeakQueue = len,
                QueueSamples = 1,
                TotalQueueLength = len,
            },
            (_, existing, len) =>
            {
                existing.CurrentQueue = len;
                existing.PeakQueue = Math.Max(existing.PeakQueue, len);
                existing.QueueSamples++;
                existing.TotalQueueLength += len;
                return existing;
            },
            queueLength
        );
    }

    public AnalyticsReport GenerateReport()
    {
        var elapsedTime = Volatile.Read(ref _elapsedTime);
        var taskCompleted = Volatile.Read(ref _taskCompleted);
        var throughput = elapsedTime > 0 ? taskCompleted / elapsedTime : 0;

        _stationsBuffer.Clear();

        foreach (var kvp in _stationStats)
        {
            var stats = kvp.Value;
            var utilization = CalculateUtilization(stats);
            _stationsBuffer.Add(new StationMetrics
            {
                StationId = kvp.Key,
                Utilization = utilization,
                CurrentQueue = stats.CurrentQueue,
                PeakQueue = stats.PeakQueue,
                AveWaitTimeSeconds = stats.QueueSamples > 0
                    ? (float)stats.TotalQueueLength / stats.QueueSamples * 0.1f
                    : 0f
            });
        }
        
        var recommendations = GenerateRecommendations(throughput);
        
        return new AnalyticsReport()
        {
            ElapsedSeconds = elapsedTime,
            TotalTasksCompleted = taskCompleted,
            Throughput = throughput,
            Stations = _stationsBuffer,
            Recommendations = recommendations,
            GeneratedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
    }

    private List<string> GenerateRecommendations(float throughput)
    {
        _recommendationsBuffer.Clear();

        // 1. bottleneck
        int worstBottleneckId = -1;
        float worstUtilization = 0f;
        int bottleneckCount = 0;

        for (int i = 0; i < _stationsBuffer.Count; i++)
        {
            var s = _stationsBuffer[i];
            if (s.Utilization > BottleneckUtilization || s.PeakQueue > BottleneckPeakQueue)
            {
                bottleneckCount++;
                if (s.Utilization > worstUtilization)
                {
                    worstUtilization = s.Utilization;
                    worstBottleneckId = s.StationId;
                }
            }
        }

        if (bottleneckCount > 0)
        {
            _recommendationsBuffer.Add($"Bottleneck detected at Station #{worstBottleneckId}: Consider redistributing load to underutilized stations.");
        }

        // 2. underutilized
        if (bottleneckCount > 0)
        {
            int underutilizedCount = 0;
            for (int i = 0; i < _stationsBuffer.Count; i++)
            {
                if (_stationsBuffer[i].Utilization < UnderutilizedThreshold)
                {
                    underutilizedCount++;
                }
            }

            if (underutilizedCount > 0)
            {
                _recommendationsBuffer.Add($"{underutilizedCount} station(s) are underutilized. Redirect traffic from bottleneck stations.");
            }
        }

        // 3. throughput (too low)
        if (throughput < ThroughputThreshold)
        {
            _recommendationsBuffer.Add("Overall throughput is below target. Consider adding more agents or optimizing task assignment.");
        }
        
        // 4. to sum up -> recommendations
        if (_recommendationsBuffer.Count < 1)
        {
            _recommendationsBuffer.Add($"System is operating within normal parameters.");
        }

        return _recommendationsBuffer;
    }

    private float CalculateUtilization(StationStats stationStats)
    {
        return Math.Min(1.0f, stationStats.QueueSamples / 1000f);
    }

    public void Reset()
    {
        _taskCompleted = 0;
        _elapsedTime = 0;
        _stationStats.Clear();
        _stationsBuffer.Clear();
        _recommendationsBuffer.Clear();
    }
    
    private class StationStats
    {
        public int CurrentQueue { get; set; }
        public int PeakQueue { get; set; }
        public int QueueSamples { get; set; }
        public long TotalQueueLength { get; set; }
    }
}
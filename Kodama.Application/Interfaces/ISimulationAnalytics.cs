using Kodama.Shared.DTOs;

namespace Kodama.Application.Interfaces;

public interface ISimulationAnalytics
{
    /// <summary>
    /// Called per frame, to add to total running time
    /// </summary>
    /// <param name="deltaTime"></param>
    void Tick(float deltaTime);

    /// <summary>
    /// Called when an Agent finishes its task (Depositing -> Idle)
    /// </summary>
    void RecordTaskCompleted();

    /// <summary>
    /// Called when a Resource queue updates
    /// </summary>
    /// <param name="stationId"></param>
    /// <param name="queueLength"></param>
    void RecordQueueChange(int stationId, int queueLength);

    /// <summary>
    /// Creates new latest AnalyticsReport
    /// </summary>
    /// <returns></returns>
    AnalyticsReport GenerateReport();

    /// <summary>
    /// Resets all analytics data
    /// </summary>
    void Reset();
}
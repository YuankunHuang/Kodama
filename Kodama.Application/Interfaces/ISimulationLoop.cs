using Kodama.Shared.DTOs;

namespace Kodama.Application.Interfaces;

public interface ISimulationLoop
{
    SnapshotData Tick(float deltaTime);
}
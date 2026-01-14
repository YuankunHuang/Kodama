using Kodama.Shared.DTOs;

namespace Kodama.Application.Interfaces;

public interface IGameBroadcaster
{
    Task BroadcastToAllClients(SnapshotData snapshot);
}
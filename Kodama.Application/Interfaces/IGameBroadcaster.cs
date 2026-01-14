using Kodama.Application.DTOs;

namespace Kodama.Application.Interfaces;

public interface IGameBroadcaster
{
    Task BroadcastToAllClients(SnapshotData snapshot);
}
using MessagePack;
using Kodama.Shared.DTOs;
using Kodama.Application.Interfaces;
using Kodama.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Kodama.Infrastructure.Services;

public class SignalRBroadcaster : IGameBroadcaster
{
    private readonly IHubContext<GameHub, IGameClient> _hubContext;

    public SignalRBroadcaster(IHubContext<GameHub, IGameClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastToAllClients(SnapshotData snapshot)
    {
        byte[] data = MessagePackSerializer.Serialize(snapshot);
        await _hubContext.Clients.All.ReceiveSnapshot(data);
    }
}

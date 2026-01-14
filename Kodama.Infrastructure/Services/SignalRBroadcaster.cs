using System.Text.Json;
using Kodama.Application.DTOs;
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
        // Phase 1: 使用 JSON 序列化，后续换成 MessagePack
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(snapshot);
        await _hubContext.Clients.All.ReceiveSnapshot(data);
    }
}

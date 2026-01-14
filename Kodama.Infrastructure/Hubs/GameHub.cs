using Microsoft.AspNetCore.SignalR;

namespace Kodama.Infrastructure.Hubs;

public class GameHub : Hub<IGameClient>
{
}

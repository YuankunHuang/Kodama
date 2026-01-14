namespace Kodama.Infrastructure.Hubs;

public interface IGameClient
{
    Task ReceiveSnapshot(byte[] data);
}
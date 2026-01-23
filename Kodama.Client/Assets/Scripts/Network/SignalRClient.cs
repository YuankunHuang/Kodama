using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Kodama.Shared.DTOs;
using MessagePack;
using YuankunHuang.Kodama.Core;

namespace YuankunHuang.Kodama.Network
{
    public class SignalRClient
    {
        private readonly string _url = string.Empty;

        private HubConnection _connection;
        
        public SignalRClient(string url)
        {
            _url = url;
        }

        public async Task ConnectAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_url)
                .Build();

            _connection.On<byte[]>("ReceiveSnapshot", data =>
            {
                var snapshotData = MessagePackSerializer.Deserialize<SnapshotData>(data);
                MonoBehaviourUtil.Instance.RunOnMainThread(() =>
                {
                    EventBus.Publish(EventKeys.SnapshotReceived, snapshotData);
                });
            });
            
            await _connection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            await _connection.StopAsync();
        }
        
        public async Task SetTimeScaleAsync(float scale)
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("SetTimeScale", scale);
            }
        }

        public async Task RestartAsync()
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("Restart");
            }
        }
    }    
}
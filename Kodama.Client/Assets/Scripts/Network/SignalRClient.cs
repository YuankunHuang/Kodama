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
            try
            {
                UnityEngine.Debug.Log($"[SignalR] Connecting to {_url}...");
                
                _connection = new HubConnectionBuilder()
                    .WithUrl(_url)
                    .Build();

                _connection.On<byte[]>("ReceiveSnapshot", data =>
                {
                    // UnityEngine.Debug.Log($"[SignalR] Received snapshot: {data.Length} bytes");
                    var snapshotData = MessagePackSerializer.Deserialize<SnapshotData>(data);
                    MonoBehaviourUtil.Instance.RunOnMainThread(() =>
                    {
                        EventBus.Publish(EventKeys.SnapshotReceived, snapshotData);
                    });
                });
                
                await _connection.StartAsync();
                UnityEngine.Debug.Log("[SignalR] Connected successfully!");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[SignalR] Connection failed: {e}");
            }
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
        
        public async Task SetPausedAsync(bool paused)
        {
            if (_connection?.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("SetPaused", paused);
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
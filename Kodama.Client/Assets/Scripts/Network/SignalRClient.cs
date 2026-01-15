using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Kodama.Shared.DTOs;
using UnityEngine;
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
                var snapshotData = JsonSerializer.Deserialize<SnapshotData>(data);
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
    }    
}
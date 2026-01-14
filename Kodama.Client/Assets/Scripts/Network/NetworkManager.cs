using UnityEngine;
using YuankunHuang.Kodama.Core;

namespace YuankunHuang.Kodama.Network
{
    public class NetworkManager : IModule
    {
        private SignalRClient _signalRClient;
        
        private const string ServerUrl = "http://localhost:5059/gamehub";
        
        public void Init()
        {
            Debug.Log($"[NetworkManager] OnInit");
            
            _signalRClient = new SignalRClient(ServerUrl);
            _ = _signalRClient.ConnectAsync();
        }

        public void Dispose()
        {
            Debug.Log($"[NetworkManager] OnDispose");
            
            _ = _signalRClient.DisconnectAsync();
        }
    }    
}
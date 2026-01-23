using UnityEngine;
using YuankunHuang.Kodama.Core;

namespace YuankunHuang.Kodama.Network
{
    public class NetworkManager : IModule
    {
        private SignalRClient _signalRClient;
        private TimeScaleController _timeScaleController;
        
        private const string ServerUrl = "http://localhost:5059/gamehub";
        
        public void Init()
        {
            Debug.Log("[NetworkManager] Init");
            
            _signalRClient = new SignalRClient(ServerUrl);
            _ = _signalRClient.ConnectAsync();
            
            _timeScaleController = new TimeScaleController(this);
            _timeScaleController.Init();
        }

        public void Dispose()
        {
            Debug.Log("[NetworkManager] Dispose");
            
            _timeScaleController.Dispose();
            _timeScaleController = null;
            
            _ = _signalRClient.DisconnectAsync();
            _signalRClient = null;
        }
        
        public void SetTimeScale(float scale)
        {
            _ = _signalRClient?.SetTimeScaleAsync(scale);
        }

        public void Restart()
        {
            _ = _signalRClient?.RestartAsync();
        }
    }    
}
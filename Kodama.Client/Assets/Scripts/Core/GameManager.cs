using System;
using UnityEngine;
using YuankunHuang.Kodama.Network;
using YuankunHuang.Kodama.Render;

namespace YuankunHuang.Kodama.Core
{
    public class GameManager : MonoBehaviour
    {
        private bool _isInitialized = false;

        [SerializeField] private Transform _agent;
        
        private void OnEnable()
        {
            if (_isInitialized)
            {
                return;
            }
            
            // initialize
            ModuleRegistry.Register(new NetworkManager());
            ModuleRegistry.Register(new RenderManager(_agent));
            
            ModuleRegistry.Get<NetworkManager>().Init();
            ModuleRegistry.Get<RenderManager>().Init();
            
            _isInitialized = true;
        }

        private void OnDisable()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            // dispose
            ModuleRegistry.Get<RenderManager>().Dispose();
            ModuleRegistry.Get<NetworkManager>().Dispose();

            ModuleRegistry.Clear();
            
            _isInitialized = false;
        }
    }    
}
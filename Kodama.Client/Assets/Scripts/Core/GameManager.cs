using System;
using System.Collections;
using UnityEngine;
using YuankunHuang.Kodama.Network;
using YuankunHuang.Kodama.Render;

namespace YuankunHuang.Kodama.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Mesh _agentMesh;
        [SerializeField] private Material _agentMaterial;

        private bool _isInitialized = false;
        
        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            if (_isInitialized)
            {
                return;
            }
            
            StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            yield return new WaitUntil(() => MonoBehaviourUtil.Instance != null);
            
            // initialize
            ModuleRegistry.Register(new NetworkManager());
            ModuleRegistry.Register(new RenderManager(_agentMesh, _agentMaterial));
            
            ModuleRegistry.Get<NetworkManager>().Init();
            ModuleRegistry.Get<RenderManager>().Init();
            
            _isInitialized = true;
        }

        private void OnDisable()
        {
            Dispose();
        }

        private void Dispose()
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
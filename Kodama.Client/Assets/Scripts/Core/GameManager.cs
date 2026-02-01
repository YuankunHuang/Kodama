using System;
using System.Collections;
using UnityEngine;
using YuankunHuang.Kodama.Network;
using YuankunHuang.Kodama.Render;

namespace YuankunHuang.Kodama.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Agent")]
        [SerializeField] private Mesh _agentMesh;
        [SerializeField] private Material _agentMaterial;
        [SerializeField] private Material _agentReturningMaterial; // Highlighted when returning to base
        [SerializeField] private Vector3 _agentScale;
        
        [Header("Tree")]
        [SerializeField] private Mesh _treeMesh;
        [SerializeField] private Material _treeMaterial;
        [SerializeField] private Vector3 _treeScale;
        
        [Header("Resource")]
        [SerializeField] private Mesh _resourceMesh;
        [SerializeField] private Material _resourceMaterial;
        [SerializeField] private Material _resourceCollectingMaterial; // Highlighted when being collected
        [SerializeField] private Vector3 _resourceScale;

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
            // Unlock frame rate
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
            
            yield return new WaitUntil(() => MonoBehaviourUtil.Instance != null);
            
            var renderConfig = new RenderConfig
            {
                AgentMesh = _agentMesh,
                AgentMaterial = _agentMaterial,
                AgentReturningMaterial = _agentReturningMaterial,
                AgentScale = _agentScale,
                TreeMesh = _treeMesh,
                TreeMaterial = _treeMaterial,
                TreeScale = _treeScale,
                ResourceMesh = _resourceMesh,
                ResourceMaterial = _resourceMaterial,
                ResourceCollectingMaterial = _resourceCollectingMaterial,
                ResourceScale = _resourceScale,
            };
            
            // Initialize modules
            ModuleRegistry.Register(new NetworkManager());
            ModuleRegistry.Register(new RenderManager(renderConfig));
            
            ModuleRegistry.Get<NetworkManager>().Init();
            ModuleRegistry.Get<RenderManager>().Init();
            
            _isInitialized = true;
        }

        private void Update()
        {
            // ESC to quit
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
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
            
            // Dispose modules
            ModuleRegistry.Get<RenderManager>().Dispose();
            ModuleRegistry.Get<NetworkManager>().Dispose();

            ModuleRegistry.Clear();
            
            _isInitialized = false;
        }
    }    
}

using System;
using System.Collections.Generic;
using Kodama.Shared.DTOs;
using UnityEngine;
using YuankunHuang.Kodama.Core;
using YuankunHuang.Kodama.Utils;

namespace YuankunHuang.Kodama.Render
{
    public class RenderConfig
    {
        public Mesh AgentMesh;
        public Material AgentMaterial;
        public Vector3 AgentScale;
        
        public Mesh TreeMesh;
        public Material TreeMaterial;
        public Vector3 TreeScale;
        
        public Mesh ResourceMesh;
        public Material ResourceMaterial;
        public Vector3 ResourceScale;
    }

    public class RenderManager : IModule
    {
        private readonly RenderConfig _config;
        
        // Agent rendering (interpolated)
        private InstancedRenderer _agentRenderer;
        private Vector3[] _prevPositions;
        private Vector3[] _currPositions;
        private Vector3[] _renderedPositions;
        
        // Static entity rendering
        private InstancedRenderer _treeRenderer;
        private InstancedRenderer _resourceRenderer;
        private Vector3[] _treePositions;
        private Vector3[] _resourcePositions;
        
        private float _snapshotReceivedTime;
        private bool _hasData;
        private const float SnapshotInterval = 0.1f;
        
        public RenderManager(RenderConfig config)
        {
            _config = config;
            _agentRenderer = new InstancedRenderer(config.AgentMesh, config.AgentMaterial);
            _treeRenderer = new InstancedRenderer(config.TreeMesh, config.TreeMaterial);
            _resourceRenderer = new InstancedRenderer(config.ResourceMesh, config.ResourceMaterial);
            
            _treePositions = new Vector3[1]; // Only 1 tree for now
        }
        
        public void Init()
        {
            Debug.Log("[RenderManager] Init");
            EventBus.Subscribe<SnapshotData>(EventKeys.SnapshotReceived, OnSnapshotReceived);
            MonoBehaviourUtil.Instance.OnUpdate += OnUpdate;
        }

        public void Dispose()
        {
            Debug.Log("[RenderManager] Dispose");
            EventBus.Unsubscribe<SnapshotData>(EventKeys.SnapshotReceived, OnSnapshotReceived);
            MonoBehaviourUtil.Instance.OnUpdate -= OnUpdate;
        }

        private void OnUpdate()
        {
            if (!_hasData) return;

            // Interpolate agents
            var elapsed = Time.time - _snapshotReceivedTime;
            var t = Mathf.Clamp01(elapsed / SnapshotInterval);

            for (var i = 0; i < _renderedPositions.Length; ++i)
            {
                _renderedPositions[i] = Vector3.Lerp(_prevPositions[i], _currPositions[i], t);
            }

            // Render all entities
            _agentRenderer.Render(_renderedPositions, _config.AgentScale);
            _treeRenderer.Render(_treePositions, _config.TreeScale);
            _resourceRenderer.Render(_resourcePositions, _config.ResourceScale);
        }

        private void OnSnapshotReceived(SnapshotData snapshot)
        {
            // === Agents (interpolated) ===
            int agentCount = snapshot.Agents?.Count ?? 0;
            
            if (_currPositions == null || _currPositions.Length < agentCount)
            {
                _currPositions = new Vector3[agentCount];
                _prevPositions = new Vector3[agentCount];
                _renderedPositions = new Vector3[agentCount];
            }
            
            if (_hasData)
            {
                // Swap buffers
                (_prevPositions, _currPositions) = (_currPositions, _prevPositions);
                
                for (var i = 0; i < agentCount; ++i)
                {
                    var agent = snapshot.Agents[i];
                    _currPositions[i] = HexUtils.HexToWorld(agent.Q, agent.R);
                }
            }
            else
            {
                for (var i = 0; i < agentCount; ++i)
                {
                    var agent = snapshot.Agents[i];
                    var pos = HexUtils.HexToWorld(agent.Q, agent.R);
                    _prevPositions[i] = pos;
                    _currPositions[i] = pos;
                    _renderedPositions[i] = pos;
                }
                _hasData = true;
            }
            
            // === Tree (static) ===
            _treePositions[0] = HexUtils.HexToWorld(snapshot.Tree.Q, snapshot.Tree.R);
            
            // === Resources (static) ===
            int resourceCount = snapshot.Resources?.Count ?? 0;
            if (_resourcePositions == null || _resourcePositions.Length < resourceCount)
            {
                _resourcePositions = new Vector3[resourceCount];
            }
            
            for (var i = 0; i < resourceCount; ++i)
            {
                var res = snapshot.Resources[i];
                _resourcePositions[i] = HexUtils.HexToWorld(res.Q, res.R);
            }
            
            _snapshotReceivedTime = Time.time;
        }
    }    
}

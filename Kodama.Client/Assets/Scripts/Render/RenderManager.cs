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
        public Material AgentReturningMaterial;
        public Vector3 AgentScale;
        
        public Mesh TreeMesh;
        public Material TreeMaterial;
        public Vector3 TreeScale;
        
        public Mesh ResourceMesh;
        public Material ResourceMaterial;
        public Material ResourceCollectingMaterial;
        public Vector3 ResourceScale;
    }

    public class RenderManager : IModule
    {
        private const byte AgentStateReturningToBase = 4; // Matches AgentState.ReturningToBase
        
        private readonly RenderConfig _config;
        
        // Agent rendering (interpolated) - split by state
        private InstancedRenderer _agentRenderer;
        private InstancedRenderer _agentReturningRenderer;
        
        // Agent position buffers (by ID for interpolation)
        private Vector3[] _prevPositions;
        private Vector3[] _currPositions;
        private byte[] _agentStates;
        
        // Temp buffers for rendering (reused each frame)
        private List<Vector3> _normalAgentPositions;
        private List<Vector3> _returningAgentPositions;
        
        // Static entity rendering - split by state
        private InstancedRenderer _treeRenderer;
        private InstancedRenderer _resourceRenderer;
        private InstancedRenderer _resourceCollectingRenderer;
        
        private Vector3[] _treePositions;
        private List<Vector3> _normalResourcePositions;
        private List<Vector3> _collectingResourcePositions;
        
        private float _snapshotReceivedTime;
        private bool _hasData;
        private const float SnapshotInterval = 0.1f;
        
        public RenderManager(RenderConfig config)
        {
            _config = config;
            
            // Agent renderers
            _agentRenderer = new InstancedRenderer(config.AgentMesh, config.AgentMaterial);
            _agentReturningRenderer = new InstancedRenderer(config.AgentMesh, 
                config.AgentReturningMaterial ?? config.AgentMaterial);
            
            // Resource renderers
            _resourceRenderer = new InstancedRenderer(config.ResourceMesh, config.ResourceMaterial);
            _resourceCollectingRenderer = new InstancedRenderer(config.ResourceMesh, 
                config.ResourceCollectingMaterial ?? config.ResourceMaterial);
            
            // Tree renderer
            _treeRenderer = new InstancedRenderer(config.TreeMesh, config.TreeMaterial);
            _treePositions = new Vector3[1];
            
            // Temp buffers
            _normalAgentPositions = new List<Vector3>(10000);
            _returningAgentPositions = new List<Vector3>(10000);
            _normalResourcePositions = new List<Vector3>(500);
            _collectingResourcePositions = new List<Vector3>(500);
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

            // Interpolate and group agents by state
            var elapsed = Time.time - _snapshotReceivedTime;
            var t = Mathf.Clamp01(elapsed / SnapshotInterval);

            _normalAgentPositions.Clear();
            _returningAgentPositions.Clear();

            for (var i = 0; i < _currPositions.Length; ++i)
            {
                var pos = Vector3.Lerp(_prevPositions[i], _currPositions[i], t);
                
                if (_agentStates[i] == AgentStateReturningToBase)
                {
                    _returningAgentPositions.Add(pos);
                }
                else
                {
                    _normalAgentPositions.Add(pos);
                }
            }

            // Render agents (normal + returning)
            if (_normalAgentPositions.Count > 0)
                _agentRenderer.RenderList(_normalAgentPositions, _config.AgentScale);
            if (_returningAgentPositions.Count > 0)
                _agentReturningRenderer.RenderList(_returningAgentPositions, _config.AgentScale);
            
            // Render resources (normal + collecting)
            if (_normalResourcePositions.Count > 0)
                _resourceRenderer.RenderList(_normalResourcePositions, _config.ResourceScale);
            if (_collectingResourcePositions.Count > 0)
                _resourceCollectingRenderer.RenderList(_collectingResourcePositions, _config.ResourceScale);
            
            // Render tree
            _treeRenderer.Render(_treePositions, _config.TreeScale);
        }

        private void OnSnapshotReceived(SnapshotData snapshot)
        {
            // === Agents (interpolated) ===
            int agentCount = snapshot.Agents?.Count ?? 0;
            
            if (_currPositions == null || _currPositions.Length < agentCount)
            {
                _currPositions = new Vector3[agentCount];
                _prevPositions = new Vector3[agentCount];
                _agentStates = new byte[agentCount];
            }
            
            if (_hasData)
            {
                // Swap buffers
                (_prevPositions, _currPositions) = (_currPositions, _prevPositions);
                
                for (var i = 0; i < agentCount; ++i)
                {
                    var agent = snapshot.Agents[i];
                    _currPositions[i] = HexUtils.HexToWorld(agent.Q, agent.R);
                    _agentStates[i] = agent.State;
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
                    _agentStates[i] = agent.State;
                }
                _hasData = true;
            }
            
            // === Tree (static) ===
            _treePositions[0] = HexUtils.HexToWorld(snapshot.Tree.Q, snapshot.Tree.R);
            
            // === Resources (grouped by state) ===
            _normalResourcePositions.Clear();
            _collectingResourcePositions.Clear();
            
            int resourceCount = snapshot.Resources?.Count ?? 0;
            for (var i = 0; i < resourceCount; ++i)
            {
                var res = snapshot.Resources[i];
                var pos = HexUtils.HexToWorld(res.Q, res.R);
                
                if (res.IsBeingCollected)
                {
                    _collectingResourcePositions.Add(pos);
                }
                else
                {
                    _normalResourcePositions.Add(pos);
                }
            }
            
            _snapshotReceivedTime = Time.time;
        }
    }    
}

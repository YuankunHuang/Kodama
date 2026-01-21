using System;
using Kodama.Shared.DTOs;
using UnityEngine;
using YuankunHuang.Kodama.Core;
using YuankunHuang.Kodama.Utils;

namespace YuankunHuang.Kodama.Render
{
    public class RenderManager : IModule
    {
        private InstancedRenderer _renderer;
        private Vector3[] _prevPositions; // prev
        private Vector3[] _currPositions; // cur
        private Vector3[] _renderedPositions; // lerped - actually seen
        
        private float _snapshotReceivedTime;
        private bool _hasData;
        private const float SnapshotInterval = .1f;
        
        public RenderManager(Mesh mesh, Material material)
        {
            _renderer = new InstancedRenderer(mesh, material);
        }
        
        public void Init()
        {
            Debug.Log($"[RenderManager] OnInit");
            EventBus.Subscribe<SnapshotData>(EventKeys.SnapshotReceived, OnSnapshotReceived);

            MonoBehaviourUtil.Instance.OnUpdate += OnUpdate;
        }

        public void Dispose()
        {
            Debug.Log($"[RenderManager] OnDispose");
            EventBus.Unsubscribe<SnapshotData>(EventKeys.SnapshotReceived, OnSnapshotReceived);
            
            MonoBehaviourUtil.Instance.OnUpdate -= OnUpdate;
        }

        private void OnUpdate()
        {
            if (!_hasData)
            {
                return;
            }

            var elapsed = Time.time - _snapshotReceivedTime;
            var t = Mathf.Clamp01(elapsed / SnapshotInterval);

            for (var i = 0; i < _renderedPositions.Length; ++i)
            {
                _renderedPositions[i] = Vector3.Lerp(_prevPositions[i], _currPositions[i], t);
            }

            _renderer.Render(_renderedPositions);
        }

        private void OnSnapshotReceived(SnapshotData snapshot)
        {
            if (_currPositions == null || _currPositions.Length < snapshot.Agents.Count)
            {
                _currPositions = new Vector3[snapshot.Agents.Count];
            }
            if (_prevPositions == null || _prevPositions.Length < _currPositions.Length)
            {
                _prevPositions = new Vector3[_currPositions.Length];
            }
            if (_renderedPositions == null || _renderedPositions.Length < _currPositions.Length)
            {
                _renderedPositions = new Vector3[_currPositions.Length];
            }
            
            if (_hasData)
            {
                (_prevPositions, _currPositions) = (_currPositions, _prevPositions);
                
                for (var i = 0; i < snapshot.Agents.Count; ++i)
                {
                    var agent = snapshot.Agents[i];
                    _currPositions[i] = HexUtils.HexToWorld(agent.Q, agent.R);
                }
            }
            else
            {
                for (var i = 0; i < snapshot.Agents.Count; ++i)
                {
                    var agent = snapshot.Agents[i];
                    _prevPositions[i] = _currPositions[i] = HexUtils.HexToWorld(agent.Q, agent.R);
                }
                
                _hasData = true;
            }
            
            _snapshotReceivedTime = Time.time;
        }
    }    
}
using System;
using Kodama.Shared.DTOs;
using UnityEngine;
using YuankunHuang.Kodama.Core;

namespace YuankunHuang.Kodama.Render
{
    public class RenderManager : IModule
    {
        private Transform _agent;

        private SnapshotData _prevSnapshot;
        private SnapshotData _currSnapshot;
        private float _snapshotReceivedTime;
        private bool _hasData;
        private const float SnapshotInterval = .1f;
        
        public RenderManager(Transform agent)
        {
            _agent = agent;
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

            if (_prevSnapshot.Agents == null || _prevSnapshot.Agents.Length == 0 ||
                _currSnapshot.Agents == null || _currSnapshot.Agents.Length == 0)
            {
                return;
            }

            var elapsed = Time.time - _snapshotReceivedTime;
            var t = Mathf.Clamp01(elapsed / SnapshotInterval);

            var prevPos = new Vector3(_prevSnapshot.Agents[0].Q, _prevSnapshot.Agents[0].R, 0);
            var currPos = new Vector3(_currSnapshot.Agents[0].Q, _currSnapshot.Agents[0].R, 0);
            _agent.position = Vector3.Lerp(prevPos, currPos, t);
        }

        private void OnSnapshotReceived(SnapshotData snapshot)
        {
            if (_hasData)
            {
                _prevSnapshot = _currSnapshot;
            }
            else
            {
                _prevSnapshot = snapshot;
                _hasData = true;
            }
            
            _currSnapshot = snapshot;
            _snapshotReceivedTime = Time.time;
        }
    }    
}
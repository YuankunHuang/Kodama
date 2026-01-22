using UnityEngine;
using YuankunHuang.Kodama.Core;
using YuankunHuang.Kodama.Network;

namespace YuankunHuang.Kodama.Network
{
    /// <summary>
    /// Controls simulation time scale via keyboard.
    /// [ = Slow down, ] = Speed up, \ = Reset to 1x
    /// </summary>
    public class TimeScaleController
    {
        private float _currentScale = 1f;
        private NetworkManager _networkManager;

        public TimeScaleController(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public void Init()
        {
            MonoBehaviourUtil.Instance.OnUpdate += Update;
        }

        public void Dispose()
        {
            MonoBehaviourUtil.Instance.OnUpdate -= Update;
        }

        private void Update()
        {
            bool changed = false;
            
            if (Input.GetKeyDown(KeyCode.LeftBracket)) // [
            {
                _currentScale = Mathf.Max(0.1f, _currentScale / 2f);
                changed = true;
            }
            else if (Input.GetKeyDown(KeyCode.RightBracket)) // ]
            {
                _currentScale = Mathf.Min(10f, _currentScale * 2f);
                changed = true;
            }
            else if (Input.GetKeyDown(KeyCode.Backslash)) // \
            {
                _currentScale = 1f;
                changed = true;
            }
            
            if (changed)
            {
                Debug.Log($"[TimeScale] Setting to {_currentScale:F1}x");
                _networkManager?.SetTimeScale(_currentScale);
            }
        }
    }
}

using Kodama.Shared.DTOs;
using UnityEngine;
using YuankunHuang.Kodama.Core;
using YuankunHuang.Kodama.Network;

namespace YuankunHuang.Kodama.UI
{
    public class SimulationHUD : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool _showHUD = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.H;
        [SerializeField] private KeyCode _restartKey = KeyCode.R;
        [SerializeField] private KeyCode _pauseKey = KeyCode.Space;
        
        // Cached stats
        private SimulationStats _stats;
        private float _clientFps;
        private float _fpsUpdateTimer;
        private int _frameCount;
        
        // GUI Style
        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private bool _stylesInitialized;

        private void OnEnable()
        {
            EventBus.Subscribe<SnapshotData>(EventKeys.SnapshotReceived, OnSnapshotReceived);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SnapshotData>(EventKeys.SnapshotReceived, OnSnapshotReceived);
        }

        private void OnSnapshotReceived(SnapshotData snapshot)
        {
            _stats = snapshot.Stats;
        }

        private bool _isPaused;

        private void Update()
        {
            // Toggle HUD
            if (Input.GetKeyDown(_toggleKey))
            {
                _showHUD = !_showHUD;
            }
            
            // Restart simulation
            if (Input.GetKeyDown(_restartKey))
            {
                var networkManager = ModuleRegistry.Get<NetworkManager>();
                networkManager?.Restart();
            }
            
            // Pause/Resume simulation
            if (Input.GetKeyDown(_pauseKey))
            {
                _isPaused = !_isPaused;
                var networkManager = ModuleRegistry.Get<NetworkManager>();
                networkManager?.SetPaused(_isPaused);
            }
            
            // Calculate client FPS
            _frameCount++;
            _fpsUpdateTimer += Time.unscaledDeltaTime;
            if (_fpsUpdateTimer >= 0.5f)
            {
                _clientFps = _frameCount / _fpsUpdateTimer;
                _frameCount = 0;
                _fpsUpdateTimer = 0;
            }
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;
            
            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0, 0, 0, 0.8f));
            bgTex.Apply();
            _boxStyle.normal.background = bgTex;
            
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.8f, 1f, 0.8f) },
                alignment = TextAnchor.MiddleLeft
            };
            
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0f, 1f, 1f) },
                alignment = TextAnchor.MiddleCenter
            };
            
            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!_showHUD) return;
            
            InitStyles();
            
            float width = 320;
            float height = 820;
            float x = 10;
            float y = 10;
            
            GUILayout.BeginArea(new Rect(x, y, width, height), _boxStyle);
            
            // Header
            GUILayout.Label("KODAMA SIMULATION", _headerStyle);
            GUILayout.Space(6);
            
            // Performance
            GUILayout.Label("─── PERFORMANCE ───", _headerStyle);
            GUILayout.Label($"Server Tick:  {_stats.TickTimeMs:F2} ms", _labelStyle);
            GUILayout.Label($"Memory Alloc: {_stats.MemoryAllocBytes} bytes", _labelStyle);
            GUILayout.Label($"Client FPS:   {_clientFps:F0}", _labelStyle);
            GUILayout.Label($"Time Scale:   {(_isPaused ? 0 : _stats.TimeScale):F1}x", _labelStyle);
            GUILayout.Space(6);
            
            // Entities
            GUILayout.Label("─── ENTITIES ───", _headerStyle);
            GUILayout.Label($"Agents:       {_stats.AgentCount:N0}", _labelStyle);
            GUILayout.Label($"Tree Energy:  {_stats.TreeEnergy:N0}", _labelStyle);
            GUILayout.Space(6);
            
            // Resource Details
            GUILayout.Label("─── RESOURCES ───", _headerStyle);
            int totalRes = _stats.ResourceCount > 0 ? _stats.ResourceCount : 1;
            float occupiedPct = (float)_stats.ResourcesOccupied / totalRes * 100f;
            GUILayout.Label($"Total:        {_stats.ResourceCount:N0}", _labelStyle);
            GUILayout.Label($"Occupied:     {_stats.ResourcesOccupied:N0} ({occupiedPct:F0}%)", new GUIStyle(_labelStyle) { normal = { textColor = new Color(1f, 0.5f, 0f) } });
            GUILayout.Label($"Available:    {_stats.ResourcesAvailable:N0} ({100-occupiedPct:F0}%)", new GUIStyle(_labelStyle) { normal = { textColor = Color.green } });
            GUILayout.Space(6);
            
            // Agent States
            GUILayout.Label("─── AGENT STATES ───", _headerStyle);
            int total = _stats.AgentCount > 0 ? _stats.AgentCount : 1;
            
            DrawStateBar("Idle", _stats.AgentsIdle, total, new Color(0.7f, 0.7f, 0.8f));
            DrawStateBar("Finding", _stats.AgentsFinding, total, Color.yellow);
            DrawStateBar("Moving", _stats.AgentsMoving, total, Color.cyan);
            DrawStateBar("Collecting", _stats.AgentsCollecting, total, new Color(1f, 0.5f, 0f));
            DrawStateBar("Returning", _stats.AgentsReturning, total, Color.green);
            DrawStateBar("Depositing", _stats.AgentsDepositing, total, Color.magenta);
            
            GUILayout.Space(6);
            
            // Controls section
            GUILayout.Label("─── CONTROLS ───", _headerStyle);
            var controlStyle = new GUIStyle(_labelStyle) { fontSize = 13, normal = { textColor = new Color(0.9f, 0.9f, 0.7f) } };
            GUILayout.Label("H - Toggle HUD", controlStyle);
            GUILayout.Label("R - Restart Simulation", controlStyle);
            GUILayout.Label("Space - Pause/Resume", controlStyle);
            GUILayout.Label("-/+ - Slow/Fast", controlStyle);
            GUILayout.Label("0 - Reset Speed", controlStyle);
            GUILayout.Space(6);
            var exitStyle = new GUIStyle(_labelStyle) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.4f, 0.4f) } };
            GUILayout.Label("ESC - Quit", exitStyle);
            
            GUILayout.EndArea();
        }

        private void DrawStateBar(string label, int count, int total, Color color)
        {
            float pct = (float)count / total * 100f;
            
            // Create colored label style
            var coloredStyle = new GUIStyle(_labelStyle)
            {
                normal = { textColor = color },
                fontSize = 13
            };
            
            // Use simple text-based bar
            int barLength = 8;
            int filled = Mathf.RoundToInt(pct / 100f * barLength);
            string bar = new string('█', filled) + new string('░', barLength - filled);
            
            GUILayout.Label($"{label,-10} {bar} {count,5} ({pct,2:F0}%)", coloredStyle);
        }
    }
}

using UnityEngine;
using YuankunHuang.Kodama.Utils;

namespace YuankunHuang.Kodama.Render
{
    /// <summary>
    /// Renders a hex grid using GL.Lines.
    /// Attach this to a GameObject in the scene.
    /// </summary>
    public class HexGridRenderer : MonoBehaviour
    {
        [SerializeField] private Material _lineMaterial;
        [SerializeField] private int _gridRadius = 60;
        [SerializeField] private Color _lineColor = new Color(0.2f, 0.3f, 0.4f, 0.5f);
        
        private static readonly float Sqrt3 = Mathf.Sqrt(3f);
        private const float HexSize = 1f;

        private void OnRenderObject()
        {
            if (_lineMaterial == null) return;
            
            _lineMaterial.SetPass(0);
            
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            GL.Color(_lineColor);
            
            // Draw hexes in axial coordinates
            for (int q = -_gridRadius; q <= _gridRadius; q++)
            {
                int r1 = Mathf.Max(-_gridRadius, -q - _gridRadius);
                int r2 = Mathf.Min(_gridRadius, -q + _gridRadius);
                
                for (int r = r1; r <= r2; r++)
                {
                    DrawHexOutline(q, r);
                }
            }
            
            GL.End();
            GL.PopMatrix();
        }

        private void DrawHexOutline(int q, int r)
        {
            // Get hex center in world space
            Vector3 center = HexUtils.HexToWorld(q, r);
            
            // Pointy-top hex vertices (6 corners)
            // Angle offset for pointy-top: starts at 30 degrees
            for (int i = 0; i < 6; i++)
            {
                float angle1 = Mathf.Deg2Rad * (60f * i + 30f);
                float angle2 = Mathf.Deg2Rad * (60f * ((i + 1) % 6) + 30f);
                
                Vector3 v1 = center + new Vector3(
                    HexSize * Mathf.Cos(angle1),
                    0f,
                    HexSize * Mathf.Sin(angle1)
                );
                Vector3 v2 = center + new Vector3(
                    HexSize * Mathf.Cos(angle2),
                    0f,
                    HexSize * Mathf.Sin(angle2)
                );
                
                GL.Vertex(v1);
                GL.Vertex(v2);
            }
        }
    }
}

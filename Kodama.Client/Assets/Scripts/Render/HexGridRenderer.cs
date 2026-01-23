using UnityEngine;
using UnityEngine.Rendering;
using YuankunHuang.Kodama.Utils;

namespace YuankunHuang.Kodama.Render
{
    /// <summary>
    /// Renders a hex grid using a pre-baked Mesh (zero per-frame allocation).
    /// </summary>
    public class HexGridRenderer : MonoBehaviour
    {
        [SerializeField] private Material _lineMaterial;
        [SerializeField] private int _gridRadius = 60;
        [SerializeField] private Color _lineColor = new Color(0.2f, 0.3f, 0.4f, 0.5f);
        
        private const float HexSize = 1f;
        
        // Pre-computed mesh for GPU rendering
        private Mesh _gridMesh;
        
        // Pre-computed corner offsets (pointy-top hex, 6 corners)
        private static readonly Vector3[] CornerOffsets = new Vector3[6];
        
        static HexGridRenderer()
        {
            // Pre-calculate corner offsets once (pointy-top: starts at 30 degrees)
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i + 30f);
                CornerOffsets[i] = new Vector3(
                    HexSize * Mathf.Cos(angle),
                    0f,
                    HexSize * Mathf.Sin(angle)
                );
            }
        }

        private void Awake()
        {
            BakeGridMesh();
        }

        private void BakeGridMesh()
        {
            // Count hexes
            int hexCount = 0;
            for (int q = -_gridRadius; q <= _gridRadius; q++)
            {
                int r1 = Mathf.Max(-_gridRadius, -q - _gridRadius);
                int r2 = Mathf.Min(_gridRadius, -q + _gridRadius);
                hexCount += (r2 - r1 + 1);
            }
            
            // Each hex has 6 edges, each edge has 2 vertices
            int vertexCount = hexCount * 6 * 2;
            var vertices = new Vector3[vertexCount];
            var indices = new int[vertexCount];
            var colors = new Color[vertexCount];
            
            int idx = 0;
            for (int q = -_gridRadius; q <= _gridRadius; q++)
            {
                int r1 = Mathf.Max(-_gridRadius, -q - _gridRadius);
                int r2 = Mathf.Min(_gridRadius, -q + _gridRadius);
                
                for (int r = r1; r <= r2; r++)
                {
                    Vector3 center = HexUtils.HexToWorld(q, r);
                    
                    for (int i = 0; i < 6; i++)
                    {
                        vertices[idx] = center + CornerOffsets[i];
                        vertices[idx + 1] = center + CornerOffsets[(i + 1) % 6];
                        colors[idx] = _lineColor;
                        colors[idx + 1] = _lineColor;
                        indices[idx] = idx;
                        indices[idx + 1] = idx + 1;
                        idx += 2;
                    }
                }
            }
            
            _gridMesh = new Mesh();
            _gridMesh.name = "HexGrid";
            
            // CRITICAL: Use 32-bit index buffer for meshes with >65535 vertices
            _gridMesh.indexFormat = IndexFormat.UInt32;
            
            _gridMesh.vertices = vertices;
            _gridMesh.colors = colors;
            _gridMesh.SetIndices(indices, MeshTopology.Lines, 0);
            _gridMesh.UploadMeshData(true); // Mark as non-readable to free CPU memory
            
            Debug.Log($"[HexGridRenderer] Baked {hexCount} hexes, {vertexCount} vertices");
        }

        private void OnRenderObject()
        {
            if (_lineMaterial == null || _gridMesh == null) return;
            
            _lineMaterial.SetPass(0);
            Graphics.DrawMeshNow(_gridMesh, transform.localToWorldMatrix);
        }

        private void OnDestroy()
        {
            if (_gridMesh != null)
            {
                Destroy(_gridMesh);
            }
        }
    }
}

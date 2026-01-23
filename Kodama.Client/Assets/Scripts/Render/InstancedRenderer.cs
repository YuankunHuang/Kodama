using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Kodama.Render
{
    public class InstancedRenderer
    {
        private Mesh _mesh;
        private Material _material;
        private Matrix4x4[] _matrices;
        
        public InstancedRenderer(Mesh mesh, Material material)
        {
            _mesh = mesh;
            _material = material;
            _matrices = new Matrix4x4[1023];
        }

        public void Render(Vector3[] positions, Vector3 scale)
        {
            var batchedCount = 0;
            for (var i = 0; i < positions.Length; ++i)
            {
                _matrices[batchedCount] = Matrix4x4.TRS(positions[i], Quaternion.identity, scale);
                ++batchedCount;

                if (batchedCount == _matrices.Length)
                {
                    Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, batchedCount);
                    batchedCount = 0;
                }
            }

            if (batchedCount > 0)
            {
                Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, batchedCount);
            }
        }

        public void RenderList(List<Vector3> positions, Vector3 scale)
        {
            var batchedCount = 0;
            for (var i = 0; i < positions.Count; ++i)
            {
                _matrices[batchedCount] = Matrix4x4.TRS(positions[i], Quaternion.identity, scale);
                ++batchedCount;

                if (batchedCount == _matrices.Length)
                {
                    Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, batchedCount);
                    batchedCount = 0;
                }
            }

            if (batchedCount > 0)
            {
                Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, batchedCount);
            }
        }
    }
}
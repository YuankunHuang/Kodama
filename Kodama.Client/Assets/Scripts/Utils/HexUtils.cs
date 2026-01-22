using UnityEngine;

namespace YuankunHuang.Kodama.Utils
{
    public static class HexUtils
    {
        private const float Sqrt3 = 1.732050808f;
        private const float HexSize = 1f;

        public static Vector3 HexToWorld(int q, int r)
        {
            float x = HexSize * Sqrt3 * (q + r * 0.5f);
            float z = HexSize * 1.5f * r;
            return new Vector3(x, 0, z);
        }
    }
}
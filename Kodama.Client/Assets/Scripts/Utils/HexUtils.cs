using UnityEngine;

namespace YuankunHuang.Kodama.Utils
{
    public static class HexUtils
    {
        private const float Sqrt3Over2 = 0.866025404f;
        private const float HexSize = 1f; // hexagon "radius"

        public static Vector3 HexToWorld(int q, int r)
        {
            float x = HexSize * (q * Sqrt3Over2);
            float y = HexSize * (q * 0.5f - r);
            return new Vector3(x, y, 0);
        }
    }
}
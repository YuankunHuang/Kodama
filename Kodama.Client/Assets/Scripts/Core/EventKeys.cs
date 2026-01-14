namespace YuankunHuang.Kodama.Core
{
    public class EventKeys
    {
        private static int KeyStart = 0;
        public static readonly int SnapshotReceived = GetUniqueKey();

        private static int GetUniqueKey()
        {
            return ++KeyStart;
        }
    }
}
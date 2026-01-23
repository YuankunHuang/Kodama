using System;
using MessagePack;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    [MessagePackObject]
    public struct ResourceSnapshot
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public int Q { get; set; }
        [Key(2)]
        public int R { get; set; }
        [Key(3)]
        public bool IsBeingCollected { get; set; }

        public ResourceSnapshot(int id, int q, int r, bool isBeingCollected)
        {
            Id = id;
            Q = q;
            R = r;
            IsBeingCollected = isBeingCollected;
        }
    }
}

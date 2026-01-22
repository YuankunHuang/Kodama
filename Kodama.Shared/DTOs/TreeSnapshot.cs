using System;
using MessagePack;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    [MessagePackObject]
    public struct TreeSnapshot
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public int Q { get; set; }
        [Key(2)]
        public int R { get; set; }

        public TreeSnapshot(int id, int q, int r)
        {
            Id = id;
            Q = q;
            R = r;
        }
    }
}

using System;
using MessagePack;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    [MessagePackObject]
    public struct AgentSnapshot
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public int Q { get; set; }
        [Key(2)]
        public int R { get; set; }
        [Key(3)]
        public byte State { get; set; }

        public AgentSnapshot(int id, int q, int r, byte state)
        {
            Id = id;
            Q = q;
            R = r;
            State = state;
        }
    }
}

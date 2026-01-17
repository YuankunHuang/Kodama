using System;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    public struct AgentSnapshot
    {
        public Guid Id { get; set; }
        public int Q { get; set; }
        public int R { get; set; }

        public AgentSnapshot(Guid id, int q, int r)
        {
            Id = id;
            Q = q;
            R = r;
        }
    }
}

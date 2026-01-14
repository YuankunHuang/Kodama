using System;

namespace Kodama.Shared.DTOs
{
    [Serializable]
    public struct AgentSnapshot
    {
        public Guid Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public AgentSnapshot(Guid id, float x, float y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }
}

using System;
using Unity.Entities;

namespace Components
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct WaveData : IComponentData
    {
        public float amplitude;
        public float zOffset;
        public float yOffset;
    }
}

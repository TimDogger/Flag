using System;
using Unity.Entities;

namespace Components
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct MoveSpeedData : IComponentData
    {
        public float value;
    }
}

using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace Components
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct ColorData : IComponentData
    {
        public float4 color;
    }
}

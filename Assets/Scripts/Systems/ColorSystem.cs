using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    public class ColorSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            EntityManager entityManager = FlagManager.Instance.entityManager;
            
            Entities.ForEach((Entity entity, ref ColorData colorData, ref MaterialColor materialColor) =>
            {
                colorData.color = new Random(1).NextFloat4();
                materialColor.Value = colorData.color;

            }).Run();
        }
    }
}

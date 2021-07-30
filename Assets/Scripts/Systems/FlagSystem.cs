using Components;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Systems
{
    public class FlagSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float elapsedTime = (float)Time.ElapsedTime;
            
            return Entities.ForEach((ref Translation translation, in MoveSpeedData moveSpeedData, in WaveData waveData) =>
            {
                float xPos = waveData.amplitude * math.sin((float) elapsedTime * moveSpeedData.value +
                                                           translation.Value.z * waveData.zOffset +
                                                           translation.Value.y * waveData.yOffset);
                translation.Value = new float3(xPos, translation.Value.y, translation.Value.z);
            }).Schedule(inputDeps);
        }
    }
}
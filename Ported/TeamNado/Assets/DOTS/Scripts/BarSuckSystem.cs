using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TornadoUpdateSystem))]
public class BarSuckSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem EntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    struct BarSuckSystemJob : IJobForEachWithEntity<UnsuckedBarComponent, BarComponent, Translation>
    {
        public TornadoComponent tor;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        
        [BurstCompile]
        public void Execute(Entity entity, int index, [ReadOnly] ref UnsuckedBarComponent unused, ref BarComponent bar, ref Translation t)
        {
            float tdx = (tor.tornadoPos.x - t.Value.x);
            float tdz = (tor.tornadoPos.z - t.Value.z);

            float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
            if (tornadoDist < TornadoConstants.TornadoMaxForceDistance)
            {
                //bar.velocity.y = TornadoConstants.TornadoUpForce;
                bar.oldX = t.Value.x;
                bar.oldY = t.Value.y;
                bar.oldZ = t.Value.z;
                
                CommandBuffer.RemoveComponent<UnsuckedBarComponent>(index, entity);
                CommandBuffer.AddComponent(index, entity, new SuckedBarComponent());
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var tor = GetSingleton<TornadoComponent>();

        var job = new BarSuckSystemJob();
        job.tor = GetSingleton<TornadoComponent>();
        job.CommandBuffer = EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        var scheduled = job.Schedule(this, inputDependencies);
        EntityCommandBufferSystem.AddJobHandleForProducer(scheduled);
        return scheduled;
    }
}

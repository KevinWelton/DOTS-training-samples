using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class BrickSpawnSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        // Cache the BeginInitializationEntityCommandBufferSystem in a field, so we don't have to create it every frame
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
  
    struct BrickSpawnSystemJob : IJobForEachWithEntity<SpawnComponent, LocalToWorld>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        
        
        [BurstCompile]
        public void Execute(Entity entity, int index, [ReadOnly] ref SpawnComponent SpawnComp,
            [ReadOnly] ref LocalToWorld location)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
            for (var x = 0; x < SpawnComp.Count; x++)
            {
                for (var y = 0; y < SpawnComp.Count; y++)
                {
                    var instance = CommandBuffer.Instantiate(index, SpawnComp.Prefab);

                    // Place the instantiated in a grid with some noise
                    var position = math.transform(location.Value,
                        new float3(x * 1.3F, noise.cnoise(new float2(x, y) * 0.21F) * 2, y * 1.3F));
                    CommandBuffer.SetComponent(index, instance, new Translation {Value = position});
                }
            }

            CommandBuffer.DestroyEntity(index, entity);
            
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;
        
        var job = new BrickSpawnSystemJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDependencies);
        
        // Now that the job is set up, schedule it to be run. 
        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}

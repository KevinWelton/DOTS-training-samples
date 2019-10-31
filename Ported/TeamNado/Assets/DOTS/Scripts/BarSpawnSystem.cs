using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Animations;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using Random = Unity.Mathematics.Random;

public class BarSpawnSystem : JobComponentSystem
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
  
    struct BarSpawnSystemJob : IJobForEachWithEntity<SpawnBarComponent, LocalToWorld>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        
        [BurstCompile]
        public void Execute(Entity entity, int index, [ReadOnly] ref SpawnBarComponent SpawnComp,
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
            
            var random = new Random(1);
            
            // buildings
            for (var i = 0; i < SpawnComp.Count; i++)
            {
                int height = random.NextInt(4, 12);
                float3 pos = new float3(random.NextFloat(-45f, 45f),0f,random.NextFloat(-45f, 45f));
                float spacing = 2f;

                // Make 3 bars at each height.
                for (var j = 0; j < height; j++)
                {
                    float3 point1 = new float3(pos.x + spacing, j * spacing, pos.z - spacing);
                    float3 point2 = new float3(pos.x - spacing, j * spacing, pos.z - spacing);
                    float3 point3 = new float3(pos.x + 0f, j * spacing, pos.z + spacing);
                    float3 yAxis = new float3(0f, 1f, 0f);

                    var instance1 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position1 = math.transform(location.Value, new float3(pos.x, j * spacing, pos.z - spacing));
                    CommandBuffer.SetComponent(index, instance1, new Translation { Value = position1 });
                    CommandBuffer.SetComponent(index, instance1, new Rotation { Value = quaternion.AxisAngle(yAxis, 1.5708f) });
                    CommandBuffer.SetComponent(index, instance1, new BarComponent { point1 = point1, point2 = point2});

                    var instance2 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position2 = math.transform(location.Value, new float3(pos.x - spacing / 2, j * spacing, pos.z));
                    CommandBuffer.SetComponent(index, instance2, new Translation { Value = position2 });
                    CommandBuffer.SetComponent(index, instance2, new Rotation { Value = quaternion.AxisAngle(yAxis, 0.5235f) });
                    CommandBuffer.SetComponent(index, instance1, new BarComponent { point1 = point2, point2 = point3});

                    var instance3 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position3 = math.transform(location.Value, new float3(pos.x + spacing / 2, j * spacing, pos.z));
                    CommandBuffer.SetComponent(index, instance3, new Translation { Value = position3 });
                    CommandBuffer.SetComponent(index, instance3, new Rotation { Value = quaternion.AxisAngle(yAxis, -0.5235f) });
                    CommandBuffer.SetComponent(index, instance1, new BarComponent { point1 = point3, point2 = point1});
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
        
        var job = new BarSpawnSystemJob
        {
            CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDependencies);
        
        // Now that the job is set up, schedule it to be run. 
        m_EntityCommandBufferSystem.AddJobHandleForProducer(job);

        return job;
    }
}

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
                float3 pos = new float3( random.NextFloat(-45f, 45f),1.1f,random.NextFloat(-45f, 45f));
                float spacing = 2f;

                // Make 3 bars at each height.
                for (var j = 0; j < height; j++)
                {
                    float3 point1 = new float3(pos.x + spacing, pos.y + j * spacing, pos.z - spacing);
                    float3 point2 = new float3(pos.x - spacing, pos.y + j * spacing, pos.z - spacing);
                    float3 point3 = new float3(pos.x + 0f, pos.y + j * spacing, pos.z + spacing);

                    float3 yAxis = new float3(0f, 1f, 0f);

/*                    var point1Bar = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var point1BarPosition = math.transform(location.Value, point1);
                    CommandBuffer.SetComponent(index, point1Bar, new Translation { Value = point1BarPosition });
                    
                    var point2Bar = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var point2BarPosition = math.transform(location.Value, point2);
                    CommandBuffer.SetComponent(index, point2Bar, new Translation { Value = point2BarPosition });
                    
                    var point3Bar = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var point3BarPosition = math.transform(location.Value, point3);
                    CommandBuffer.SetComponent(index, point3Bar, new Translation { Value = point3BarPosition });*/
                    
                    // floors
                    var instance1 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position1 = math.transform(location.Value, new float3(pos.x, pos.y + j * spacing, pos.z - spacing));
                    CommandBuffer.SetComponent(index, instance1, new Translation { Value = position1 });
                    CommandBuffer.SetComponent(index, instance1, new Rotation { Value = quaternion.AxisAngle(yAxis, 1.5708f) });
                    CommandBuffer.SetComponent(index, instance1, new BarComponent {});

                    var instance2 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position2 = math.transform(location.Value, new float3(pos.x - spacing / 2, pos.y + j * spacing, pos.z));
                    CommandBuffer.SetComponent(index, instance2, new Translation { Value = position2 });
                    CommandBuffer.SetComponent(index, instance2, new Rotation { Value = quaternion.AxisAngle(yAxis, 0.5235f) });
                    CommandBuffer.SetComponent(index, instance2, new BarComponent { });

                    var instance3 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position3 = math.transform(location.Value, new float3(pos.x + spacing / 2, pos.y + j * spacing, pos.z));
                    CommandBuffer.SetComponent(index, instance3, new Translation { Value = position3 });
                    CommandBuffer.SetComponent(index, instance3, new Rotation { Value = quaternion.AxisAngle(yAxis, -0.5235f) });
                    CommandBuffer.SetComponent(index, instance3, new BarComponent { });

                    if (j == 0)
                        continue;
                    
                    float3 point1_1 = new float3(pos.x + spacing, pos.y + (j - 1) * spacing, pos.z - spacing);
                    float3 point2_2 = new float3(pos.x - spacing, pos.y + (j - 1) * spacing, pos.z - spacing);
                    float3 point3_3 = new float3(pos.x + 0f, pos.y + (j-1) * spacing, pos.z + spacing);

                    // horizontal connections
                    var instance5 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position5 = math.transform(location.Value, new float3(pos.x, pos.y + (j * 2 - 1) * spacing / 2, pos.z - spacing));
                    CommandBuffer.SetComponent(index, instance5, new Translation { Value = position5 });
                    CommandBuffer.SetComponent(index, instance5, new Rotation { Value = quaternion.Euler(-0.4712f, 1.5708f, 0f)});
                    CommandBuffer.SetComponent(index, instance5, new BarComponent { });

                    var instance6 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position6 = math.transform(location.Value, new float3(pos.x, pos.y + (j * 2 - 1) * spacing / 2, pos.z - spacing));
                    CommandBuffer.SetComponent(index, instance6, new Translation { Value = position6 });
                    CommandBuffer.SetComponent(index, instance6, new Rotation { Value = quaternion.Euler(0.4712f, 1.5708f, 0f)});
                    CommandBuffer.SetComponent(index, instance6, new BarComponent { });

                    var instance7 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position7 = math.transform(location.Value, new float3(pos.x - spacing / 2, pos.y + (j * 2 - 1) * spacing / 2, pos.z));
                    CommandBuffer.SetComponent(index, instance7, new Translation { Value = position7 });
                    CommandBuffer.SetComponent(index, instance7, new Rotation { Value = quaternion.Euler(-0.4712f, 0.5235f, 0f)});
                    CommandBuffer.SetComponent(index, instance7, new BarComponent { });
                    
                    var instance8 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position8 = math.transform(location.Value, new float3(pos.x - spacing / 2, pos.y + (j * 2 - 1) * spacing / 2, pos.z));
                    CommandBuffer.SetComponent(index, instance8, new Translation { Value = position8 });
                    CommandBuffer.SetComponent(index, instance8, new Rotation { Value = quaternion.Euler(0.4712f, 0.5235f, 0f)});
                    CommandBuffer.SetComponent(index, instance8, new BarComponent { });

                    var instance9 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position9 = math.transform(location.Value, new float3(pos.x + spacing / 2, pos.y + (j * 2 - 1) * spacing / 2, pos.z));
                    CommandBuffer.SetComponent(index, instance9, new Translation { Value = position9 });
                    CommandBuffer.SetComponent(index, instance9, new Rotation { Value = quaternion.Euler(-0.4712f, -0.5235f, 0f)});
                    CommandBuffer.SetComponent(index, instance9, new BarComponent { });

                    var instance10 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position10 = math.transform(location.Value, new float3(pos.x + spacing / 2, pos.y + (j * 2 - 1) * spacing / 2, pos.z));
                    CommandBuffer.SetComponent(index, instance10, new Translation { Value = position10 });
                    CommandBuffer.SetComponent(index, instance10, new Rotation { Value = quaternion.Euler(0.4712f, -0.5235f, 0f)});
                    CommandBuffer.SetComponent(index, instance10, new BarComponent { });
                    
                    // vertical connections
                    var instance11 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position11 = math.transform(location.Value, new float3(pos.x + spacing, pos.y + (j * 2 - 1) * spacing / 2, pos.z - spacing));
                    CommandBuffer.SetComponent(index, instance11, new Translation { Value = position11 });
                    CommandBuffer.SetComponent(index, instance11, new Rotation { Value = quaternion.Euler(1.5708f, 1.5708f, 0f)});
                    CommandBuffer.SetComponent(index, instance11, new BarComponent { });    
                    
                    var instance12 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position12 = math.transform(location.Value, new float3(pos.x - spacing, pos.y + (j * 2 - 1) * spacing / 2, pos.z - spacing));
                    CommandBuffer.SetComponent(index, instance12, new Translation { Value = position12 });
                    CommandBuffer.SetComponent(index, instance12, new Rotation { Value = quaternion.Euler(1.5708f, 0.5235f, 0f)});
                    CommandBuffer.SetComponent(index, instance12, new BarComponent { }); 
                    
                    var instance13 = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                    var position13 = math.transform(location.Value, new float3(pos.x + 0f, pos.y + (j * 2 - 1) * spacing / 2, pos.z + spacing));
                    CommandBuffer.SetComponent(index, instance13, new Translation { Value = position13 });
                    CommandBuffer.SetComponent(index, instance13, new Rotation { Value = quaternion.Euler(1.5708f, -0.5235f, 0f)});
                    CommandBuffer.SetComponent(index, instance13, new BarComponent { });
                }
            }
            
            // ground details
            for (var i = 0; i < 600; i++)
            {
                float3 pos = new float3(random.NextFloat(-55f, 55f),0.1f,random.NextFloat(-55f, 55f));
                var instance = CommandBuffer.Instantiate(index, SpawnComp.Prefab);
                var position = math.transform(location.Value, new float3(pos.x + random.NextFloat(-.2f, -.1f), 0.1f, pos.z + random.NextFloat(.1f, .2f)));
                CommandBuffer.SetComponent(index, instance, new Translation { Value = position });
                CommandBuffer.SetComponent(index, instance, new Rotation { Value = quaternion.Euler(0f, random.NextFloat(0f, 6.2832f), 0f)});
                CommandBuffer.SetComponent(index, instance, new BarComponent {  });
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

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class TornadoUpdateSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    struct TornadoUpdateSystemJob : IJobForEach<TornadoComponent, Translation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;

        public float time;
        public float deltaTime;
        
        [BurstCompile]
        public void Execute(ref TornadoComponent tornadoComp, ref Translation translation)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;

            //clutterComp.velocity = translation.Value - new float3(0f,0f,0f);
            //translation.Value = clutterComp.velocity * deltaTime;
/*            var tornadoX = math.cos(time / 6f) * 30f;
            var tornadoZ = math.sin(time / 6f * 1.618f) * 30f;
            tornadoComp.tornadoPos.x = tornadoX;
            tornadoComp.tornadoPos.z = tornadoZ;*/

            tornadoComp.tornadoPos = new float3(
                Mathf.Cos(time / 6f) * 30f + Mathf.Sin((translation.Value.y) / 5f + time / 4f) * 3f,
                translation.Value.y,
                Mathf.Sin(time / 6f * 1.618f) * 30f);
            
            //translation.Value += new float3(tornadoX, 0f, tornadoZ);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new TornadoUpdateSystemJob();
        
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;

        job.time = UnityEngine.Time.time;
        job.deltaTime = UnityEngine.Time.deltaTime;

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}
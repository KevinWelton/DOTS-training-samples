﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

public class ClutterUpdateSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    struct ClutterUpdateSystemJob : IJobForEach<ClutterComponent, Translation, Rotation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;

        public float deltaTime;
        public float Time;
        public TornadoComponent tornadoComp;
        
        [BurstCompile]
        public void Execute(ref ClutterComponent clutterComp, ref Translation translation, ref Rotation rotation)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;

            float3 tornadoPos = new float3(
                Mathf.Cos(Time / 6f) * 30f + Mathf.Sin((translation.Value.y) / 5f + Time / 4f) * 3f,
                translation.Value.y,
                Mathf.Sin(Time / 6f * 1.618f) * 30f);
            
            var delta = tornadoPos - translation.Value;
            float dist = math.length(delta);
            delta /= dist;
            float inForce = dist - Mathf.Clamp01(tornadoPos.y / 50f) * 30f * 0.5f + 2f;
            translation.Value += new float3(-delta.z * 30 + delta.x * inForce, TornadoConstants.UpForce, delta.x * 30 + delta.z * inForce) * deltaTime;

            if (translation.Value.y > 50f)
            {
                translation.Value = new float3(translation.Value.x, translation.Value.y - 50f, translation.Value.z);
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ClutterUpdateSystemJob();
        
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;

        job.deltaTime = UnityEngine.Time.deltaTime;
        job.Time = UnityEngine.Time.time;
        job.tornadoComp = GetSingleton<TornadoComponent>();
        
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}
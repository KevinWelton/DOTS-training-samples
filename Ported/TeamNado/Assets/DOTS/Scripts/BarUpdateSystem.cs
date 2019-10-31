using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

public class BarUpdateSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    struct BarUpdateSystemJob : IJobForEach<BarComponent, Translation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;

        public float deltaTime;
        
        [BurstCompile]
        public void Execute(ref BarComponent barComp, ref Translation translation)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
            float3 magicTornado = new Vector3(0,0,0);
            //barComp.velocity 
            
            
            //dx is x distance to tornado
            //dz is z distance to tornado
            
	        //this is raw code i ported
            /*
             * // tornado force. The tornado does not seem to actually pull differently at differet heights. It 
					//   DOES however have sway, which is a sine wave that will act as an offset at different heights.
					//   The sine wave does not appear to rotate though. If we could do that it would look even better.
					float tdx = tornadoX+TornadoSway(point.y) - point.x;
					float tdz = tornadoZ - point.z;
					float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
					tdx /= tornadoDist;
					tdz /= tornadoDist;
					// If the tornado is too far away, don't consider it as a force at all.
					if (tornadoDist<tornadoMaxForceDist) {
						float force = (1f - tornadoDist / tornadoMaxForceDist);
						float yFader= Mathf.Clamp01(1f - point.y / tornadoHeight);
						// See above where tornadoFader is defined. Early on, this makes the tornado weaker by
						//   multiplying the normal force (tornadoForce*Random.Range(-.3f, 1.3f) by a value that starts
						//   at 0 early and then builds up over the next 5-10 seconds, clamping at 1.
						force *= tornadoFader*tornadoForce*Random.Range(-.3f,1.3f);
						float forceY = tornadoUpForce;
						point.oldY -= forceY * force;
						float forceX = -tdz + tdx * tornadoInwardForce*yFader;
						float forceZ = tdx + tdz * tornadoInwardForce*yFader;
						point.oldX -= forceX * force;
						point.oldZ -= forceZ * force;
					}

					point.x += (point.x - point.oldX) * invDamping;
					point.y += (point.y - point.oldY) * invDamping;
					point.z += (point.z - point.oldZ) * invDamping;

					point.oldX = startX;
					point.oldY = startY;
					point.oldZ = startZ;
					if (point.y < 0f) {
						point.y = 0f;
						point.oldY = -point.oldY;
						point.oldX += (point.x - point.oldX) * friction;
						point.oldZ += (point.z - point.oldZ) * friction;
					}
             */
            var random = new Random(1);
	        float startX = translation.Value.x;
	        float startY = translation.Value.y;
	        float startZ = translation.Value.z;
	        
	        float tdx = magicTornado.x - translation.Value.x;
	        float tdz = magicTornado.z - translation.Value.z;
	        float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
	        //tdx and tdz are the components of a unit vector towards the tornado
	        
	        tdx /= tornadoDist;
	        tdz /= tornadoDist;
	        // If the tornado is too far away, don't consider it as a force at all.
	        if (tornadoDist<TornadoConstants.TornadoMaxForceDistance) {
		        float force = (1f - tornadoDist / TornadoConstants.TornadoMaxForceDistance);
		        float yFader= Mathf.Clamp01(1f - translation.Value.y / TornadoConstants.TornadoHeight);
		        // See above where tornadoFader is defined. Early on, this makes the tornado weaker by
		        //   multiplying the normal force (tornadoForce*Random.Range(-.3f, 1.3f) by a value that starts
		        //   at 0 early and then builds up over the next 5-10 seconds, clamping at 1.
		        force *= TornadoConstants.InwardForce * random.NextFloat(-.3f, 1.3f);//Range(-.3f,1.3f);
		        float forceY = TornadoConstants.UpForce;
		        
		        
		        //OLD VALUES NOT REALLY USED!!!
		        
		        barComp.oldY -= forceY * force;
		        float forceX = -tdz + tdx * TornadoConstants.InwardForce*yFader;
		        float forceZ = tdx + tdz * TornadoConstants.InwardForce*yFader;
		        barComp.oldX -= forceX * force;
		        barComp.oldZ -= forceZ * force;
		        //
		        
		        //IF SOMETHING IS WRONG LOOK HERE!
		        //barComp.velocity.x += forceX * deltaTime;
		        //barComp.velocity.y += forceY * deltaTime;
		        //barComp.velocity.z += forceZ * deltaTime;
	        }
			//the previous example found the change in position since last frame, which is the velocity times time
	        //translation.Value.x = (barComp.velocity.x * deltaTime) * (1f - TornadoConstants.Damping);
	        //translation.Value.y = (barComp.velocity.y * deltaTime) * (1f - TornadoConstants.Damping);
	        //translation.Value.z = (barComp.velocity.z * deltaTime) * (1f - TornadoConstants.Damping);
	        translation.Value.x += (translation.Value.x - barComp.oldX) * (1f - TornadoConstants.Damping);
	        translation.Value.y += (translation.Value.y - barComp.oldY) * (1f - TornadoConstants.Damping);
	        translation.Value.z += (translation.Value.z - barComp.oldZ) * (1f - TornadoConstants.Damping);

	        barComp.oldX = startX;
	        barComp.oldY = startY;
	        barComp.oldZ = startZ;
	        if (translation.Value.y < 0f) {
		        translation.Value.y = 0f;
		        barComp.oldY = -barComp.oldY;
		        barComp.oldX += (translation.Value.x - barComp.oldX) * TornadoConstants.Friction;
		        barComp.oldZ += (translation.Value.z - barComp.oldZ) * TornadoConstants.Friction;
	        }
	        
	        
	        
	        /*
	         * if (point.y < 0f) {
						point.y = 0f;
						point.oldY = -point.oldY;
						point.oldX += (point.x - point.oldX) * friction;
						point.oldZ += (point.z - point.oldZ) * friction;
					}
	         */
	        
	        

        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new BarUpdateSystemJob();
        
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;

        job.deltaTime = UnityEngine.Time.deltaTime;
        
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}
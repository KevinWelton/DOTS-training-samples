//using System.Data.OleDb;
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
    struct BarUpdateSystemJob : IJobForEach<BarComponent,TornadoComponent,  Translation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;

        public float deltaTime;
	    
        
        [BurstCompile]
        public void Execute(ref BarComponent barComp, ref TornadoComponent tornadoComp, ref Translation translation)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
            
            //barComp.velocity 
            
            
            //dx is x distance to tornado
            //dz is z distance to tornado
            //translation.Value += new float3(-delta.z * tornado.spinRate + delta.x * inForce, tornado.upwardSpeed, delta.x * tornado.spinRate + delta.z * inForce) * deltaTime;
	        
	        
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

	        float shearX = 0f;
	        float shearZ = 0f;
	        
	        float tdx = (tornadoComp.tornadoPos.x - translation.Value.x);
	        float tdz = (tornadoComp.tornadoPos.z - translation.Value.z);
	        float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
	        //tdx and tdz are the components of a unit vector towards the tornado
	        
	        //at the beginning of the frame, the bar undergoes gravity
	        //barComp.velocity.y = TornadoConstants.Gravity;
	        //float yAccel = TornadoConstants.Gravity;
	        
	        //distance in the x and z direction from the nado
	        tdx /= tornadoDist;
	        tdz /= tornadoDist;

	        float forceY = 0;
	        if (translation.Value.y > 0.1)
	        {
		        forceY += TornadoConstants.Gravity;
	        }
	        // If the tornado is too far away, don't consider it as a force at all.
	        if (tornadoDist<TornadoConstants.TornadoMaxForceDistance) {
		        
		        float force = (1f - tornadoDist / TornadoConstants.TornadoMaxForceDistance);
		        float yFader= Mathf.Clamp01(1f - translation.Value.y / TornadoConstants.TornadoHeight);
		        // See above where tornadoFader is defined. Early on, this makes the tornado weaker by
		        //   multiplying the normal force (tornadoForce*Random.Range(-.3f, 1.3f) by a value that starts
		        //   at 0 early and then builds up over the next 5-10 seconds, clamping at 1.
		        force *= 0.22f * random.NextFloat(-.3f, 1.3f);//Range(-.3f,1.3f);
		        
		        
		        
		        
		        //if (force > TornadoConstants.MaxForce)
		        //{
			    //    force = TornadoConstants.MaxForce;
		        //}
		        
		        //force is now the force we want applied to the bars
		        
		        
		        float forceX = tdx * force * TornadoConstants.InwardForce * yFader;//-tdz + tdx * force * TornadoConstants.InwardForce * yFader;
		        float forceZ = tdz * force * TornadoConstants.InwardForce * yFader;//tdx + tdz * force  * TornadoConstants.InwardForce *yFader;
		        
		        
		        
		        //OK, tornadoes do a little bit of shear force to make it spin right round
		        shearX += -tdz + tdx *  force * 0.0001f;
		        shearZ += tdx + tdz * force* 0.0001f;
		        
		        
		        //forceX and forceZ are the component force vectors we want applied

		        
		        if (translation.Value.y < TornadoConstants.TornadoHeight)
		        {
			        forceY += TornadoConstants.UpForce * Mathf.Clamp(( 7/tornadoDist),0,1);
			        //forceY = TornadoConstants.UpForce * (1/tornadoDist);//TornadoConstants.TornadoMaxForceDistance);
		        }

		        //forceY += yAccel;

		        barComp.velocity.x += forceX;
		        barComp.velocity.z += forceZ;
		        barComp.velocity.y += forceY;
		        //OLD VALUES NOT REALLY USED!!!

				

		        //float3 tempForce = new float3(-tdz + tdx, forceY, tdx + tdz) * force;
		        //barComp.oldX -= forceX;
		        //barComp.oldZ -= forceZ;
		        //technically new position
		        //barComp.oldX -= forceX * deltaTime;
		        //barComp.oldY -= forceZ * deltaTime;
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
			else
	        {
		        if (barComp.velocity.x > 0)
		        {
			        barComp.velocity.x -= TornadoConstants.Friction;
		        }
		        else if (barComp.velocity.x < 0)
		        {
			        barComp.velocity.x += TornadoConstants.Friction;
		        }
		        if (barComp.velocity.z> 0)
		        {
			        barComp.velocity.z -= TornadoConstants.Friction;
		        }
		        else if (barComp.velocity.z < 0)
		        {
			        barComp.velocity.z += TornadoConstants.Friction;
		        }
		        //barComp.velocity.x = 0;
		        //barComp.velocity.z =0;
	        }

	        //gravity is always applied
	        barComp.velocity.y += forceY;
	        //barComp.velocity.x += shearX;
	        //barComp.velocity.z += shearZ;

	        barComp.velocity.x *=  (1f - TornadoConstants.Damping);
	        barComp.velocity.z *=  (1f - TornadoConstants.Damping);
	        //this is dumb.
	        translation.Value.x -= barComp.velocity.x * deltaTime; //* (1f - TornadoConstants.Damping);//(translation.Value.x - barComp.oldX) * (1f - TornadoConstants.Damping);
	        translation.Value.y += barComp.velocity.y * deltaTime;//* (1f - TornadoConstants.Damping);//(translation.Value.y - barComp.oldY) * (1f - TornadoConstants.Damping);
	        translation.Value.z -= barComp.velocity.z * deltaTime; //* (1f - TornadoConstants.Damping);//(translation.Value.z - barComp.oldZ) * (1f - TornadoConstants.Damping);

	        //translation.Value.x -= shearX* (1f - TornadoConstants.Damping);
	        // translation.Value.z -= shearZ* (1f - TornadoConstants.Damping);
			
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
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

//[UpdateInGroup(typeof(SimulationSystemGroup))]
//[UpdateAfter(typeof(BarSuckSystem))]
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
	[BurstCompile]
    struct BarUpdateSystemJob : IJobForEach<BarComponent, SuckedBarComponent, Translation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;

        public float deltaTime;
	    public float time;
        public TornadoComponent tornadoComp;
        public Random random;
        
        public void Execute(ref BarComponent barComp, [ReadOnly] ref SuckedBarComponent unused, ref Translation translation)
        {
		// There are two phases: world generation and not world generation.
		//if (generating == false) {
			// I thought "why clamp this?" so I took out the clamp call. Leave it clamped. This value slowly grows over
			//   time. Once it gets to 1, the tornado is at "max strength". Increasing it further makes the tornado
			//   cat 11.
			//tornadoFader = Mathf.Clamp01(tornadoFader + Time.deltaTime / 10f);

			float invDamping = 1f - TornadoConstants.Damping;

			var start = translation;

			barComp.oldY += .01f;

			// tornado force. The tornado does not seem to actually pull differently at differet heights. It 
			//   DOES however have sway, which is a sine wave that will act as an offset at different heights.
			//   The sine wave does not appear to rotate though. If we could do that it would look even better.
			float tdx = tornadoComp.tornadoPos.x - translation.Value.x;
			float tdz = tornadoComp.tornadoPos.z - translation.Value.z;
			float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
			tdx /= tornadoDist;
			tdz /= tornadoDist;
			// If the tornado is too far away, don't consider it as a force at all.
			if (tornadoDist<TornadoConstants.TornadoMaxForceDistance) {
				float force = (1f - tornadoDist / TornadoConstants.TornadoMaxForceDistance);
				float yFader= Mathf.Clamp01(1f - translation.Value.y / TornadoConstants.TornadoHeight);
				// See above where tornadoFader is defined. Early on, this makes the tornado weaker by
				//   multiplying the normal force (tornadoForce*Random.Range(-.3f, 1.3f) by a value that starts
				//   at 0 early and then builds up over the next 5-10 seconds, clamping at 1.
				force *= TornadoConstants.TornadoForce*random.NextFloat(-.3f,1.3f);
				float forceY = TornadoConstants.TornadoUpForce;
				barComp.oldY -= forceY * force;
				float forceX = -tdz + tdx;// * tornadoInwardForce*yFader;
				float forceZ = tdx + tdz;// * tornadoInwardForce*yFader;
				barComp.oldX -= forceX * force;
				barComp.oldZ -= forceZ * force;
			}

			translation.Value.x += (translation.Value.x - barComp.oldX) * invDamping;
			translation.Value.y += (translation.Value.y - barComp.oldY) * invDamping;
			translation.Value.z += (translation.Value.z - barComp.oldZ) * invDamping;

			barComp.oldX = start.Value.x;
			barComp.oldY = start.Value.y;
			barComp.oldZ = start.Value.z;
			if (translation.Value.y < 0f) {
				translation.Value.y = 0f;
				barComp.oldY = -barComp.oldY;
				barComp.oldX += (translation.Value.x - barComp.oldX) * TornadoConstants.Friction;
				barComp.oldZ += (translation.Value.z - barComp.oldZ) * TornadoConstants.Friction;
			}


//					Point point1 = bar.point1;
//				Point point2 = bar.point2;
//				//the length of the bar
//				float dx = point2.x - point1.x;
//				float dy = point2.y - point1.y;
//				float dz = point2.z - point1.z;
//				//dist is length of the bar after ALL forces were applied to it
//				float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
//				// Will these 4 deformations values ever be nonzero in flight, or only when the building hits the
//				// extra dist is the amount of stretch
//				float extraDist = dist - bar.length;
//
//				float pushX = (dx / dist * extraDist) * .5f;
//				float pushY = (dy / dist * extraDist) * .5f;
//				float pushZ = (dz / dist * extraDist) * .5f;
//
//				if (point1.anchor == false && point2.anchor == false) {
//					point1.x += pushX;
//					point1.y += pushY;
//					point1.z += pushZ;
//					point2.x -= pushX;
//					point2.y -= pushY;
//					point2.z -= pushZ;
//				} else if (point1.anchor) {
//					point2.x -= pushX*2f;
//					point2.y -= pushY*2f;
//					point2.z -= pushZ*2f;
//				} else if (point2.anchor) {
//					point1.x += pushX*2f;
//					point1.y += pushY*2f;
//					point1.z += pushZ*2f;
//				}
//
//				if (dx/dist * bar.oldDX + dy/dist*bar.oldDY + dz/dist*bar.oldDZ<.99f) {
//					// bar has rotated: expensive full-matrix computation
//					bar.matrix = Matrix4x4.TRS(new Vector3((point1.x + point2.x) * .5f,(point1.y + point2.y) * .5f,(point1.z + point2.z) * .5f),
//										   Quaternion.LookRotation(new Vector3(dx,dy,dz)),
//										   new Vector3(bar.thickness,bar.thickness,bar.length));
//					bar.oldDX = dx / dist;
//					bar.oldDY = dy / dist;
//					bar.oldDZ = dz / dist;
//				} else {
//					// bar hasn't rotated: only update the position elements
//					Matrix4x4 matrix = bar.matrix;
//					matrix.m03 = (point1.x + point2.x) * .5f;
//					matrix.m13 = (point1.y + point2.y) * .5f;
//					matrix.m23 = (point1.z + point2.z) * .5f;
//					bar.matrix = matrix;
				}

//				if (Mathf.Abs(extraDist) > breakResistance) {
//					if (point2.neighborCount>1) {
//						point2.neighborCount--;
//						Point newPoint = new Point();
//						newPoint.CopyFrom(point2);
//						newPoint.neighborCount = 1;
//						points[pointCount] = newPoint;
//						bar.point2 = newPoint;
//						pointCount++;
//					} else if (point1.neighborCount>1) {
//						point1.neighborCount--;
//						Point newPoint = new Point();
//						newPoint.CopyFrom(point1);
//						newPoint.neighborCount = 1;
//						points[pointCount] = newPoint;
//						bar.point1 = newPoint;
//						pointCount++;
//					}
//				}

//				//these values dont get referenced ever...
//				//nothing to see here, move along
//				bar.minX = Mathf.Min(point1.x,point2.x);
//				bar.maxX = Mathf.Max(point1.x,point2.x);
//				bar.minY = Mathf.Min(point1.y,point2.y);
//				bar.maxY = Mathf.Max(point1.y,point2.y);
//				bar.minZ = Mathf.Min(point1.z,point2.z);
//				bar.maxZ = Mathf.Max(point1.z,point2.z);
//
//				matrices[i / instancesPerBatch][i % instancesPerBatch] = bar.matrix;
	    //}
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new BarUpdateSystemJob();
        
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;

        job.deltaTime = UnityEngine.Time.deltaTime;
	    job.time = UnityEngine.Time.time;
        job.tornadoComp = GetSingleton<TornadoComponent>();
        job.random = new Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}
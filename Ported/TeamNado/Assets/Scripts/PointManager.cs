﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointManager : MonoBehaviour {
	public Material barMaterial;
	public Mesh barMesh;
	// Be default this is set to .012 in the project. And it pretty much has to be that unless you want weird behavior.
	[Range(0f,1f)]
	public float damping;
	[Range(0f,1f)]
	public float friction;
	public float breakResistance;
	public float expForce;
	[Range(0f,1f)]
	public float tornadoForce;
	public float tornadoMaxForceDist;
	public float tornadoHeight;
	public float tornadoUpForce;
	public float tornadoInwardForce;

	Point[] points;
	Bar[] bars;
	public int pointCount;

	bool generating;
	public static float tornadoX;
	public static float tornadoZ;

	float tornadoFader = 0f;

	Matrix4x4[][] matrices;
	MaterialPropertyBlock[] matProps;

	Transform cam;

	const int instancesPerBatch = 1023;

	private void Awake() {
		Time.timeScale = 0f;
	}
	void Start() {
		StartCoroutine(Generate());
		cam = Camera.main.transform;
	}

	// The magic sine wave. We should use this I think. I defer to the tornado master.
	public static float TornadoSway(float y) {
		return Mathf.Sin(y / 5f + Time.time/4f) * 3f;
	}
	
	IEnumerator Generate() {
		generating = true;

		List<Point> pointsList = new List<Point>();
		List<Bar> barsList = new List<Bar>();
		List<List<Matrix4x4>> matricesList = new List<List<Matrix4x4>>();
		matricesList.Add(new List<Matrix4x4>());

		// buildings. Make 36 of these. Only the vertices are stored, and not as part of a building. Just a big list of vertices.
		// 	Pick  random point to start with and go from there.
		for (int i = 0; i < 35; i++) {
			int height = Random.Range(4,12);
			Vector3 pos = new Vector3(Random.Range(-45f,45f),0f,Random.Range(-45f,45f));
			float spacing = 2f;
			
			// Make 3 points at each height.
			for (int j = 0; j < height; j++) {
				Point point = new Point();
				point.x = pos.x+spacing;
				point.y = j * spacing;
				point.z = pos.z-spacing;
				point.oldX = point.x;
				point.oldY = point.y;
				point.oldZ = point.z;
				
				// Height of 0 means anchored to the ground.
				if (j==0) {
					point.anchor=true;
				}
				pointsList.Add(point);
				point = new Point();
				point.x = pos.x-spacing;
				point.y = j * spacing;
				point.z = pos.z-spacing;
				point.oldX = point.x;
				point.oldY = point.y;
				point.oldZ = point.z;
				if (j==0) {
					point.anchor=true;
				}
				pointsList.Add(point);
				point = new Point();
				point.x = pos.x+0f;
				point.y = j * spacing;
				point.z = pos.z+spacing;
				point.oldX = point.x;
				point.oldY = point.y;
				point.oldZ = point.z;
				if (j==0) {
					point.anchor=true;
				}
				pointsList.Add(point);
			}
		}

		// ground details. Add 600 pairs of points that form ground clutter. Points have no explicit connection.
		for (int i=0;i<600;i++) {
			Vector3 pos = new Vector3(Random.Range(-55f,55f),0f,Random.Range(-55f,55f));
			Point point = new Point();
			point.x = pos.x + Random.Range(-.2f,-.1f);
			point.y = pos.y+Random.Range(0f,3f);
			point.z = pos.z + Random.Range(.1f,.2f);
			point.oldX = point.x;
			point.oldY = point.y;
			point.oldZ = point.z;
			pointsList.Add(point);

			point = new Point();
			point.x = pos.x + Random.Range(.2f,.1f);
			point.y = pos.y + Random.Range(0f,.2f);
			point.z = pos.z + Random.Range(-.1f,-.2f);
			point.oldX = point.x;
			point.oldY = point.y;
			point.oldZ = point.z;
			if (Random.value<.1f) {
				point.anchor = true;
			}
			pointsList.Add(point);
		}

		int batch = 0;

		// n^2 fun. Go through and determine number of neighbors for a point by finding out how many other points are within a length threshold (bar length)
		for (int i = 0; i < pointsList.Count; i++) {
			for (int j = i + 1; j < pointsList.Count; j++) {
				Bar bar = new Bar();
				bar.AssignPoints(pointsList[i],pointsList[j]);
				if (bar.length < 5f && bar.length>.2f) {
					bar.point1.neighborCount++;
					bar.point2.neighborCount++;

					barsList.Add(bar);
					matricesList[batch].Add(bar.matrix);
					if (matricesList[batch].Count == instancesPerBatch) {
						batch++;
						matricesList.Add(new List<Matrix4x4>());
					}
					
					// Return after 500 bars are calculated so we can get started. Lame.
					if (barsList.Count % 500 == 0) {
						yield return null;
					}
				}
			}
		}
		
		// Create a NEW points list that only has points with neighbors that were in range to make a bar.
		points = new Point[barsList.Count * 2];
		pointCount = 0;
		for (int i=0;i<pointsList.Count;i++) {
			if (pointsList[i].neighborCount > 0) {
				points[pointCount] = pointsList[i];
				pointCount++;
			}
		}
		Debug.Log(pointCount + " points, room for " + points.Length + " (" + barsList.Count + " bars)");

		bars = barsList.ToArray();

		matrices = new Matrix4x4[matricesList.Count][];
		for (int i=0;i<matrices.Length;i++) {
			matrices[i] = matricesList[i].ToArray();
		}

		matProps = new MaterialPropertyBlock[barsList.Count];
		Vector4[] colors = new Vector4[instancesPerBatch];
		for (int i=0;i<barsList.Count;i++) {
			colors[i%instancesPerBatch] = barsList[i].color;
			if ((i + 1) % instancesPerBatch == 0 || i == barsList.Count - 1) {
				MaterialPropertyBlock block = new MaterialPropertyBlock();
				block.SetVectorArray("_Color",colors);
				matProps[i / instancesPerBatch] = block;
			}
		}

		pointsList = null;
		barsList = null;
		matricesList = null;
		System.GC.Collect();
		generating = false;
		Time.timeScale = 1f;
	}
	
	// Physics crap
	void FixedUpdate () {
		// There are two phases: world generation and not world generation.
		if (generating == false) {
			// I thought "why clamp this?" so I took out the clamp call. Leave it clamped. This value slowly grows over
			//   time. Once it gets to 1, the tornado is at "max strength". Increasing it further makes the tornado
			//   cat 11.
			tornadoFader = Mathf.Clamp01(tornadoFader + Time.deltaTime / 10f);

			float invDamping = 1f - damping;
			for (int i = 0; i < pointCount; i++) {
				Point point = points[i];
				if (point.anchor == false) {
					float startX = point.x;
					float startY = point.y;
					float startZ = point.z;

					point.oldY += .01f;

					// tornado force. The tornado does not seem to actually pull differently at differet heights. It 
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
				}
			}

			
			for (int i = 0; i < bars.Length; i++) {
				Bar bar = bars[i];

				Point point1 = bar.point1;
				Point point2 = bar.point2;
				//the length of the bar
				float dx = point2.x - point1.x;
				float dy = point2.y - point1.y;
				float dz = point2.z - point1.z;
				//dist is length of the bar after ALL forces were applied to it
				float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
				// Will these 4 deformations values ever be nonzero in flight, or only when the building hits the
				// extra dist is the amount of stretch
				float extraDist = dist - bar.length;

				float pushX = (dx / dist * extraDist) * .5f;
				float pushY = (dy / dist * extraDist) * .5f;
				float pushZ = (dz / dist * extraDist) * .5f;

				if (point1.anchor == false && point2.anchor == false) {
					point1.x += pushX;
					point1.y += pushY;
					point1.z += pushZ;
					point2.x -= pushX;
					point2.y -= pushY;
					point2.z -= pushZ;
				} else if (point1.anchor) {
					point2.x -= pushX*2f;
					point2.y -= pushY*2f;
					point2.z -= pushZ*2f;
				} else if (point2.anchor) {
					point1.x += pushX*2f;
					point1.y += pushY*2f;
					point1.z += pushZ*2f;
				}

				if (dx/dist * bar.oldDX + dy/dist*bar.oldDY + dz/dist*bar.oldDZ<.99f) {
					// bar has rotated: expensive full-matrix computation
					bar.matrix = Matrix4x4.TRS(new Vector3((point1.x + point2.x) * .5f,(point1.y + point2.y) * .5f,(point1.z + point2.z) * .5f),
										   Quaternion.LookRotation(new Vector3(dx,dy,dz)),
										   new Vector3(bar.thickness,bar.thickness,bar.length));
					bar.oldDX = dx / dist;
					bar.oldDY = dy / dist;
					bar.oldDZ = dz / dist;
				} else {
					// bar hasn't rotated: only update the position elements
					Matrix4x4 matrix = bar.matrix;
					matrix.m03 = (point1.x + point2.x) * .5f;
					matrix.m13 = (point1.y + point2.y) * .5f;
					matrix.m23 = (point1.z + point2.z) * .5f;
					bar.matrix = matrix;
				}

				if (Mathf.Abs(extraDist) > breakResistance) {
					if (point2.neighborCount>1) {
						point2.neighborCount--;
						Point newPoint = new Point();
						newPoint.CopyFrom(point2);
						newPoint.neighborCount = 1;
						points[pointCount] = newPoint;
						bar.point2 = newPoint;
						pointCount++;
					} else if (point1.neighborCount>1) {
						point1.neighborCount--;
						Point newPoint = new Point();
						newPoint.CopyFrom(point1);
						newPoint.neighborCount = 1;
						points[pointCount] = newPoint;
						bar.point1 = newPoint;
						pointCount++;
					}
				}

				//these values dont get referenced ever...
				//nothing to see here, move along
				bar.minX = Mathf.Min(point1.x,point2.x);
				bar.maxX = Mathf.Max(point1.x,point2.x);
				bar.minY = Mathf.Min(point1.y,point2.y);
				bar.maxY = Mathf.Max(point1.y,point2.y);
				bar.minZ = Mathf.Min(point1.z,point2.z);
				bar.maxZ = Mathf.Max(point1.z,point2.z);

				matrices[i / instancesPerBatch][i % instancesPerBatch] = bar.matrix;
			}
		}
	}

	private void Update() {
		tornadoX = Mathf.Cos(Time.time/6f) * 30f;
		tornadoZ = Mathf.Sin(Time.time/6f * 1.618f) * 30f;
		cam.position = new Vector3(tornadoX,10f,tornadoZ) - cam.forward * 60f;

		if (matrices != null) {
			for (int i = 0; i < matrices.Length; i++) {
				Graphics.DrawMeshInstanced(barMesh,0,barMaterial,matrices[i],matrices[i].Length,matProps[i]);
			}
		}
	}
}

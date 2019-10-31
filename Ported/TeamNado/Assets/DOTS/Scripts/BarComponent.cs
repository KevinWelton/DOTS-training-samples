using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct BarComponent : IComponentData
{
    public float3 point1;
    public float3 point2;
    internal float3 velocity;
    internal float oldX;
    internal float oldY;
    internal float oldZ;

    public Matrix4x4 transform;
    // Add fields to your component here. Remember that:
    //
    // * A component itself is for storing data and doesn't 'do' anything.
    //
    // * To act on the data, you will need a System.
    //
    // * Data in a component must be blittable, which means a component can
    //   only contain fields which are primitive types or other blittable
    //   structs; they cannot contain references to classes.
    //
    // * You should focus on the data structure that makes the most sense
    //   for runtime use here. Authoring Components will be used for 
    //   authoring the data in the Editor.


}

[Serializable]
public struct UnsuckedBarComponent : IComponentData
{
    
}

[Serializable]
 public struct SuckedBarComponent : IComponentData
 {
     
 }
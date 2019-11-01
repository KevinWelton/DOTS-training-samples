using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct TornadoComponent : IComponentData
{
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
    public float3 tornadoPos;
}

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class TornadoAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public float3 tornadoPos;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new TornadoComponent { tornadoPos = tornadoPos };
        dstManager.AddComponentData(entity, data);
    }
}

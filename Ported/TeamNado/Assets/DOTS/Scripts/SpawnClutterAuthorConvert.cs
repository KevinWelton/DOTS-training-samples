using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[RequiresEntityConversion]
public class SpawnClutterAuthorConvert : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject PrefabA;
    public GameObject PrefabB;
    public GameObject PrefabC;
    public GameObject PrefabD;
    public GameObject PrefabE;

    public int Count;
    internal System.Random random = new System.Random();

    // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(PrefabA);
        referencedPrefabs.Add(PrefabB);
        referencedPrefabs.Add(PrefabC);
        referencedPrefabs.Add(PrefabD);
        referencedPrefabs.Add(PrefabE);
    }

    // Lets you convert the editor data representation to the entity optimal runtime representation
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new SpawnClutterComponent()
        {
            // The referenced prefab will be converted due to DeclareReferencedPrefabs.
            // So here we simply map the game object to an entity reference to that prefab.
            PrefabA = conversionSystem.GetPrimaryEntity(PrefabA),
            PrefabB = conversionSystem.GetPrimaryEntity(PrefabB),
            PrefabC = conversionSystem.GetPrimaryEntity(PrefabC),
            PrefabD = conversionSystem.GetPrimaryEntity(PrefabD),
            PrefabE = conversionSystem.GetPrimaryEntity(PrefabE),

            Count = Count,
            Height = TornadoConstants.TornadoHeight,
            Radius = TornadoConstants.TornadoMaxForceDistance,
        };

        dstManager.AddComponentData(entity, spawnerData);
    }
}

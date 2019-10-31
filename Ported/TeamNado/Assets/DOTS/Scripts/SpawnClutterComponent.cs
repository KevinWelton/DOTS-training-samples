using Unity.Collections;
using Unity.Entities;

// ReSharper disable once InconsistentNaming
public struct SpawnClutterComponent : IComponentData
{
    public int Count;
    public Entity PrefabA;
    public Entity PrefabB;
    public Entity PrefabC;
    public Entity PrefabD;
    public Entity PrefabE;

    public float Height;
    public float Radius;
}
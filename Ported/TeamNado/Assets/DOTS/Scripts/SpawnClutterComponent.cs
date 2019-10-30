using Unity.Entities;

// ReSharper disable once InconsistentNaming
public struct SpawnClutterComponent : IComponentData
{
    public int Count;
    public Entity Prefab;
    public float Height;
    public float Radius;
}
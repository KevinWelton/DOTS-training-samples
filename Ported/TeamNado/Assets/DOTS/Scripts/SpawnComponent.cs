using Unity.Entities;

// ReSharper disable once InconsistentNaming
public struct SpawnComponent : IComponentData
{
    public int Count;
    public Entity Prefab;
}
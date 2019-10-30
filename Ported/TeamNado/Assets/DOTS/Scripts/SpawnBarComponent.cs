using Unity.Entities;

// ReSharper disable once InconsistentNaming
public struct SpawnBarComponent : IComponentData
{
    public int Count;
    public Entity Prefab;
}
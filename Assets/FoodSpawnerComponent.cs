using Unity.Entities;
using Unity.Mathematics;

public struct FoodSpawnerComponent : IComponentData
{
    public Random Random;
    public float SpawnRadius { get; set; }
    public Entity FoodPrefab { get; set; }
    public int CellSize { get; set; }
    public float EatingDistance { get; set; }

    public bool Spawn;
    public int Amount;
}

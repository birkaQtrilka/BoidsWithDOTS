using Unity.Entities;

public struct BoidSpawnerComponent : IComponentData
{
    public float ElapsedTime { get; set; }
    public float Interval {get;set;}
    public Entity BoidPrefab {get;set;}
    public int BoidsPerInterval {get;set;}

    public int TotalSpawnedBoids;
    public int BoidsToSpawn;
    public float PersceptionDistance;
    public int CellSize;
    public float ArenaRadius;
    public float Speed;

    public float AlignmentBias;
    public float SeparationBias;
    public float CohesionBias;
    public float WallRepellent;
    public float FoodAttraction;
    public float EatingDistance;

    public float Step { get;set; }

}

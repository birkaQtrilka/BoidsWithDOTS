using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct BoidSpawnerComponent : IComponentData
{
    public float ElapsedTime { get; set; }
    public float Interval {get;set;}
    public int BoidsPerInterval {get;set;}
    public int TotalSpawnedBoids ;//{get;set;}
    public int BoidsToSpawn;// {get;set;}

    public float PersceptionDistance {get;set;}
    public Entity BoidPrefab {get;set;}
    public float MaxSpeed {get;set;}
    public float MaxForce {get;set;}

    public int CellSize { get;set;}
    public float ArenaRadius { get;set;}

    public float AlignmentBias { get; set;}
    public float SeparationBias { get;set;}
    public float CohesionBias { get;set;}
    public float WallRepellent { get;set;}
    public float FoodAttraction { get;set;}
    public float EatingDistance { get;set; }

    public float Step { get;set; }

}

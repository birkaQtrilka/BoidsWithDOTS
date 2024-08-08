using Unity.Entities;
using Unity.Mathematics;

public struct BoidComponent : IComponentData
{
    public float3 velocity ;//{ get; set; }
    public float3 acceleration;// {get; set;}
    public float3 currentPosition;// {get; set;}

    public Entity Prefab;

    public float PersceptionDistance {get; set;}
    public float Speed {get; set;}
    public float Step {get; set;}
    public float MaxForce { get; set;}

    public int CellSize {get; set;}
    public float ArenaRadius { get; set;}

    public float AlignmentBias {get; set; }
    public float SeparationBias {get; set;}
    public float CohesionBias {get; set;}
    public float WallRepellent { get;set;}
    public float FoodAttraction { get;set; }
    public float EatingDistance { get; set;}

    public bool IsFood { get; set;}
}

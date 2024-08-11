using Unity.Entities;
using Unity.Mathematics;

public struct BoidComponent : IComponentData
{
    public float3 velocity ;//{ get; set; }
    public float3 acceleration;// {get; set;}
    public float3 currentPosition;// {get; set;}

    public bool IsFood { get; set;}
}

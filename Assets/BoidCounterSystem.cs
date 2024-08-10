using System;
using Unity.Entities;

public partial class BoidCounterSystem : SystemBase
{
    public event Action<int> CountUpdated;
    EntityQuery boidQuery;
    int _prevEntityCount;

    protected override void OnCreate()
    {
        base.OnCreate();
        boidQuery = SystemAPI.QueryBuilder()
            .WithAll<BoidComponent>()
            .WithNone<FoodTag>()
            .Build();
    }

    protected override void OnUpdate()
    {
        int entityCount = boidQuery.CalculateEntityCount();
        if(entityCount != _prevEntityCount)
        {
            CountUpdated?.Invoke(entityCount);

        }
    }
}

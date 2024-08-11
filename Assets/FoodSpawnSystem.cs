using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct FoodSpawnSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<FoodSpawnerComponent>(out Entity spawnerEntity))
            return;
        RefRW<FoodSpawnerComponent> spawner = SystemAPI.GetComponentRW<FoodSpawnerComponent>(spawnerEntity);
        FoodSpawnerComponent spawnerValuesRO = spawner.ValueRO;

        if (!spawnerValuesRO.Spawn) return;
        spawner.ValueRW.Spawn = false;
        EntityCommandBuffer ecb = new(Allocator.Temp);
        
        for (int i = 0; i < spawner.ValueRO.Amount; i++)
        {
            Entity e = ecb.Instantiate(spawnerValuesRO.FoodPrefab);
            //maybe needs rw values
            float randomX = (float)((spawner.ValueRW.Random.NextDouble() * 2) - 1) * spawner.ValueRW.SpawnRadius;
            float randomY = (float)((spawner.ValueRW.Random.NextDouble() * 2) - 1) * spawner.ValueRW.SpawnRadius;
            float randomZ = (float)((spawner.ValueRW.Random.NextDouble() * 2) - 1) * spawner.ValueRW.SpawnRadius;

            ecb.AddComponent(e, new BoidComponent
            {
                IsFood = true,
            });
            ecb.SetComponent(e, new LocalTransform
            {
                Position = new float3(randomX, randomY, randomZ),
                Rotation = quaternion.identity,
                Scale = 1f
            });
            ecb.AddComponent(e, new FoodTag());
        }
        ecb.Playback(state.EntityManager);
    }
}

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
        if (!SystemAPI.TryGetSingletonEntity<FoodSingleton>(out Entity signletonEntity))
            return;
        RefRW<FoodSingleton> data = SystemAPI.GetComponentRW<FoodSingleton>(signletonEntity);

        if (!data.ValueRO.Spawn) return;
        data.ValueRW.Spawn = false;

        RefRW<FoodSpawnerComponent> spawner = SystemAPI.GetComponentRW<FoodSpawnerComponent>(spawnerEntity);
        EntityCommandBuffer ecb = new(Allocator.Temp);
        FoodSpawnerComponent spawnerValuesRO = spawner.ValueRO;
        

        for (int i = 0; i < data.ValueRO.Amount; i++)
        {
            Entity e = ecb.Instantiate(spawnerValuesRO.FoodPrefab);
            //maybe needs rw values
            float randomX = (float)((spawner.ValueRW.Random.NextDouble() * 2) - 1) * spawner.ValueRW.SpawnRadius;
            float randomY = (float)((spawner.ValueRW.Random.NextDouble() * 2) - 1) * spawner.ValueRW.SpawnRadius;
            float randomZ = (float)((spawner.ValueRW.Random.NextDouble() * 2) - 1) * spawner.ValueRW.SpawnRadius;

            ecb.AddComponent(e, new BoidComponent
            {
                IsFood = true,
                CellSize = spawnerValuesRO.CellSize,//cellSize is needed for the hash function to work
                EatingDistance = spawnerValuesRO.EatingDistance,
                Prefab = spawnerValuesRO.FoodPrefab
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

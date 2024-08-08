using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class FoodSpawnSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        if (!SystemAPI.TryGetSingletonEntity<FoodSpawnerComponent>(out Entity spawnerEntity))
            return;

        RefRO<FoodSpawnerComponent> spawner = SystemAPI.GetComponentRO<FoodSpawnerComponent>(spawnerEntity);
        EntityCommandBuffer ecb = new(Allocator.Temp);
        FoodSpawnerComponent spawnerValuesRO = spawner.ValueRO;
        Debug.Log("SpawningFood");
        
        for (int i = 0; i < spawnerValuesRO.Amount; i++)
        {
            Entity e = ecb.Instantiate(spawnerValuesRO.FoodPrefab);
            float randomX = UnityEngine.Random.Range(-spawner.ValueRO.SpawnRadius, spawner.ValueRO.SpawnRadius);
            float randomY = UnityEngine.Random.Range(-spawner.ValueRO.SpawnRadius, spawner.ValueRO.SpawnRadius);

            ecb.AddComponent(e, new BoidComponent
            {
                IsFood = true,
                CellSize = spawnerValuesRO.CellSize,
                EatingDistance = spawnerValuesRO.EatingDistance,
                Prefab = spawnerValuesRO.FoodPrefab
            });
            ecb.SetComponent(e, new LocalTransform
            {
                Position = new float3(randomX, randomY, 0),
                Rotation = quaternion.identity,
                Scale = 1f
            });
        }
        ecb.Playback(EntityManager);
    }
}

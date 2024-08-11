using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

public partial struct BoidSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<BoidSpawnerComponent>(out Entity spawnerEntity))
            return;
        RefRW<BoidSpawnerComponent> spawner = SystemAPI.GetComponentRW<BoidSpawnerComponent>(spawnerEntity);

        spawner.ValueRW.ElapsedTime += Time.deltaTime;

        if (spawner.ValueRO.ElapsedTime < spawner.ValueRO.Interval) return;

        BoidSpawnerComponent spawnerValuesRO = spawner.ValueRO;
        //TODO: some math if adding boids per interval won't add up to BoidsToSpawn
        spawner.ValueRW.TotalSpawnedBoids += spawnerValuesRO.BoidsPerInterval;
        if (spawnerValuesRO.TotalSpawnedBoids >= spawnerValuesRO.BoidsToSpawn)
            return;

        spawner.ValueRW.ElapsedTime = 0;
        var commandBufferSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var entityCommandBuffer = commandBufferSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var jobHandle = new SpawnBoidsJob
        {
            ecb = entityCommandBuffer,
            spawnerValuesRO = spawnerValuesRO,
            random = Unity.Mathematics.Random.CreateFromIndex((uint)(SystemAPI.Time.ElapsedTime / SystemAPI.Time.DeltaTime)),
        }.Schedule(spawnerValuesRO.BoidsPerInterval, state.Dependency);
        state.Dependency = jobHandle;
        
    }

    [BurstCompile]
    public partial struct SpawnBoidsJob : IJobFor
    {
        public EntityCommandBuffer ecb;
        public Unity.Mathematics.Random random;

        [ReadOnly] public BoidSpawnerComponent spawnerValuesRO;
        
        public void Execute(int index)
        {
            
            Entity e = ecb.Instantiate(spawnerValuesRO.BoidPrefab);
           
            float randomX = (float)((random.NextDouble() * 2) - 1) * spawnerValuesRO.ArenaRadius;
            float randomY = (float)((random.NextDouble() * 2) - 1) * spawnerValuesRO.ArenaRadius;
            float randomZ = (float)((random.NextDouble() * 2) - 1) * spawnerValuesRO.ArenaRadius;

            ecb.SetComponent(e, new LocalTransform
            {
                Position = new float3(randomX, randomY, randomZ),
                Rotation = quaternion.identity,
                Scale = 1f
            });

            ecb.AddComponent(e, new BoidComponent
            {
                velocity = math.normalize(new float3(randomX, randomY, randomZ)) * spawnerValuesRO.Speed,
            });

        }
    }
}

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

//[UpdateAfter((typeof(BoidSpawnerSystem)))]
public partial struct BoidSystem : ISystem
{
    NativeParallelMultiHashMap<int, BoidComponent> _cellVsEntityPositions;
    EntityQuery _boidQuery;
    Entity _spawnerEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _cellVsEntityPositions = new NativeParallelMultiHashMap<int, BoidComponent>(0, Allocator.Persistent);
        _boidQuery = SystemAPI.QueryBuilder()
                    .WithAll<BoidComponent>()
                    .WithAll<LocalToWorld>()
                    .Build();
        state.RequireForUpdate<BoidComponent>();
        //state.RequireForUpdate<BoidSpawnerComponent>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        _cellVsEntityPositions.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        SystemAPI.TryGetSingletonEntity<BoidSpawnerComponent>(out _spawnerEntity);//probably don't cache it here but just in the update loop?

        RefRO<BoidSpawnerComponent> spawner = SystemAPI.GetComponentRO<BoidSpawnerComponent>(_spawnerEntity);

        _cellVsEntityPositions.Clear();
        int entityCount = _boidQuery.CalculateEntityCount();
        if (entityCount > _cellVsEntityPositions.Capacity)
            _cellVsEntityPositions.Capacity = entityCount;

        //DebugDrawWalls();
        
        var cellVsEntityPositionsParallel = _cellVsEntityPositions.AsParallelWriter();
        var commandBufferSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var entityCommandBuffer = commandBufferSystem.CreateCommandBuffer(state.WorldUnmanaged);


        var populateCellJobHandle = new PopulateCellJob
        {
            cellVsEntityPositionsParallel = cellVsEntityPositionsParallel,
            sharedData = spawner.ValueRO,
        }.ScheduleParallel(state.Dependency);

        var boidMovementJobHandle = new BoidMovementJob
        {
            cellVsEntityPositions = _cellVsEntityPositions,
            deltaTime = SystemAPI.Time.DeltaTime,
            ecb = entityCommandBuffer.AsParallelWriter(),
            sharedData = spawner.ValueRO,
        }.ScheduleParallel(populateCellJobHandle);
        state.Dependency = boidMovementJobHandle;
    }
}

 [BurstCompile]
public partial struct PopulateCellJob : IJobEntity
{
    public NativeParallelMultiHashMap<int, BoidComponent>.ParallelWriter cellVsEntityPositionsParallel;
    public BoidSpawnerComponent sharedData;
         
    public void Execute(ref BoidComponent bc, in LocalTransform trans)
    {
        bc.currentPosition = trans.Position;
        cellVsEntityPositionsParallel.Add(GetUniqueKeyForPosition(trans.Position, sharedData.CellSize), bc);
    }
    readonly int GetUniqueKeyForPosition(float3 position, int cellSize)
    {
        return (int)((15 * math.floor(position.x / cellSize)) + (17 * math.floor(position.y / cellSize)) + (19 * math.floor(position.z / cellSize)));
    }
}

[BurstCompile]
public partial struct BoidMovementJob : IJobEntity
{
    [ReadOnly] public NativeParallelMultiHashMap<int, BoidComponent> cellVsEntityPositions;
    [ReadOnly] public float deltaTime;
    [ReadOnly] public BoidSpawnerComponent sharedData;

    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(Entity e, [ChunkIndexInQuery] int sortKey, ref BoidComponent boid, ref LocalTransform trans)
    {
        int    key = GetUniqueKeyForPosition(trans.Position, sharedData.CellSize);
        int    total = 0;
        float3 separation = float3.zero;
        float3 alignment = float3.zero;
        float3 cohesion = float3.zero;
        float3 foodMagnetism = float3.zero;
        float3 closestFood = float3.zero;
        float  closestFoodDistance = float.MaxValue;
        bool   canLoop = true;

        if (!cellVsEntityPositions.TryGetFirstValue(key, out BoidComponent neighbour, out NativeParallelMultiHashMapIterator<int> nmhKeyIterator)) return;

        do
        {
            Eat(sortKey, in neighbour, ref boid, ref ecb, ref e, ref canLoop);
            if (boid.IsFood) continue;

            float distance = math.distance(trans.Position, neighbour.currentPosition);
            bool neighbourIsCurrBoid = trans.Position.Equals(neighbour.currentPosition);

            if (neighbourIsCurrBoid || distance > sharedData.PersceptionDistance) continue;
            if (neighbour.IsFood && distance < closestFoodDistance)
            {
                closestFoodDistance = distance;
                foodMagnetism = neighbour.currentPosition - trans.Position;
                continue;
            }

            float3 distanceFromTo = trans.Position - neighbour.currentPosition;
            separation += (distanceFromTo / distance);
            cohesion += neighbour.currentPosition;
            alignment += neighbour.velocity;
            total++;
        }
        while (cellVsEntityPositions.TryGetNextValue(out neighbour, ref nmhKeyIterator));

        if (boid.IsFood) return;

        if (total > 0)
        {
            cohesion /= total;
            cohesion -= (trans.Position + boid.velocity);
            cohesion = math.normalize(cohesion) * sharedData.CohesionBias;

            separation /= total;
            separation -= boid.velocity;
            separation = math.normalize(separation) * sharedData.SeparationBias;

            alignment /= total;
            alignment -= boid.velocity;
            alignment = math.normalize(alignment) * sharedData.AlignmentBias;

        }

        if (!closestFood.Equals(float3.zero))
            foodMagnetism = math.normalize(foodMagnetism) * sharedData.FoodAttraction;

        float3 wallRepelant = GetWallRepellentValue(trans.Position, sharedData.ArenaRadius, sharedData.WallRepellent);

        boid.acceleration += (cohesion + alignment + separation + wallRepelant + foodMagnetism);
        boid.velocity += boid.acceleration;
        boid.velocity = math.normalize(boid.velocity) * sharedData.Speed;
        boid.acceleration = float3.zero;
        trans.Position = math.lerp(trans.Position, (trans.Position + boid.velocity), deltaTime * sharedData.Step);
        trans.Rotation = math.slerp(trans.Rotation, quaternion.LookRotation(math.normalize(boid.velocity), math.up()), deltaTime * 10);
    }

    [BurstCompile]
    readonly float3 GetWallRepellentValue(float3 currentPos, float wall, float repellent)
    {
        float3 wallRepelant = float3.zero;

        if (currentPos.x < -wall)
            wallRepelant += new float3(repellent, 0, 0);
        else if (currentPos.x > wall)
            wallRepelant += new float3(-repellent, 0, 0);
        if (currentPos.y < -wall)
            wallRepelant += new float3(0, repellent, 0);
        else if (currentPos.y > wall)
            wallRepelant += new float3(0, -repellent, 0);
        if (currentPos.z < -wall)
            wallRepelant += new float3(0, 0, repellent);
        else if (currentPos.z > wall)
            wallRepelant += new float3(0, 0, -repellent);

        return wallRepelant;
    }

    [BurstCompile]
    readonly int GetUniqueKeyForPosition(float3 position, int cellSize)
    {
        return (int)((15 * math.floor(position.x / cellSize)) + (17 * math.floor(position.y / cellSize)) + (19 * math.floor(position.z / cellSize)));
    }
    
    [BurstCompile]
    void Eat(int sortKey, in BoidComponent neighbour, ref BoidComponent boid, ref EntityCommandBuffer.ParallelWriter ecb, ref Entity e, ref bool canLoop)
    {
        if (!canLoop || !boid.IsFood || neighbour.IsFood) return;

        float distance = math.distance(boid.currentPosition, neighbour.currentPosition);

        if (distance > sharedData.EatingDistance) return;

        ecb.DestroyEntity(sortKey, e);

        var newEntity = ecb.Instantiate(sortKey, sharedData.BoidPrefab);
        ecb.SetComponent(sortKey, newEntity, new LocalTransform
        {
            Position = boid.currentPosition,
            Scale = 1f
        });

        ecb.AddComponent(sortKey, newEntity, new BoidComponent
        {
            velocity = -boid.velocity,
        });
        canLoop = false;

    }
}
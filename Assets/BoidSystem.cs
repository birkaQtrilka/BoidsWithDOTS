using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public partial struct BoidSystem : ISystem
{
    private NativeParallelMultiHashMap<int, BoidComponent> cellVsEntityPositions;
    private EntityQuery boidQuery;

    public static int GetUniqueKeyForPosition(float3 position, int cellSize)
    {
        //var hash = (int)math.hash(new int3(math.floor(math.mul(cellRotationVary, localToWorld.Position + positionOffsetVary) / cellRadius)));
        return (int)( (15 * math.floor(position.x / cellSize)) + (17 * math.floor(position.y / cellSize)) + (19 * math.floor(position.z / cellSize)));
    }

    public void OnCreate(ref SystemState state)
    {
        cellVsEntityPositions = new NativeParallelMultiHashMap<int, BoidComponent>(0, Allocator.Persistent);
        boidQuery = SystemAPI.QueryBuilder()
                    .WithAll<BoidComponent>()
                    .WithAll<LocalToWorld>()
                    .Build();
        state.RequireForUpdate<BoidComponent>();
    }

    public void OnDestroy(ref SystemState state)
    {
        cellVsEntityPositions.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        cellVsEntityPositions.Clear();
        int entityCount = boidQuery.CalculateEntityCount();
        if (entityCount > cellVsEntityPositions.Capacity)
            cellVsEntityPositions.Capacity = entityCount;

        //DebugDrawWalls();
        
        var cellVsEntityPositionsParallel = cellVsEntityPositions.AsParallelWriter();
        var commandBufferSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var entityCommandBuffer = commandBufferSystem.CreateCommandBuffer(state.WorldUnmanaged);
        var parallelEcb = entityCommandBuffer.AsParallelWriter();

        //instantiate cloned entities

        // Schedule the PopulateCellJob
        var populateCellJobHandle = new PopulateCellJob
        {
            cellVsEntityPositionsParallel = cellVsEntityPositionsParallel,
        }.ScheduleParallel(state.Dependency);

        var boidMovementJobHandle = new BoidMovementJob
        {
            cellVsEntityPositions = cellVsEntityPositions,
            deltaTime = SystemAPI.Time.DeltaTime,
            ecb = parallelEcb,
        }.ScheduleParallel(populateCellJobHandle);

        //var boidEatingJobHandle = new BoidEatingJob
        //{
        //    cellVsEntityPositions = cellVsEntityPositions,
        //    ecb = parallelEcb,
        //}.ScheduleParallel(boidMovementJobHandle);
        //destroy all boids

        state.Dependency = boidMovementJobHandle;
    }

    [BurstCompile]
    public partial struct PopulateCellJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, BoidComponent>.ParallelWriter cellVsEntityPositionsParallel;

        public void Execute(ref BoidComponent bc, in LocalTransform trans)
        {
            bc.currentPosition = trans.Position;
            cellVsEntityPositionsParallel.Add(GetUniqueKeyForPosition(trans.Position, bc.CellSize), bc);
        }
    }

    //[BurstCompile]
    //public partial struct BoidEatingJob : IJobEntity
    //{
    //    [ReadOnly] public NativeParallelMultiHashMap<int, BoidComponent> cellVsEntityPositions;

    //    public void Execute(Entity e, [ChunkIndexInQuery] int sortKey, ref BoidComponent boid, ref LocalTransform trans)
    //    {
    //        //boid.currentPosition = trans.Position;

    //        int key = GetUniqueKeyForPosition(trans.Position, boid.CellSize);
    //        if (!cellVsEntityPositions.TryGetFirstValue(key, out BoidComponent neighbour, out NativeParallelMultiHashMapIterator<int> nmhKeyIterator)) return;
    //        //get the first 
    //        do
    //        {
               
    //        }
    //        while (cellVsEntityPositions.TryGetNextValue(out neighbour, ref nmhKeyIterator));
    //    }

    //}

    [BurstCompile]
    public partial struct BoidMovementJob : IJobEntity
    {
        [ReadOnly] public NativeParallelMultiHashMap<int, BoidComponent> cellVsEntityPositions;
        public float deltaTime;
        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(Entity e, [ChunkIndexInQuery] int sortKey, ref BoidComponent boid, ref LocalTransform trans)
        {

            int key = GetUniqueKeyForPosition(trans.Position, boid.CellSize);
            int total = 0;
            float3 separation = float3.zero;
            float3 alignment = float3.zero;
            float3 cohesion = float3.zero;
            float3 foodMagnetism = float3.zero;
            float3 closestFood = float3.zero;
            float closestFoodDistance = float.MaxValue;

            //DebugDrawBoid(trans.Position);

            if (!cellVsEntityPositions.TryGetFirstValue(key, out BoidComponent neighbour, out NativeParallelMultiHashMapIterator<int> nmhKeyIterator)) return;

            // Debug.DrawLine(neighbour.currentPosition, trans.Position, Color.red);
            bool canLoop = true;
            do
            {
                Eat(sortKey,in neighbour, ref boid, ref ecb,ref e,ref canLoop);
                if (boid.IsFood) continue;

                float distance = math.distance(trans.Position, neighbour.currentPosition);
                if (trans.Position.Equals(neighbour.currentPosition) || //neighbour is this boid
                    distance > boid.PersceptionDistance) continue;
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
                cohesion = math.normalize(cohesion) * boid.CohesionBias;

                separation /= total;
                separation -= boid.velocity;
                separation = math.normalize(separation) * boid.SeparationBias;

                alignment /= total;
                alignment -= boid.velocity;
                alignment = math.normalize(alignment) * boid.AlignmentBias;

            }

            if(!closestFood.Equals(float3.zero))
                foodMagnetism = math.normalize(foodMagnetism) * boid.FoodAttraction;

            float3 wallRepelant = GetWallRepellentValue(trans.Position, boid.ArenaRadius, boid.WallRepellent);

            boid.acceleration += (cohesion + alignment + separation + wallRepelant + foodMagnetism);
            boid.velocity += boid.acceleration;
            boid.velocity = math.normalize(boid.velocity) * boid.Speed;
            boid.acceleration = float3.zero;
            trans.Position = math.lerp(trans.Position, (trans.Position + boid.velocity), deltaTime * boid.Step);
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

        void Eat(int sortKey, in BoidComponent neighbour, ref BoidComponent boid, ref EntityCommandBuffer.ParallelWriter ecb, ref Entity e, ref bool canLoop)
        {
            if (!canLoop) return;

            bool foodAndBoidInteraction = neighbour.IsFood ^ boid.IsFood;
            if (!foodAndBoidInteraction) return;

            float distance = math.distance(boid.currentPosition, neighbour.currentPosition);
            //bool isCurrentBoid = boid.Equals(neighbour);
            if (distance > boid.EatingDistance)
                return;

            if (boid.IsFood)
            {
                //ecb.AddComponent(e, new DestroyTag());
                ecb.DestroyEntity(sortKey, e);
                canLoop = false;
                return;
            }

            var newEntity = ecb.Instantiate(sortKey, boid.Prefab);
            ecb.SetComponent(sortKey, newEntity, new LocalTransform
            {
                Position = boid.currentPosition,
                Scale = 1f
            });
            ecb.AddComponent(sortKey, newEntity, new BoidComponent
            {
                velocity = -boid.velocity,
                PersceptionDistance = boid.PersceptionDistance,
                Speed = boid.Speed,
                CellSize = boid.CellSize,
                AlignmentBias = boid.AlignmentBias,
                SeparationBias = boid.SeparationBias,
                CohesionBias = boid.CohesionBias,
                Step = boid.Step,
                MaxForce = boid.MaxForce,
                FoodAttraction = boid.FoodAttraction,
                ArenaRadius = boid.ArenaRadius,
                WallRepellent = boid.WallRepellent,
                EatingDistance = boid.EatingDistance,
                Prefab = boid.Prefab,
            });

            canLoop = false;
        }

        //readonly void DebugDrawBoid(float3 position)
        //{
        //    float3 Up = new (0, 1, 0);
        //    float3 Right = new (1, 0, 0);
        //    float size = .2f;

        //    Debug.DrawLine(position + (Up - Right) * size, position + (Up + Right) * size);
        //    Debug.DrawLine(position + (-Up - Right) * size,position + (-Up + Right) * size);
        //    Debug.DrawLine(position + (Up - Right) * size, position + (-Up - Right) * size);
        //    Debug.DrawLine(position + (Up + Right) * size, position + (-Up + Right) * size);
        //}

    }
    //readonly void DebugDrawWalls()
    //{
    //    float3 Up = new(0, 1, 0);
    //    float3 Right = new(1, 0, 0);
    //    float wall = 10;

    //    Debug.DrawLine((Up - Right) * wall, (Up + Right) * wall);
    //    Debug.DrawLine((-Up - Right) * wall, (-Up + Right) * wall);
    //    Debug.DrawLine((Up - Right) * wall, (-Up - Right) * wall);
    //    Debug.DrawLine((Up + Right) * wall, (-Up + Right) * wall);
    //}
}
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class FoodSpawnerAuthoring : MonoBehaviour
{
    [field: SerializeField] public BoidSpawnerAuthoring BoidSpawner { get; set; }

    [field: SerializeField] public GameObject FoodPrefab { get; set; }
    [field: SerializeField] public int Amount { get; set; }

    public class FoodSpawnerBaker : Baker<FoodSpawnerAuthoring>
    {
        public override void Bake(FoodSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new FoodSpawnerComponent
            {
                Amount = authoring.Amount,
                SpawnRadius = authoring.BoidSpawner.ArenaRadius,
                FoodPrefab = GetEntity(authoring.FoodPrefab, TransformUsageFlags.Dynamic),
                CellSize = authoring.BoidSpawner.CellSize,
                EatingDistance = authoring.BoidSpawner.EatingDistance
            });
        }
    }
}

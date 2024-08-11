using Unity.Entities;
using UnityEngine;

public class BoidSpawnerAuthoring : MonoBehaviour
{

    [field: SerializeField] public GameObject Prefab { get; private set; }

    [field: SerializeField] public float Interval {get; private set;}
    [field: SerializeField] public int BoidsPerInterval {get; private set;}
    [field: SerializeField] public int BoidsToSpawn {get; private set;}
                            
    [field: SerializeField] public float PersceptionDistance {get; private set;}
    [field: SerializeField] public float Speed {get; private set;}

    [field: SerializeField] public int CellSize { get; set; }
    [field: SerializeField] public float ArenaRadius { get; set; }

    [field: SerializeField] public float AlignmentBias { get; set; }
    [field: SerializeField] public float SeparationBias { get; set; }
    [field: SerializeField] public float CohesionBias { get; set; }
    [field: SerializeField] public float WallRepellent { get; set; }
    [field: SerializeField] public float FoodAttraction { get; set; }
    [field: SerializeField] public float EatingDistance { get; set; }
    [field: SerializeField] public float Step { get; set; }

    class BoidSpawnerBaker : Baker<BoidSpawnerAuthoring>
    {
        public override void Bake(BoidSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new BoidSpawnerComponent()
            {
                BoidPrefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Interval = authoring.Interval,
                Speed = authoring.Speed,
                PersceptionDistance = authoring.PersceptionDistance,
                BoidsPerInterval = authoring.BoidsPerInterval,
                BoidsToSpawn = authoring.BoidsToSpawn,
                ArenaRadius = authoring.ArenaRadius,
                CellSize = authoring.CellSize,
                AlignmentBias = authoring.AlignmentBias,
                SeparationBias = authoring.SeparationBias,
                CohesionBias = authoring.CohesionBias,
                Step = authoring.Step,
                WallRepellent = authoring.WallRepellent,
                FoodAttraction = authoring.FoodAttraction,
                EatingDistance = authoring.EatingDistance,
            });
        }
    }

}

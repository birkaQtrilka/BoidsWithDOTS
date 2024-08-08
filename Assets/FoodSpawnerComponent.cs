using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct FoodSpawnerComponent : IComponentData
{
    public float SpawnRadius { get; set; }
    public int Amount { get; set; }
    public Entity FoodPrefab { get; set; }
    public int CellSize { get; set; }
    public float EatingDistance { get; set; }
}

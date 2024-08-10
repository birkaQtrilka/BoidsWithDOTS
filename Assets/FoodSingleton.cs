using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct FoodSingleton : IComponentData
{
    public bool Spawn;
    public int Amount;
}

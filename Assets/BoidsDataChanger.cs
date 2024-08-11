using System;
using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class BoidsDataChanger : MonoBehaviour
{
    Entity _boidsSingleton;
    Entity _foodSingleton;

    EntityManager _entityManager;
    [Header("Boid behavior settings")]
    [SerializeField] Slider _sliderSpawnAmount;
    [SerializeField] Slider _sliderAlignment;
    [SerializeField] Slider _sliderSeparation;
    [SerializeField] Slider _sliderCohesion;
    [SerializeField] Slider _sliderSpeed;

    [Header("Space settings")]
    [SerializeField] Slider _sliderArenaSize;
    [SerializeField] Slider _sliderCellSize;

    EntityQuery _singletonsQuerry;

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _singletonsQuerry = _entityManager.CreateEntityQuery(typeof(BoidSpawnerComponent), typeof(FoodSpawnerComponent));
        StartCoroutine(GetData());
    }

    IEnumerator GetData()
    {
        yield return new WaitUntil(()=>
        _singletonsQuerry.TryGetSingletonEntity<BoidSpawnerComponent>(out _boidsSingleton) &&
        _singletonsQuerry.TryGetSingletonEntity<FoodSpawnerComponent>(out _foodSingleton)
        );
        BoidSpawnerComponent initialBoidState = _entityManager.GetComponentData<BoidSpawnerComponent>(_boidsSingleton);
        FoodSpawnerComponent initialFoodState = _entityManager.GetComponentData<FoodSpawnerComponent>(_foodSingleton);

        SliderUpdateFood(_sliderSpawnAmount, initialFoodState.Amount, (c, f) => { c.Amount = (int)f; return c; });

        SliderUpdateBoid(_sliderAlignment, initialBoidState.AlignmentBias, (c, f) => { c.AlignmentBias = f; return c; });
        SliderUpdateBoid(_sliderSeparation, initialBoidState.SeparationBias, (c, f) => { c.SeparationBias = f; return c; });
        SliderUpdateBoid(_sliderCohesion, initialBoidState.CohesionBias, (c, f) => { c.CohesionBias = f; return c; });
        SliderUpdateBoid(_sliderSpeed, initialBoidState.Speed, (c, f) => { c.Speed = f; return c; });

        SliderUpdateBoid(_sliderArenaSize, initialBoidState.ArenaRadius, (c, f) => { c.ArenaRadius = f; return c; });
        SliderUpdateBoid(_sliderCellSize, initialBoidState.CellSize, (c, f) => { c.CellSize = (int)f; return c; });
        _sliderCellSize.wholeNumbers = true;
    }

    void SliderUpdateBoid(Slider slider, float startValue, Func<BoidSpawnerComponent, float, BoidSpawnerComponent> change)
    {
        slider.value = startValue;

        slider.onValueChanged.AddListener(f => {
            var initialState = _entityManager.GetComponentData<BoidSpawnerComponent>(_boidsSingleton);
            _entityManager.SetComponentData(_boidsSingleton, change(initialState, slider.value));

        });

    }

    void SliderUpdateFood(Slider slider, float startValue, Func<FoodSpawnerComponent, float, FoodSpawnerComponent> change)
    {
        slider.value = startValue;

        slider.onValueChanged.AddListener(f => {
            var initialState = _entityManager.GetComponentData<FoodSpawnerComponent>(_foodSingleton);
            _entityManager.SetComponentData(_foodSingleton, change(initialState, slider.value));

        });

    }

    public void SpawnFood()
    {
        FoodSpawnerComponent initialState = _entityManager.GetComponentData<FoodSpawnerComponent>(_foodSingleton);
        initialState.Spawn = true;

        _entityManager.SetComponentData(_foodSingleton, initialState);
    }
}

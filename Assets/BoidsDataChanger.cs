using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class BoidsDataChanger : MonoBehaviour
{
    Entity _boidsSingleton;
    EntityManager _entityManager;
    [SerializeField] Slider _sliderAlignment;
    [SerializeField] Slider _sliderSeparation;
    [SerializeField] Slider _sliderCohesion;
    [SerializeField] Slider _sliderSpeed;

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _boidsSingleton = _entityManager.CreateEntityQuery(typeof(BoidSpawnerComponent)).GetSingletonEntity();
        var initialState = _entityManager.GetComponentData<BoidSpawnerComponent>(_boidsSingleton);

        SliderUpdate(_sliderAlignment, initialState.AlignmentBias, (c, f) => { c.AlignmentBias = f; return c; });
        SliderUpdate(_sliderSeparation, initialState.SeparationBias, (c, f) => { c.SeparationBias = f; return c; });
        SliderUpdate(_sliderCohesion, initialState.CohesionBias, (c, f) => { c.CohesionBias = f; return c; });
        SliderUpdate(_sliderSpeed, initialState.Speed, (c, f) => { c.Speed = f; return c; });
    }

    void SliderUpdate(Slider slider, float startValue, Func<BoidSpawnerComponent, float, BoidSpawnerComponent> change)
    {

        slider.value = startValue;

        slider.onValueChanged.AddListener(f => {
            var initialState = _entityManager.GetComponentData<BoidSpawnerComponent>(_boidsSingleton);
            _entityManager.SetComponentData(_boidsSingleton, change(initialState, slider.value));

        });

    }

}

using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class FoodSpawnerMono : MonoBehaviour
{
    Entity _singleton;
    World _world;
    int _spawnAmount;
    [SerializeField] Slider _spawnAmountSlider;
    [SerializeField] TextMeshProUGUI _sliderTextMesh;

    void Awake()
    {
        _world = World.DefaultGameObjectInjectionWorld;
        _spawnAmount = 1; 
        _spawnAmountSlider.onValueChanged.AddListener(v =>
        {
            _spawnAmount = (int)v;
            _sliderTextMesh.text = "Spawn Amount: " + _spawnAmount;
        });
    }

    Entity GetSingleton()
    {
        if(_world.IsCreated && !_world.EntityManager.Exists(_singleton))
        {
            _singleton = _world.EntityManager.CreateEntity();
            _world.EntityManager.AddComponentData(_singleton, new FoodSingleton { Spawn = false, Amount = _spawnAmount});
        }
        return _singleton;
    }

    public void SpawnFood()
    {
        Entity singleton = GetSingleton();
        _world.EntityManager.SetComponentData(singleton, new FoodSingleton { Spawn = true, Amount = _spawnAmount });
    }
}

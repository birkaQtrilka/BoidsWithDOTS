using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class FoodSpawnerMono : MonoBehaviour
{
    [SerializeField] Slider _spawnAmountSlider;
    [SerializeField] TextMeshProUGUI _sliderTextMesh;

    Entity _singleton;
    EntityManager _entityManager;
    int _spawnAmount;

    void Awake()
    {
        _spawnAmount = 1; 
        _spawnAmountSlider.onValueChanged.AddListener(v =>
        {
            _spawnAmount = (int)v;
            _sliderTextMesh.text = "Spawn Amount: " + _spawnAmount;
        });
    }

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _singleton = _entityManager.CreateEntityQuery(typeof(FoodSpawnerComponent)).GetSingletonEntity();
    }

    public void SpawnFood()
    {
        FoodSpawnerComponent initialState = _entityManager.GetComponentData<FoodSpawnerComponent>(_singleton);
        initialState.Spawn = true;
        initialState.Amount = _spawnAmount;

        _entityManager.SetComponentData(_singleton, initialState);
    }
}

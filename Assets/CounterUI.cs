using TMPro;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CounterUI : MonoBehaviour
{
    TextMeshProUGUI _textMesh;
    BoidCounterSystem _system;

    void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _system = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<BoidCounterSystem>();
        _system.CountUpdated += OnCountUpdated;   
    }

    void OnDestroy()
    {
        _system.CountUpdated -= OnCountUpdated;

    }
    
    void OnCountUpdated(int newCount)
    {
        _textMesh.text = "Boid Count: " + newCount.ToString();
    }
}

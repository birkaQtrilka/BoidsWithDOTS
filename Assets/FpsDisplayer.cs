using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsDisplayer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _textMesh;

    Dictionary<int, string> _cachedNumberStrings = new();
    int[] _frameRateSamples;
    int _cacheNumbersAmount = 300;
    int _averageFromAmount = 30;
    int _averageCounter = 0;
    int _currentAveraged;

    void Awake()
    {
        for (int i = 0; i < _cacheNumbersAmount; i++)
        {
            _cachedNumberStrings[i] = i.ToString();
        }
        _frameRateSamples = new int[_averageFromAmount];
    }
    void Update()
    {
        // Sample
        var currentFrame = (int)Mathf.Round(1f / Time.smoothDeltaTime); // If your game modifies Time.timeScale, use unscaledDeltaTime and smooth manually (or not).
        _frameRateSamples[_averageCounter] = currentFrame;

        // Average
        var average = 0f;

        foreach (var frameRate in _frameRateSamples)
        {
            average += frameRate;
        }

        _currentAveraged = (int)Mathf.Round(average / _averageFromAmount);
        _averageCounter = (_averageCounter + 1) % _averageFromAmount;

        // Assign to UI
        _textMesh.text = "FPS: " + _currentAveraged switch
        {
            var x when x >= 0 && x < _cacheNumbersAmount => _cachedNumberStrings[x],
            var x when x >= _cacheNumbersAmount => $"> {_cacheNumbersAmount}",
            var x when x < 0 => "< 0",
            _ => "?"
        };
    }
}

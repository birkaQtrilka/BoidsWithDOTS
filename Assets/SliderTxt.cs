using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderTxt : MonoBehaviour
{
    [SerializeField] string _startText;
    Slider _slider;
    TextMeshProUGUI _textMesh;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _slider.onValueChanged.AddListener(f => _textMesh.text = _startText + f.ToString());
        _textMesh.text = _startText;
    }
}

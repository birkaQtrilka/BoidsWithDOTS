using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] Canvas _ui;
    [SerializeField] GameObject _instuctions;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if(Input.GetKeyDown(KeyCode.T))
            _ui.gameObject.SetActive(!_ui.gameObject.activeInHierarchy);
        if (Input.GetKeyDown(KeyCode.Y))
            _instuctions.SetActive(!_instuctions.activeInHierarchy);
    }
}

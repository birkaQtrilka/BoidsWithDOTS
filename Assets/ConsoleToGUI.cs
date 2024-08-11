using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleToGUI : MonoBehaviour
{
    [SerializeField] bool _enable;
#if !UNITY_EDITOR
    static string myLog = "";
    private string output;
    private string stack;
    void OnEnable()
    {

        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog + "\n" + stackTrace;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }
    }

    void OnGUI()
    {
        //if (!Application.isEditor && _enable) //Do not display in editor ( or you can use the UNITY_EDITOR macro to also disable the rest)
        {
            myLog = GUI.TextArea(new Rect(Screen.height/2 + 10, 10, Screen.width - 10, Screen.height/3 - 10), myLog);
        }
    }
#endif
}

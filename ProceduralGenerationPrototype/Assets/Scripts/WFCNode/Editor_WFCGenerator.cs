using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WFCGenerator))]
public class Editor_WFCGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WFCGenerator script = (WFCGenerator)target;

        if(GUILayout.Button("Regenerate"))
        {
            ClearLog();
            script.Regenerate();
        }
    }

    public void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}

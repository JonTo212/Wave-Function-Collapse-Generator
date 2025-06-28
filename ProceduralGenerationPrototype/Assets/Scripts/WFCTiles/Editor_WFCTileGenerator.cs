using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WFCTileGenerator))]
public class Editor_WFCTileGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WFCTileGenerator script = (WFCTileGenerator)target;

        if (GUILayout.Button("Regenerate"))
        {
            ClearLog();
            script.Regenerate();
        }

        if (GUILayout.Button("Destroy"))
        {
            ClearLog();
            script.DestroyGrid();
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

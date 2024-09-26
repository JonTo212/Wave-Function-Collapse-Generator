using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveFunctionCollapse))]
public class Editor_WaveFunctionCollapse : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WaveFunctionCollapse script = (WaveFunctionCollapse)target;

        if(GUILayout.Button("Regenerate"))
        {
            script.Regenerate();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileGridManager))]
public class Editor_TileGridManager : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TileGridManager script = (TileGridManager)target;

        if(GUILayout.Button("Regenerate"))
        {
            script.Regenerate();
        }
    }
}

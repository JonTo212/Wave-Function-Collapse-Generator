using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WFCTileGenerator))]
public class Editor_WFCTileGenerator : Editor
{
    private TileType selectedTileType = TileType.Ground;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WFCTileGenerator script = (WFCTileGenerator)target;

        float totalWidth = EditorGUIUtility.currentViewWidth - 28f;
        float dropdownWidth = totalWidth * 0.5f;// - 10.5f;
        float buttonWidth = totalWidth * 0.25f;// + 5.25f;

        EditorGUILayout.LabelField("Tile Type Selection", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        selectedTileType = (TileType)EditorGUILayout.EnumPopup(selectedTileType, GUILayout.Width(dropdownWidth));

        if (GUILayout.Button("Add", GUILayout.Width(buttonWidth)))
        {
            script.AddTileType(selectedTileType);
        }

        if (GUILayout.Button("Remove", GUILayout.Width(buttonWidth)))
        {
            script.RemoveTileType(selectedTileType);
        }

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add All"))
        {
            script.AddAllTiles();
        }

        if (GUILayout.Button("Remove All"))
        {
            script.RemoveAllTiles();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("WFC Controls", EditorStyles.boldLabel);
        if (GUILayout.Button("WFC"))
        {
            ClearLog();
            script.RegenerateWFC();
        }

        if (GUILayout.Button("Build Paths"))
        {
            ClearLog();
            script.RegeneratePath();
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

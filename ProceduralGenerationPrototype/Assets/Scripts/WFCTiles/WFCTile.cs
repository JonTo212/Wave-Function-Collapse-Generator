using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FaceType
{
    Yellow,
    Blue
}

public enum FaceDirection
{
    Up,
    Down,
    Left,
    Right,
    Forward,
    Back
}

[System.Serializable]
public struct SerializableFace
{
    public FaceDirection direction;
    public FaceType faceType;
}


[CreateAssetMenu(fileName = "WFCTile", menuName = "WFC/Tile")]
[System.Serializable]
public class WFCTile : ScriptableObject
{
    public int weight;
    public GameObject prefab;

    public SerializableFace[] faces = new SerializableFace[6];
    public Dictionary<Vector3, FaceType> faceMap = new Dictionary<Vector3, FaceType>();
    public bool pathable;

    private void OnEnable()
    {
        InitializeFaces();
    }

    public void InitializeFaces()
    {
        faceMap.Clear();
        faceMap[Vector3.up] = faces[0].faceType;
        faceMap[Vector3.down] = faces[1].faceType;
        faceMap[Vector3.left] = faces[2].faceType;
        faceMap[Vector3.right] = faces[3].faceType;
        faceMap[Vector3.forward] = faces[4].faceType;
        faceMap[Vector3.back] = faces[5].faceType;
    }

    public bool CanConnect(WFCTile neighbour, Vector3 direction)
    {
        if (faceMap == null || faceMap.Count == 0)
        {
            InitializeFaces();
        }

        if (!faceMap.ContainsKey(direction) || !neighbour.faceMap.ContainsKey(-direction))
        {
            return false;
        }

        return faceMap[direction] == neighbour.faceMap[-direction];
    }

    public List<Vector3Int> GetConnectors()
    {
        if (faceMap == null || faceMap.Count == 0)
        {
            InitializeFaces();
        }

        if (!pathable) return new List<Vector3Int>();

        return faceMap.Where(pair => pair.Value == FaceType.Blue)
                      .Select(pair => Vector3Int.FloorToInt(pair.Key))
                      .ToList();
    }

    public bool IsPathCompatible(WFCTile neighbour, Vector3Int dir)
    {
        var aFace = faceMap[dir];
        var bFace = neighbour.faceMap[-dir];

        return pathable && neighbour.pathable && aFace == FaceType.Blue && bFace == FaceType.Blue && aFace == bFace;
    }

    public bool HasConnector(Vector3Int dir)
    {
        if (!faceMap.TryGetValue(dir, out var face))
        {
            //Debug.LogWarning($"{name}: No face found in direction {dir}");
            return false;
        }
        else if(face != FaceType.Blue)
        {
            //Debug.LogWarning($"{name}: FaceType incorrect." + " Facetype = " + face);
            return false;
        }

        //return whether current tile is pathable & whether the given direction has a defined face that is blue
        return pathable;
    }
}
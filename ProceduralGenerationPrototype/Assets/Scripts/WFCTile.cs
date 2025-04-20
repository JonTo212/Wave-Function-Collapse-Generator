using System.Collections;
using System.Collections.Generic;
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
        if (!faceMap.ContainsKey(direction) || !neighbour.faceMap.ContainsKey(-direction))
        {
            return false;
        }

        return faceMap[direction] == neighbour.faceMap[-direction];
    }
}
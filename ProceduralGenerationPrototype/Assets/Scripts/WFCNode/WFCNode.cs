using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WFCNode", menuName = "WFC/Node")]
[System.Serializable]
public class WFCNode : ScriptableObject
{
    public string prefabName;
    public GameObject prefab;
    public List<NodeData> nodeDataList = new List<NodeData>();
    public Dictionary<Vector3, List<string>> validNodeDictionary = new Dictionary<Vector3, List<string>>();
    public Dictionary<Vector3, List<WFCNode>> validNodes = new Dictionary<Vector3, List<WFCNode>>();

    private void OnEnable()
    {
        PopulateValidNodeDictionary();
    }

    public void PopulateValidNodeDictionary()
    {
        if (validNodeDictionary != null)
        {
            validNodeDictionary.Clear(); //make sure it's freshly populated
        }

        //Tie valid node string lists to direction
        validNodeDictionary[Vector3.up] = nodeDataList[0].validNodes;
        validNodeDictionary[Vector3.down] = nodeDataList[1].validNodes;     
        validNodeDictionary[Vector3.left] = nodeDataList[2].validNodes;  
        validNodeDictionary[Vector3.right] = nodeDataList[3].validNodes;     
        validNodeDictionary[Vector3.forward] = nodeDataList[4].validNodes;
        validNodeDictionary[Vector3.back] = nodeDataList[5].validNodes;
    }

    public List<string> GetValidNeighbours(Vector3 currentCoords, Vector3 dir)
    {
        return validNodeDictionary[dir];
    }

    /*public List<WFCNode> GetValidNeighbours(Vector3 currentCoords, Vector3 dir)
    {
        return validNodes[dir];
    }*/

    public int GetValidNodeCount()
    {
        int count = 0;

        foreach (NodeData nodeData in nodeDataList)
        {
            foreach(string name in nodeData.validNodes)
            {
                count++;
            }
        }

        return count;
    }
}

[System.Serializable]
public class NodeData
{
    public List<string> validNodes;
}
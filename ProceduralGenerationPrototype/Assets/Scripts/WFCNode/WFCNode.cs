using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WFCNode", menuName = "WFC/Node")]
[System.Serializable]
public class WFCNode : ScriptableObject
{
    public GameObject prefab;
    public List<FaceData> faceDataList = new List<FaceData>();
    public Dictionary<Vector3, string> faces = new Dictionary<Vector3, string>();
    public int weight;

    public WFCConnection viableTopNodes;
    public WFCConnection viableBottomNodes;
    public WFCConnection viableLeftNodes;
    public WFCConnection viableRightNodes;
    public WFCConnection viableForwardNodes;
    public WFCConnection viableBackwardNodes;

    public Dictionary<string, List<string>> validFaceConstraints
    {
        get { return FaceConstraints.faceConstraints; }
    }

    private void OnEnable()
    {
        PopulateFaceDictionary();
    }

    //called when changes happen in editor
    private void OnValidate()
    {
        //AutoPopulateOppositeConnections();
    }

    //auto-populate the equivalent node
    private void AutoPopulateOppositeConnections()
    {
        foreach (var topNode in viableTopNodes.compatibleNodes)
        {
            if (!topNode.viableBottomNodes.compatibleNodes.Contains(this))
            {
                topNode.viableBottomNodes.compatibleNodes.Add(this);
            }
            else
            {
                topNode.viableBottomNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var bottomNode in viableBottomNodes.compatibleNodes)
        {
            if (!bottomNode.viableTopNodes.compatibleNodes.Contains(this))
            {
                bottomNode.viableTopNodes.compatibleNodes.Add(this);
            }
            else
            {
                bottomNode.viableTopNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var leftNode in viableLeftNodes.compatibleNodes)
        {
            if (!leftNode.viableRightNodes.compatibleNodes.Contains(this))
            {
                leftNode.viableRightNodes.compatibleNodes.Add(this);
            }
            else
            {
                leftNode.viableRightNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var rightNode in viableRightNodes.compatibleNodes)
        {
            if (!rightNode.viableLeftNodes.compatibleNodes.Contains(this))
            {
                rightNode.viableLeftNodes.compatibleNodes.Add(this);
            }
            else
            {
                rightNode.viableLeftNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var forwardNode in viableForwardNodes.compatibleNodes)
        {
            if (!forwardNode.viableBackwardNodes.compatibleNodes.Contains(this))
            {
                forwardNode.viableBackwardNodes.compatibleNodes.Add(this);
            }
            else
            {
                forwardNode.viableBackwardNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var backwardNode in viableBackwardNodes.compatibleNodes)
        {
            if (!backwardNode.viableForwardNodes.compatibleNodes.Contains(this))
            {
                backwardNode.viableForwardNodes.compatibleNodes.Add(this);
            }
            else
            {
                backwardNode.viableForwardNodes.compatibleNodes.Remove(this);
            }
        }
    }

    public void PopulateFaceDictionary()
    {
        if (faces != null)
        {
            faces.Clear(); //make sure it's freshly populated
        }

        //tie each label to a direction
        faces[Vector3.up] = faceDataList[0].label;
        faces[Vector3.down] = faceDataList[1].label;     
        faces[Vector3.left] = faceDataList[2].label;  
        faces[Vector3.right] = faceDataList[3].label;     
        faces[Vector3.forward] = faceDataList[4].label;     
        faces[Vector3.back] = faceDataList[5].label;
    }
}

[System.Serializable]
public class WFCConnection
{
    public List<WFCNode> compatibleNodes;
}

[System.Serializable]
public class FaceData
{
    public string label;
}

public static class FaceConstraints
{
    public static Dictionary<string, List<string>> faceConstraints = new Dictionary<string, List<string>>
    {
        { "-1", new List<string>() { "3" } },
        { "0", new List<string>() { "1" } },
        { "1", new List<string>(){ "1f" } },
        { "1f", new List<string>() { "1" } },
        { "2s", new List<string>() { "2s" } },
        { "3", new List<string>() { "-1" } },
        { "4_1", new List<string>() { "4_1" } },
        { "4_2", new List<string>() { "4_2" } },
        { "4_3", new List<string>() { "4_3" } },
        { "4_4", new List<string>() { "4_4" } },
        { "5_1", new List<string>() { "5_1" } },
        { "5_2", new List<string>() { "5_2" } },
        { "5_3", new List<string>() { "5_3" } },
        { "5_4", new List<string>() { "5_4" } }
    };

}

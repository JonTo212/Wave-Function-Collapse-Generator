using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WFCNode", menuName = "WFC/Node")]
[System.Serializable]
public class WFCNode : ScriptableObject
{
    public GameObject prefab;
    public int weight;

    public WFCConnection viableTopNodes;
    public WFCConnection viableBottomNodes;
    public WFCConnection viableLeftNodes;
    public WFCConnection viableRightNodes;

    // Called when the object is created or modified in the Unity editor.
    private void OnValidate()
    {
        AutoPopulateOppositeConnections();
    }

    // This method will auto-populate the opposite connections
    private void AutoPopulateOppositeConnections()
    {
        // Update all viable top nodes
        foreach (var topNode in viableTopNodes.compatibleNodes)
        {
            if (!topNode.viableBottomNodes.compatibleNodes.Contains(this))
            {
                topNode.viableBottomNodes.compatibleNodes.Add(this);
            }
        }

        // Update all viable bottom nodes
        foreach (var bottomNode in viableBottomNodes.compatibleNodes)
        {
            if (!bottomNode.viableTopNodes.compatibleNodes.Contains(this))
            {
                bottomNode.viableTopNodes.compatibleNodes.Add(this);
            }
        }

        // Update all viable left nodes
        foreach (var leftNode in viableLeftNodes.compatibleNodes)
        {
            if (!leftNode.viableRightNodes.compatibleNodes.Contains(this))
            {
                leftNode.viableRightNodes.compatibleNodes.Add(this);
            }
        }

        // Update all viable right nodes
        foreach (var rightNode in viableRightNodes.compatibleNodes)
        {
            if (!rightNode.viableLeftNodes.compatibleNodes.Contains(this))
            {
                rightNode.viableLeftNodes.compatibleNodes.Add(this);
            }
        }
    }
}

[System.Serializable]
public class WFCConnection
{
    public List<WFCNode> compatibleNodes;
}

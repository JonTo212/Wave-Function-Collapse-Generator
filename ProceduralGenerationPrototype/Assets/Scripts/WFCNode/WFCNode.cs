using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WFCNode", menuName = "WFC/Node")]
[System.Serializable]
public class WFCNode : ScriptableObject
{
    public GameObject prefab;
    //[HideInInspector] public GameObject instantiatedObject; //save object for regeneration -> for some reason this doesn't properly destroy all of them
    public int weight;

    public WFCConnection viableTopNodes;
    public WFCConnection viableBottomNodes;
    public WFCConnection viableLeftNodes;
    public WFCConnection viableRightNodes;

    //called when changes happen in editor
    private void OnValidate()
    {
        AutoPopulateOppositeConnections();
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
        }

        foreach (var bottomNode in viableBottomNodes.compatibleNodes)
        {
            if (!bottomNode.viableTopNodes.compatibleNodes.Contains(this))
            {
                bottomNode.viableTopNodes.compatibleNodes.Add(this);
            }
        }

        foreach (var leftNode in viableLeftNodes.compatibleNodes)
        {
            if (!leftNode.viableRightNodes.compatibleNodes.Contains(this))
            {
                leftNode.viableRightNodes.compatibleNodes.Add(this);
            }
        }

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

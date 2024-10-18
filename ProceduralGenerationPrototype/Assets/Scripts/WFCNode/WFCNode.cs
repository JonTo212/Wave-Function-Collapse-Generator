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
    public WFCConnection viableForwardNodes;
    public WFCConnection viableBackwardNodes;

    //called when changes happen in editor
    private void OnValidate()
    {
        AutoRemoveOppositeConnections();
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

        foreach (var forwardNode in viableForwardNodes.compatibleNodes)
        {
            if (!forwardNode.viableBackwardNodes.compatibleNodes.Contains(this))
            {
                forwardNode.viableBackwardNodes.compatibleNodes.Add(this);
            }
        }

        foreach (var backwardNode in viableBackwardNodes.compatibleNodes)
        {
            if (!backwardNode.viableForwardNodes.compatibleNodes.Contains(this))
            {
                backwardNode.viableForwardNodes.compatibleNodes.Add(this);
            }
        }
    }

    //auto-populate the equivalent node
    private void AutoRemoveOppositeConnections()
    {
        foreach (var topNode in viableTopNodes.compatibleNodes)
        {
            if (topNode.viableBottomNodes.compatibleNodes.Contains(this))
            {
                topNode.viableBottomNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var bottomNode in viableBottomNodes.compatibleNodes)
        {
            if (bottomNode.viableTopNodes.compatibleNodes.Contains(this))
            {
                bottomNode.viableTopNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var leftNode in viableLeftNodes.compatibleNodes)
        {
            if (leftNode.viableRightNodes.compatibleNodes.Contains(this))
            {
                leftNode.viableRightNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var rightNode in viableRightNodes.compatibleNodes)
        {
            if (rightNode.viableLeftNodes.compatibleNodes.Contains(this))
            {
                rightNode.viableLeftNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var forwardNode in viableForwardNodes.compatibleNodes)
        {
            if (forwardNode.viableBackwardNodes.compatibleNodes.Contains(this))
            {
                forwardNode.viableBackwardNodes.compatibleNodes.Remove(this);
            }
        }

        foreach (var backwardNode in viableBackwardNodes.compatibleNodes)
        {
            if (backwardNode.viableForwardNodes.compatibleNodes.Contains(this))
            {
                backwardNode.viableForwardNodes.compatibleNodes.Remove(this);
            }
        }
    }
}

[System.Serializable]
public class WFCConnection
{
    public List<WFCNode> compatibleNodes;
}

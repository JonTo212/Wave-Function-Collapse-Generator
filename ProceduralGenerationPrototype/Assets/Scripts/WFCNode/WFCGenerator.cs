    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCGenerator : MonoBehaviour
{
    [Header("Grid Parameters")]
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;

    private WFCNode[,] grid;

    [Header("Node Parameters")]

    [SerializeField] private List<WFCNode> nodes;                    //list of all nodes
    private List<Vector2Int> toCollapse = new List<Vector2Int>();    //list of nodes that still need to be collapsed

    private Vector2Int[] neighbourCoordinates = new Vector2Int[]
    {
        new Vector2Int(0, 1),          //up
        new Vector2Int(0, -1),         //down
        new Vector2Int(-1, 0),         //left
        new Vector2Int(1, 0),          //right
    };

    #region Helper Functions

    private bool IsInsideGrid(Vector2Int gridPos) //check whether a given node's position is inside the grid (to ensure no checks out of bounds)
    {
        if (gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ReducePossibleNodes(List<WFCNode> potentialNodes, List<WFCNode> validNodes) //remove all nodes that are invalid
    {
        for (int i = potentialNodes.Count - 1; i >= 0; i--) //start at the end of the list so that it doesn't skip over nodes as they're being removed
        {
            if (!validNodes.Contains(potentialNodes[i]))
            {
                potentialNodes.RemoveAt(i);
            }
        }
    }

    private WFCNode GetHighestWeightNode(List<WFCNode> potentialNodes)
    {
        WFCNode highestWeightedNode = potentialNodes[0];
        int highestWeight = highestWeightedNode.weight;

        for (int i = 1; i < potentialNodes.Count; i++) //loop through all potential nodes, starting with the first, updating weight and corresponding node each time
        {
            if (potentialNodes[i].weight > highestWeight)
            {
                highestWeight = potentialNodes[i].weight;
                highestWeightedNode = potentialNodes[i];
            }
        }

        return highestWeightedNode;
    }

    #endregion

    #region Regenerate function

    private void DestroyGrid() //for regenerating -> for some reason using node.instantiatedObject doesn't destroy everything
    {
        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("WFCNode");
        foreach (GameObject go in instantiatedObjects)
        {
            Destroy(go);
        }
    }

    public void Regenerate()
    {
        DestroyGrid();
        InitializeGrid();
        CollapseGrid();
    }

    #endregion

    private void Start()
    {
        InitializeGrid();
        CollapseGrid();
    }

    private void InitializeGrid()
    {
        grid = new WFCNode[gridWidth, gridHeight];
    }

    private void CollapseGrid()
    {
        toCollapse.Clear();

        toCollapse.Add(new Vector2Int(gridWidth / 2, gridHeight / 2)); //start with middle tile

        while (toCollapse.Count > 0)
        {
            int x = toCollapse[0].x;
            int y = toCollapse[0].y;

            List<WFCNode> potentialNodes = new List<WFCNode>(nodes); //create a new list with all of the nodes to begin with

            for (int i = 0; i < neighbourCoordinates.Length; i++) //loop through all neighbours (i.e. up, down, left, right)
            {
                Vector2Int neighbour = new Vector2Int(x + neighbourCoordinates[i].x, y + neighbourCoordinates[i].y); //this represents each neighbour position

                if (IsInsideGrid(neighbour))
                {
                    WFCNode neighbourNode = grid[neighbour.x, neighbour.y];

                    if (neighbourNode != null) //if the neighbour node is null, it needs to be collapsed still
                    {

                        WFCConnection[] connections = new WFCConnection[]
                        {
                            neighbourNode.viableBottomNodes,  // index 0 = top node, so the corresponding nodes to check are viable bottom nodes
                            neighbourNode.viableTopNodes,     // index 1 = bottom node, so the corresponding nodes to check are viable top nodes
                            neighbourNode.viableRightNodes,   // index 2 = left node, so the corresponding nodes to check are viable right nodes
                            neighbourNode.viableLeftNodes     // index 3 = right node, so the corresponding nodes to check are viable left nodes
                        };

                        ReducePossibleNodes(potentialNodes, connections[i].compatibleNodes); //reduce possible nodes for this tile

                    }
                    else
                    {
                        if (!toCollapse.Contains(neighbour))
                        {
                            toCollapse.Add(neighbour); //node has not been collapsed, so add it to the list of nodes needing to be collapsed
                        }
                    }
                }
            }

            if (potentialNodes.Count < 1) //no possible tiles based on constraints -> this can be changed if desired
            {
                grid[x, y] = nodes[0]; //if no other possibilities, just add a floor
                //Debug.Log("Attempted to collapse on " + x + ", " + y + " but no compatible tiles.");
            }
            else
            {
                grid[x, y] = potentialNodes[Random.Range(0, potentialNodes.Count)]; //choose random node
                //grid[x, y] = GetHighestWeightNode(potentialNodes); //choose the highest weighted node
            }

            GameObject newNode = Instantiate(grid[x, y].prefab, new Vector3(x, 0, y), Quaternion.identity);
            //grid[x, y].instantiatedObject = newNode; //for some reason this doesn't work
            toCollapse.RemoveAt(0);
        }
    }
}

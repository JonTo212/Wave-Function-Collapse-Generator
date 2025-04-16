using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCGenerator : MonoBehaviour
{
    [Header("Grid Parameters")]
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private int gridDepth;

    private NodeState [,,] grid; //3D array

    [Header("Node Parameters")]
    [SerializeField] private List<WFCNode> groundNodes;              //list of all ground nodes
    [SerializeField] private List<WFCNode> airNodes;                 //list of all air nodes
    [SerializeField] private List<WFCNode> groundRightEdgeNodes;              //list of all ground edge nodes
    [SerializeField] private List<WFCNode> groundLeftEdgeNodes;              //list of all ground edge nodes
    [SerializeField] private List<WFCNode> groundTopEdgeNodes;              //list of all ground edge nodes
    [SerializeField] private List<WFCNode> groundBottomEdgeNodes;              //list of all ground edge nodes
    [SerializeField] private WFCNode emptyNode;
    [SerializeField] private WFCNode fallBackNode;
    private List<Vector3Int> toCollapse = new List<Vector3Int>();    //list of nodes that still need to be collapsed

    private Vector3Int[] neighbourCoordinates = new Vector3Int[]
    {
        new Vector3Int(0, 1, 0),          //up
        new Vector3Int(0, -1, 0),         //down
        new Vector3Int(-1, 0, 0),         //left
        new Vector3Int(1, 0, 0),          //right
        new Vector3Int(0, 0, 1),          //forward
        new Vector3Int(0, 0, -1)          //backward
    };

    #region Helper Functions

    private bool IsInsideGrid(Vector3Int gridPos) //check whether a given node's position is inside the grid (to ensure no checks out of bounds)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth &&
           gridPos.y >= 0 && gridPos.y < gridHeight &&
           gridPos.z >= 0 && gridPos.z < gridDepth;
    }

    private void ReducePossibleNodes(List<WFCNode> potentialNodes, WFCNode neighbourNode, Vector3 direction)
    {
        for (int i = potentialNodes.Count - 1; i >= 0; i--) //start at the end of the list so that it doesn't skip over nodes as they're being removed
        {
            if (!neighbourNode.validNodeDictionary[-direction].Contains(potentialNodes[i].prefabName))
            {
                potentialNodes.RemoveAt(i);
            }
        }
    }

    private bool Collapsed()
    {
        foreach(NodeState node in grid)
        {
            if (!node.collapsed)
            {
                return false;
            }
        }
        return true;
    }

    private Vector3Int GetMinEntropyCoordsInLayer(int y)
    {
        Vector3Int minCoords = new Vector3Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight), Random.Range(0, gridDepth));
        int minCount = int.MaxValue;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridDepth; z++)
            {
                if (!grid[x, y, z].collapsed)
                {
                    int count = grid[x, y, z].potentialNodes.Count;
                    if (count < minCount)
                    {
                        minCount = count;
                        minCoords = new Vector3Int(x, y, z);
                    }
                    else if (count == minCount) //randomize when they're the same
                    {
                        if (Random.Range(0, 2) == 0)
                        {
                            minCoords = new Vector3Int(x, y, z);
                        }
                    }
                }
            }
        }
        return minCoords;
    }

    private WFCNode GetWeightedRandomNode(List<WFCNode> potentialNodes)
    {
        int totalWeight = potentialNodes.Sum(node => node.weight);
        int randomWeight = Random.Range(0, totalWeight);

        int cumulativeWeight = 0;
        foreach (WFCNode node in potentialNodes)
        {
            cumulativeWeight += node.weight;
            if (randomWeight < cumulativeWeight)
            {
                return node;
            }
        }
        return potentialNodes[0];
    }


    #endregion

    #region Regenerate function

    public void DestroyGrid() //for regenerating -> for some reason using node.instantiatedObject doesn't destroy everything
    {
        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("WFCNode");
        foreach (GameObject go in instantiatedObjects)
        {
            DestroyImmediate(go);
        }
    }

    public void Regenerate()
    {
        DestroyGrid();
        InitializeGrid();
        WFC();
        //CollapseGrid();
    }

    #endregion

    private void Start()
    {
        InitializeGrid();
        WFC();
    }

    private void InitializeGrid()
    {
        grid = new NodeState[gridWidth, gridHeight, gridDepth];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    grid[x, y, z] = new NodeState();

                    grid[x, y, z].potentialNodes = new List<WFCNode>();

                    grid[x, y, z].potentialNodes.AddRange(groundNodes);

                    if (y > 0)
                    {
                        grid[x, y, z].potentialNodes.AddRange(airNodes); //only add air nodes if above ground level
                    }

                    grid[x, y, z].potentialNodes.Add(emptyNode);
                    grid[x, y, z].currentNode = null;
                }
            }
        }
    }

    private void WFC()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            while (!LayerCollapsed(y))
            {
                IterateLayer(y);
            }
        }
        Visualize();
    }

    private bool LayerCollapsed(int y)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridDepth; z++)
            {
                if (!grid[x, y, z].collapsed)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void IterateLayer(int y)
    {
        Vector3Int coords = GetMinEntropyCoordsInLayer(y);
        CollapseAt(coords);
        Propagate(coords);
    }

    private void CollapseAt(Vector3Int coords)
    {
        int x = coords.x;
        int y = coords.y;
        int z = coords.z;

        if (grid[x, y, z].potentialNodes.Count < 1) //no possible tiles
        {
            grid[x, y, z].currentNode = fallBackNode; //if no other possibilities, add fallback tile
            print("Broken at: " + new Vector3(x, y, z));
        }
        else
        {
            grid[x, y, z].currentNode = GetWeightedRandomNode(grid[x, y, z].potentialNodes);
            //grid[x, y, z].currentNode = grid[x, y, z].potentialNodes[Random.Range(0, grid[x, y, z].potentialNodes.Count)]; //choose random node
            grid[x, y, z].potentialNodes.Clear();
            grid[x, y, z].potentialNodes.Add(grid[x, y, z].currentNode); //remove the rest of the nodes from potentialNodes so there is only 1 left
        }

        grid[x, y, z].collapsed = true;
    }

    private void HandleEdgeNodes(Vector3Int coords)
    {
        int x = coords.x;
        int y = coords.y;
        int z = coords.z;

        List<WFCNode> possibleNodes = grid[x, y, z].potentialNodes;

        bool isLeftEdge = x == 0;
        bool isRightEdge = x == gridWidth - 1;
        bool isFrontEdge = z == 0;
        bool isBackEdge = z == gridDepth - 1;

        if (isLeftEdge)
            grid[x, y, z].potentialNodes.AddRange(groundLeftEdgeNodes);
        else if (isRightEdge)
            grid[x, y, z].potentialNodes.AddRange(groundRightEdgeNodes);
        else if (isFrontEdge)
            grid[x, y, z].potentialNodes.AddRange(groundTopEdgeNodes);
        else if (isBackEdge)
            grid[x, y, z].potentialNodes.AddRange(groundBottomEdgeNodes);
        else
            grid[x, y, z].potentialNodes.AddRange(groundNodes);

        if (grid[x, y, z].potentialNodes.Count == 0)
        {
            grid[x, y, z].potentialNodes.AddRange(groundNodes);
        }
    }

    private void Propagate(Vector3Int coords)
    {
        toCollapse.Add(coords);

        while (toCollapse.Count > 0)
        {
            toCollapse.RemoveAt(0);

            foreach (Vector3Int neighbour in neighbourCoordinates)
            {
                Vector3Int neighbourCoords = coords + neighbour;

                if (IsInsideGrid(neighbourCoords))
                {
                    List<WFCNode> otherPossibleNodes = grid[neighbourCoords.x, neighbourCoords.y, neighbourCoords.z].potentialNodes;

                    List<string> possibleNeighbours = grid[coords.x, coords.y, coords.z].currentNode.GetValidNeighbours(coords, neighbour);

                    if (grid[neighbourCoords.x, neighbourCoords.y, neighbourCoords.z].potentialNodes.Count == 0)
                        continue;
                  
                    for (int i = otherPossibleNodes.Count - 1; i >= 0; i--)
                    {
                        WFCNode node = otherPossibleNodes[i];
                        if (!possibleNeighbours.Contains(node.prefabName))
                        {
                            Constrain(neighbourCoords, node);
                            if (!toCollapse.Contains(neighbourCoords))
                            {
                                toCollapse.Add(neighbourCoords);
                            }
                        }
                    }
                }
            }
        }
    }

    private void Constrain(Vector3Int coords, WFCNode node)
    {
        grid[coords.x, coords.y, coords.z].potentialNodes.Remove(node);
    }

    private void Visualize()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    GameObject newNode = Instantiate(grid[x, y, z].currentNode.prefab, new Vector3(x, y, z), Quaternion.identity);
                }
            }
        }
    }
}

public class NodeState
{
    public List<WFCNode> potentialNodes;
    public WFCNode currentNode;
    public bool collapsed;
}

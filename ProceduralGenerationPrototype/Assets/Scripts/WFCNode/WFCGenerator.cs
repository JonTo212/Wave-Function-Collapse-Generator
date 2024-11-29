using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
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
    [SerializeField] private WFCNode emptyNode;
    [SerializeField] private WFCNode floorNode;
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
        grid = new NodeState[gridWidth, gridHeight, gridDepth];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    List<WFCNode> potentialNodes = new List<WFCNode>();

                    potentialNodes.AddRange(groundNodes);
                    potentialNodes.Add(floorNode);
                    potentialNodes.Add(emptyNode);

                    if (y > 0)
                    {
                        potentialNodes.AddRange(airNodes); //only add air nodes if above ground level
                    }


                    grid[x, y, z].potentialNodes = new List<WFCNode>(potentialNodes);
                    grid[x, y, z].currentNode = null;
                    grid[x, y, z].collapsed = false;
                }
            }
        }
    }

    private void CollapseGrid()
    {
        toCollapse.Clear();

        toCollapse.Add(new Vector3Int(gridWidth / 2, gridHeight / 2, gridDepth / 2)); //start with central tile

        while (toCollapse.Count > 0)
        {
            int x = toCollapse[0].x;
            int y = toCollapse[0].y;
            int z = toCollapse[0].z;

            //Create a list of all nodes

            for (int i = 0; i < neighbourCoordinates.Length; i++) //loop through all neighbours (i.e. up, down, left, right)
            {
                Vector3Int neighbour = new Vector3Int(x + neighbourCoordinates[i].x, y + neighbourCoordinates[i].y, z + neighbourCoordinates[i].z); //this represents each neighbour position

                if (IsInsideGrid(neighbour))
                {
                    WFCNode neighbourNode = grid[neighbour.x, neighbour.y, neighbour.z].currentNode;

                    if (neighbourNode != null) //if the neighbour node is null, it needs to be collapsed still
                    {

                        Vector3 dir = neighbourCoordinates[i];
                        ReducePossibleNodes(grid[x, y, z].potentialNodes, neighbourNode, dir); //reduce possible nodes for this tile

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

            bool broken = false;

            if (grid[x, y, z].potentialNodes.Count < 1) //no possible tiles based on constraints -> this can be changed if desired
            {
                grid[x, y, z].currentNode = floorNode; //if no other possibilities, add a floor
                broken = true;
            }
            else
            {
                grid[x, y, z].currentNode = grid[x, y, z].potentialNodes[Random.Range(0, grid[x, y, z].potentialNodes.Count)]; //choose random node
            }

            GameObject newNode = Instantiate(grid[x, y, z].currentNode.prefab, new Vector3(x, y, z), Quaternion.identity);
            
            if(broken)
            {
                newNode.name = $"{x}, {y}, {z}, broken";
            }

            toCollapse.RemoveAt(0);

            List<int> nodeCounts = new List<int>();
            for (int i = 0; i < toCollapse.Count; i++)
            {
                Vector3Int position = toCollapse[i];
                NodeState node = grid[position.x, position.y, position.z];
                if (node != null) //ISSUE IS THAT NODE IS NULL BECAUSE IT HAS NOT BEEN COLLAPSED YET. MAKE A NEW FUNCTION THAT REDUCES THE POTENTIAL NEIGHBOURS SOMEHOW
                {
                    nodeCounts[i] = node.currentNode.GetValidNodeCount();
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

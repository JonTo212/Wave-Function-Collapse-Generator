using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCTileGenerator : MonoBehaviour
{
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private int gridDepth;

    private TileState[,,] grid;

    [SerializeField] private List<WFCTile> groundTiles;
    [SerializeField] private List<WFCTile> pathEndTiles;
    [SerializeField] private List<WFCTile> pathTiles;
    [SerializeField] private List<WFCTile> crossroadTiles;
    [SerializeField] private WFCTile fallBackTile;
    //[SerializeField] private List<WFCTile> airTiles;
    //[SerializeField] private WFCTile emptyTile;

    private Queue<Vector3Int> toCollapse = new Queue<Vector3Int>();

    private Vector3Int start;
    private Vector3Int end;
    private List<WFCTile> pathableTiles;

    private Vector3Int[] neighbourCoordinates3D = new Vector3Int[]
    {
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    private Vector3Int[] neighbourCoordinates2D = new Vector3Int[]
    {
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    private static System.Random rng = new System.Random(); //to be called for Shuffle

    public void DestroyGrid() //for regenerating -> for some reason using node.instantiatedObject doesn't destroy everything
    {
        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("WFCTile");
        foreach (GameObject go in instantiatedObjects)
        {
            DestroyImmediate(go);
        }
    }

    public void Regenerate()
    {
        DestroyGrid();
        InitializeGrid();

        List<Vector3Int> multiPath = new List<Vector3Int>();
        multiPath.AddRange(TryCreatePath(start, end));
        multiPath.AddRange(TryCreatePath(end, start));
        CollapsePathTiles(multiPath);

        WFC();
    }

    private void Start()
    {
        InitializeGrid();
        WFC();
    }

    #region Wave Function Collapse
    private bool IsInsideGrid(Vector3Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth &&
               gridPos.y >= 0 && gridPos.y < gridHeight &&
               gridPos.z >= 0 && gridPos.z < gridDepth;
    }

    private void Visualize()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    GameObject tileObj = grid[x, y, z].currentTile.prefab;
                    Instantiate(tileObj, new Vector3(x, y, z), tileObj.transform.rotation);
                }
            }
        }
    }

    private void InitializeGrid()
    {
        grid = new TileState[gridWidth, gridHeight, gridDepth];
        start = new Vector3Int(0, 0, 0);
        end = new Vector3Int(gridWidth - 1, 0, gridDepth - 1);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    Vector3Int currentTile = new Vector3Int(x, y, z);
                    grid[x, y, z] = new TileState
                    {
                        potentialTiles = new List<WFCTile>(groundTiles),
                        currentTile = null,
                        collapsed = false
                    };

                    /*if (y > 0)
                    {
                        grid[x, y, z].potentialTiles.AddRange(airTiles);
                    }*/
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
                Vector3Int coords = GetMinEntropyCoordsInLayer(y);
                CollapseAt(coords);
                Propagate(coords);
            }
        }
        Visualize();
    }

    private bool LayerCollapsed(int y)
    {
        for (int x = 0; x < gridWidth; x++)
            for (int z = 0; z < gridDepth; z++)
                if (!grid[x, y, z].collapsed)
                    return false;
        return true;
    }

    private Vector3Int GetMinEntropyCoordsInLayer(int y)
    {
        Vector3Int minCoords = new Vector3Int(Random.Range(0, gridWidth), y, Random.Range(0, gridDepth));
        int minCount = int.MaxValue;

        for (int x = 0; x < gridWidth; x++)
            for (int z = 0; z < gridDepth; z++)
                if (!grid[x, y, z].collapsed)
                {
                    int count = grid[x, y, z].potentialTiles.Count;
                    if (count < minCount || (count == minCount && Random.Range(0, 2) == 0))
                    {
                        minCount = count;
                        minCoords = new Vector3Int(x, y, z);
                    }
                }
        return minCoords;
    }

    private void CollapseAt(Vector3Int coords)
    {
        var state = grid[coords.x, coords.y, coords.z];

        if (state.potentialTiles == null || state.potentialTiles.Count == 0)
        {
            state.currentTile = fallBackTile;
            //print("fallback at " + coords);
        }
        else
        {
            state.currentTile = GetRandomTile(state.potentialTiles);
        }

        state.potentialTiles.Clear();
        state.potentialTiles.Add(state.currentTile);
        state.collapsed = true;
    }

    private WFCTile GetWeightedRandomTile(List<WFCTile> potentialTiles)
    {
        if (potentialTiles == null || potentialTiles.Count == 0)
        {
            return fallBackTile;
        }
        int totalWeight = 0;
        foreach (var Tile in potentialTiles)
            totalWeight += Tile.weight;

        int randomWeight = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var Tile in potentialTiles)
        {
            cumulativeWeight += Tile.weight;
            if (randomWeight < cumulativeWeight)
                return Tile;
        }

        return potentialTiles[0];
    }

    private WFCTile GetRandomTile(List<WFCTile> potentialTiles)
    {
        if (potentialTiles == null || potentialTiles.Count == 0)
        {
            return fallBackTile;
        }

        int randomIndex = Random.Range(0, potentialTiles.Count);
        return potentialTiles[randomIndex];
    }

    private void Propagate(Vector3Int coords)
    {
        toCollapse.Enqueue(coords);

        while (toCollapse.Count > 0)
        {
            toCollapse.Dequeue();

            foreach (Vector3Int neighbour in neighbourCoordinates3D)
            {
                Vector3Int neighbourCoords = coords + neighbour;

                if (IsInsideGrid(neighbourCoords))
                {
                    var neighbourState = grid[neighbourCoords.x, neighbourCoords.y, neighbourCoords.z];
                    var currentState = grid[coords.x, coords.y, coords.z];

                    List<WFCTile> valid = new List<WFCTile>(neighbourState.potentialTiles);

                    foreach (var candidate in valid)
                    {
                        if (!currentState.currentTile.CanConnect(candidate, neighbour))
                        {
                            neighbourState.potentialTiles.Remove(candidate);
                            if (!toCollapse.Contains(neighbourCoords))
                                toCollapse.Enqueue(neighbourCoords);
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Pathfinding
    public List<Vector3Int> TryCreatePath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = ApplyPathfindingAlgorithm(start, end);

        //this is a fallback, to future proof any additions that might affect pathfinding
        if (path == null)
        {
            Debug.LogError("No path exists between start and end.");
            return new List<Vector3Int>();
        }

        return path;
    }

    private List<Vector3Int> GetNeighboursTowards(Vector3Int current, Vector3Int end)
    {
        //orders neighbours based on how close they are to the end tile
        return neighbourCoordinates2D.OrderBy(x => (current + x - end).sqrMagnitude).ToList();
    }
    public static List<T> Shuffle<T>(List<T> source)
    {
        //uses System.Random to order the list randomly
        return source.OrderBy(x => rng.Next()).ToList();
    }

    private List<Vector3Int> ApplyPathfindingAlgorithm(Vector3Int start, Vector3Int end)
    {
        //If you use a queue, it finds the shortest path by exploring the closest neighbours first (Breadth-First Search).
        //If use a stack, it explores the entire branch of possibilities in a given direction before returning to the next (Depth-First Search).
        Stack<Vector3Int> open = new Stack<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        //Queue = Enqueue, Stack = Push
        open.Push(start);
        cameFrom[start] = start;

        while (open.Count > 0)
        {
            //Queue = Dequeue, Stack = Pop
            Vector3Int current = open.Pop();
            if (current == end) break;

            List<Vector3Int> neighbours = GetNeighboursTowards(current, end);
            neighbours = Shuffle(neighbours);

            foreach (var direction in neighbours)
            {
                Vector3Int newDir = current + direction;

                //skip if neighbour is out of bounds, above ground level or we've already checked it
                if (!IsInsideGrid(newDir) || newDir.y != 0 || cameFrom.ContainsKey(newDir)) 
                    continue;

                //check all 4 directions, but, if it is already in cameFrom dictionary, skip it; each tile is only visited once
                //i.e. this saves the TILE and corresponding DIRECTION.
                //Queue = Enqueue, Stack = Push
                open.Push(newDir);
                cameFrom[newDir] = current;
            }
        }

        if (!cameFrom.ContainsKey(end))
            return null;

        /* rebuild path backwards and flip it, rather than inserting from index 0 for efficiency
         * can't use cameFrom, because it tells you the tile and corresponding direction that it was relative to the original.
         * cameFrom[currentReverse] gets the corresponding KEY for currentReverse, and saves it as currentReverse -> this allows us to move backwards through the dictionary and build the list.
         */
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int currentReverse = end;

        while (currentReverse != start)
        {
            path.Add(currentReverse);
            currentReverse = cameFrom[currentReverse];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    private void CollapsePathTiles(List<Vector3Int> path)
    {
        //build dictionary of the grid positions and associated hashset of directions that have path tiles (hashset so there's no duplicates for when tiles overlap)
        Dictionary<Vector3Int, HashSet<Vector3Int>> connectionsAtPoint = new Dictionary<Vector3Int, HashSet<Vector3Int>>();

        //populate dictionary with blank hashsets to avoid errors
        foreach (Vector3Int pos in path)
        {
            connectionsAtPoint[pos] = new HashSet<Vector3Int>();
        }

        //loop through path and save all previous and next tile directions, because we know they're paths
        for (int i = 0; i < path.Count; i++)
        {
            Vector3Int current = path[i];

            //inDir -> previous tile direction
            if (i > 0)
            {
                Vector3Int inDir = path[i - 1] - current;
                if (inDir != Vector3Int.zero)
                    connectionsAtPoint[current].Add(inDir);
            }

            //outDir -> next tile direction
            if (i < path.Count - 1)
            {
                Vector3Int outDir = path[i + 1] - current;
                if (outDir != Vector3Int.zero)
                    connectionsAtPoint[current].Add(outDir);
            }
        }

        //path.Distinct() functions like a hashset -> only checks duplicates once
        foreach (Vector3Int currentPoint in path.Distinct())
        {
            TileState state = grid[currentPoint.x, currentPoint.y, currentPoint.z];
            HashSet<Vector3Int> requiredConnections = connectionsAtPoint[currentPoint]; //get the hashset of all connection directions tied to the current spot
            List<WFCTile> tilePool = new List<WFCTile>();

            if (requiredConnections.Count == 1) //if only one connection, use path end
                tilePool = pathEndTiles; 
            else if (requiredConnections.Count == 2) //if 2 connections, use straight or corner path
                tilePool = pathTiles;
            else
                tilePool = crossroadTiles; //3+ connections = crossroad

            Debug.Log(
    $"Point {currentPoint}  dirCount={requiredConnections.Count}  " +
    $"dirs=[{string.Join(",", requiredConnections)}]  pool={tilePool.Count}"
);  

            //loop through the pool of tiles and choose the one that would fit the requirements
            WFCTile chosen = PickBestTile(tilePool, requiredConnections);

            if (chosen == null)
            {
                Debug.LogWarning($"No suitable path tile for {currentPoint}. Using fallback.");
                chosen = fallBackTile;
            }

            state.currentTile = chosen;
            state.potentialTiles.Clear();
            state.potentialTiles.Add(chosen);
            state.collapsed = true;

            Propagate(currentPoint);
        }
    }

    private WFCTile PickBestTile(List<WFCTile> pool, HashSet<Vector3Int> requiredConnections)
    {
        HashSet<Vector3Int> connectionDirs = requiredConnections;

        int fewestExtras = int.MaxValue;
        List<WFCTile> bestTiles = new List<WFCTile>();

        foreach (WFCTile tile in pool)
        {
            //checks if current tile from pool (e.g. path tiles + corner tiles) has a valid connection face in the required connection direction
            bool isValid = true;
            foreach (Vector3Int dir in connectionDirs)
            {
                if (!tile.HasConnector(dir))
                {
                    isValid = false;
                    break;
                }
            }
            if (!isValid) continue;


            //if the tile passed the previous check, check how many faces it has (GetConnectors returns all of the pathable faces)
            //if there's more than the valid faces, they're counted as extras
            int extras = 0;
            foreach (Vector3Int connections in tile.GetConnectors())
            {
                if (!connectionDirs.Contains(connections))
                {
                    extras++;
                }
            }

            //if there's less extra connectors than any tile we've seen so far (remember this is a foreach loop), clear the list of bestTiles and add the current one
            //this is because the current tile is better than the previous one
            if (extras < fewestExtras)
            {
                fewestExtras = extras;
                bestTiles.Clear();
                bestTiles.Add(tile);
            }

            //if it's equivalent, add it to the list
            else if (extras == fewestExtras)
            {
                bestTiles.Add(tile);
            }

            //stop checking if we find a 'perfect' tile
            if (fewestExtras == 0)
                break;
        }

        if (bestTiles.Count > 0)
            return bestTiles[rng.Next(bestTiles.Count)];
        else
            return fallBackTile;
    }
    #endregion
}
public class TileState
{
    public List<WFCTile> potentialTiles;
    public WFCTile currentTile;
    public bool collapsed;
}
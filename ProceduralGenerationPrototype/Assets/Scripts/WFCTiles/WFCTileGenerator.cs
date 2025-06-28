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
    [SerializeField] private WFCTile crossRoadTile;
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

        bool pathCreated = TryCreatePath(start, end);
        if (!pathCreated)
        {
            Debug.LogError("Failed to create path.");
            return;
        }

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
    public bool TryCreatePath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = ApplyPathfindingAlgorithm(start, end);

        //this is a fallback, to future proof any additions that might affect pathfinding
        if (path == null)
        {
            Debug.LogError("No path exists between start and end.");
            return false;
        }

        //choose path tiles first
        CollapsePathTiles(path);
        return true;
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
        WFCTile prevTile = null;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3Int current = path[i];
            List<WFCTile> tileOptions;

            /* get previous and next tile positions */
            //inDir = current tile pos - previous tile pos
            Vector3Int inDir = Vector3Int.zero;
            if (i > 0)
                inDir = path[i - 1] - current;

            //outDir = next tile pos - current tile pos
            Vector3Int outDir = Vector3Int.zero;
            if (i < path.Count - 1)
                outDir = path[i + 1] - current;


            //starting tiles
            if (i == 0 || i == path.Count - 1) 
                tileOptions = pathEndTiles; 

            //everything in between
            else
                tileOptions = pathTiles;


            /* remove any tiles that are 
            /* if inDir == 0 -> starting tile
             * if outDir == 0 -> ending tile
             * if tile.HasConnector(inDir) -> check if current option on list has a pathable tile with a path face for the previous tile
             * if tile.HasConnector(outDir) -> check if current option on list has pathable tile with path face for the next tile
             * end result is a list of tiles that has connections on both sides, or is a start/end tile
            */

            /*
            //this is for debugging
            foreach(var tile in tileOptions)
            {
                bool hasIn = (inDir == Vector3Int.zero) || tile.HasConnector(inDir);
                bool hasOut = (outDir == Vector3Int.zero) || tile.HasConnector(outDir);

                if (!hasIn || !hasOut)
                {
                    Debug.Log(
                        $"[REJECTED] Tile: {tile.name} at {current} | " +
                        $"inDir: {inDir}, outDir: {outDir} | " +
                        $"HasConnector(in): {tile.HasConnector(inDir)} | " +
                        $"HasConnector(out): {tile.HasConnector(outDir)}");
                }
                else
                {
                    Debug.Log(
                        $"[ACCEPTED] Tile: {tile.name} at {current} | " +
                        $"inDir: {inDir}, outDir: {outDir} | " +
                        $"HasConnector(in): {tile.HasConnector(inDir)} | " +
                        $"HasConnector(out): {tile.HasConnector(outDir)}");
                }
            }
            */

            List<WFCTile> candidates = tileOptions.Where(tile =>
                (inDir == Vector3Int.zero || tile.HasConnector(inDir)) &&
                (outDir == Vector3Int.zero || tile.HasConnector(outDir))
                ).ToList();

            //ensure that previous tile can connect to current tile
            if (prevTile != null && outDir != Vector3Int.zero)
            {
                candidates = candidates.Where(tile => prevTile.CanConnect(tile, outDir)).ToList();
            }

            //returns first tile in the list (first tile that fits the parameters) or null if there's nothing
            //place the fallback if nothing there
            WFCTile chosen = candidates.FirstOrDefault();
            if (chosen == null)
            {
                Debug.LogWarning($"No compatible tile at {current}, using fallback.");
                chosen = fallBackTile;
            }

            TileState state = grid[current.x, current.y, current.z];
            state.currentTile = chosen;
            state.potentialTiles = new List<WFCTile> { chosen };
            state.collapsed = true;

            Propagate(current);
            prevTile = chosen;

            //Debug.Log($"At position {current}, inDir: {inDir}, outDir: {outDir}, candidate count: {candidates.Count()}");
        }
    }
    #endregion
}
public class TileState
{
    public List<WFCTile> potentialTiles;
    public WFCTile currentTile;
    public bool collapsed;
}
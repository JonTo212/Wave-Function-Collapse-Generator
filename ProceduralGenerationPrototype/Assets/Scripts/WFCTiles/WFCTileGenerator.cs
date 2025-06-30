using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region PathType
public enum PathMode
{
    Manual,
    Random,
    EntireGrid
}

public enum TileType
{
    Ground,
    Path,
    PathEnd,
    Crossroad
}

[System.Serializable]
public class PathProperties
{
    public PathMode mode;
    public Vector3Int start;
    public Vector3Int end;
    public bool useShortestPath;

    //switch path generation type based on enum selected in editor, set start and end points accordingly
    public void Generate(int gridWidth, int gridDepth)
    {
        switch (mode)
        {
            case PathMode.Random:
                start = new Vector3Int(Random.Range(0, gridWidth), 0, Random.Range(0, gridDepth));
                end = new Vector3Int(Random.Range(0, gridWidth), 0, Random.Range(0, gridDepth));
                break;

            case PathMode.EntireGrid:
                start = Vector3Int.zero;
                end = new Vector3Int(gridWidth - 1, 0, gridDepth - 1);
                break;

            case PathMode.Manual:
            default:
                break;
        }
    }
}
#endregion

public class WFCTileGenerator : MonoBehaviour
{
    [Header("Grid setup")]
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private int gridDepth;
    private TileState[,,] grid;

    [Header("Tiles")]
    [SerializeField] private List<WFCTile> groundTiles;
    [SerializeField] private List<WFCTile> pathEndTiles;
    [SerializeField] private List<WFCTile> pathTiles;
    [SerializeField] private List<WFCTile> crossroadTiles;
    [SerializeField] private WFCTile fallBackTile;
    private Dictionary<TileType, List<WFCTile>> tileTypes = new Dictionary<TileType, List<WFCTile>>();
    private HashSet<WFCTile> allTiles = new HashSet<WFCTile>();
    //[SerializeField] private List<WFCTile> airTiles;
    //[SerializeField] private WFCTile emptyTile;

    [Header("WFC queue")]
    private Queue<Vector3Int> toCollapse = new Queue<Vector3Int>();

    [Header("Directional coordinates")]
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

    [Header("Pathfinding")]
    private static System.Random rng = new System.Random(); //to be called for Shuffle
    [SerializeField] public List<PathProperties> paths = new List<PathProperties>();

    #region UI button functions
    public void PopulateTileTypes()
    {
        tileTypes.Clear();
        tileTypes[TileType.Ground] = groundTiles;
        tileTypes[TileType.Path] = pathTiles;
        tileTypes[TileType.PathEnd] = pathEndTiles;
        tileTypes[TileType.Crossroad] = crossroadTiles;
    }

    public void DestroyGrid() //for regenerating -> for some reason using node.instantiatedObject doesn't destroy everything
    {
        GameObject[] instantiatedObjects = GameObject.FindGameObjectsWithTag("WFCTile");
        foreach (GameObject go in instantiatedObjects)
        {
            DestroyImmediate(go);
        }
    }

    public void RegenerateWFC() //for WFC button
    {
        DestroyGrid();
        InitializeGridWithAllTiles();

        WFC();
    }

    public void RegeneratePath() //for WFC + Pathfinding button
    {
        DestroyGrid();
        InitializeGridForPath();

        CollapsePathTiles(GeneratePaths());
        WFC();
    }

    public void AddTileType(TileType tileType) //for WFC button
    {
        PopulateTileTypes();

        //UnionWith is just AddRange for a hashset -> use hashset so there's no dupes
        allTiles.UnionWith(tileTypes[tileType]);
    }

    public void RemoveTileType(TileType tileType) //for WFC button
    {
        PopulateTileTypes();
        List<WFCTile> tilesToRemove = tileTypes[tileType];

        foreach (WFCTile tile in tilesToRemove)
        {
            allTiles.Remove(tile);
        }
    }

    public void AddAllTiles() //for WFC button
    {
        PopulateTileTypes();
        allTiles.Clear();

        foreach (var kvp in tileTypes)
        {
            foreach (WFCTile tile in kvp.Value)
            {
                allTiles.Add(tile);
            }
        }
    }

    public void RemoveAllTiles() //for WFC button
    {
        allTiles.Clear();
    }
    #endregion

    #region Wave Function Collapse
    private bool IsInsideGrid(Vector3Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth &&
               gridPos.y >= 0 && gridPos.y < gridHeight &&
               gridPos.z >= 0 && gridPos.z < gridDepth;
    }

    private void Visualize()
    {
        //create parent to group a stored map
        Transform tileParent = new GameObject("GeneratedTiles").transform;
        tileParent.tag = "WFCTile";

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    GameObject tilePrefab = grid[x, y, z].currentTile.prefab;
                    GameObject newtileObj = Instantiate(tilePrefab, new Vector3(x, y, z), tilePrefab.transform.rotation, tileParent);
                    newtileObj.name = $"Tile ({x},{y},{z})";
                }
            }
        }
    }

    private void InitializeGridWithAllTiles()
    {
        //build grid, fill it with data structures that determine whether the grid point is collapsed
        grid = new TileState[gridWidth, gridHeight, gridDepth];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    //only give the potential tiles list non-pathed tiles, since we are building the path separately
                    Vector3Int currentTile = new Vector3Int(x, y, z);

                    grid[x, y, z] = new TileState
                    {
                        potentialTiles = new List<WFCTile>(allTiles),
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
                //get the grid point with the least options and collapse it first
                Vector3Int coords = GetMinEntropyCoordsInLayer(y);
                CollapseAt(coords);
                Propagate(coords);
            }
        }
        Visualize();
    }

    private bool LayerCollapsed(int y)
    {
        //checker for if the entirety of one layer (i.e. one level of y) is completely collapsed
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

    private Vector3Int GetMinEntropyCoordsInLayer(int y)
    {
        Vector3Int minCoords = new Vector3Int(Random.Range(0, gridWidth), y, Random.Range(0, gridDepth));
        int minCount = int.MaxValue;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridDepth; z++)
            {
                if (!grid[x, y, z].collapsed)
                {
                    //compare current tile's potential tiles to the previous, if there's less then save that as the mininum entropy option
                    int count = grid[x, y, z].potentialTiles.Count;
                    if (count < minCount || (count == minCount && Random.Range(0, 2) == 0))
                    {
                        minCount = count;
                        minCoords = new Vector3Int(x, y, z);
                    }
                }
            }
        }
        return minCoords;
    }

    private void CollapseAt(Vector3Int coords)
    {
        //get a random tile from list of possible tiles and mark grid point as collapsed
        TileState state = grid[coords.x, coords.y, coords.z];
        state.currentTile = GetRandomTile(state.potentialTiles);

        state.potentialTiles.Clear();
        state.potentialTiles.Add(state.currentTile);
        state.collapsed = true;
    }

    private WFCTile GetWeightedRandomTile(List<WFCTile> potentialTiles)
    {
        //fallback
        if (potentialTiles == null || potentialTiles.Count == 0)
        {
            return fallBackTile;
        }

        //get total weight for a valid random weight value
        int totalWeight = 0;
        foreach (WFCTile tile in potentialTiles)
        {
            totalWeight += tile.weight;
        }

        //then, basically roulette wheel it until there's a tile that pushes the total over the random weight, select that one
        int randomWeight = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;
        foreach (WFCTile tile in potentialTiles)
        {
            cumulativeWeight += tile.weight;
            if (randomWeight < cumulativeWeight)
            {
                return tile;
            }
        }

        return potentialTiles[0];
    }

    private WFCTile GetRandomTile(List<WFCTile> potentialTiles)
    {
        //fallback
        if (potentialTiles == null || potentialTiles.Count == 0)
        {
            return fallBackTile;
        }

        //randomly select tile from list of possibilities
        int randomIndex = Random.Range(0, potentialTiles.Count);
        return potentialTiles[randomIndex];
    }

    private void Propagate(Vector3Int coords)
    {
        //queue starting coordinate
        toCollapse.Enqueue(coords);

        //runs until the entire grid is collapsed
        while (toCollapse.Count > 0)
        {
            //remove current tile so it isn't re-checked
            toCollapse.Dequeue();

            foreach (Vector3Int neighbour in neighbourCoordinates3D)
            {
                Vector3Int neighbourCoords = coords + neighbour;

                if (IsInsideGrid(neighbourCoords))
                {
                    TileState neighbourState = grid[neighbourCoords.x, neighbourCoords.y, neighbourCoords.z];
                    TileState currentState = grid[coords.x, coords.y, coords.z];

                    List<WFCTile> valid = new List<WFCTile>(neighbourState.potentialTiles);

                    //loop through all neighbours to compare potentialTiles' constraints with current tile
                    foreach (WFCTile candidate in valid)
                    {
                        if (!currentState.currentTile.CanConnect(candidate, neighbour))
                        {
                            //remove potential tile from neighbouring list if invalid with current tile
                            neighbourState.potentialTiles.Remove(candidate);

                            //if neighbour isn't in the list to collapse, add it
                            if (!toCollapse.Contains(neighbourCoords))
                            {
                                toCollapse.Enqueue(neighbourCoords);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Pathfinding
    private void InitializeGridForPath()
    {
        //build grid, fill it with data structures that determine whether the grid point is collapsed
        grid = new TileState[gridWidth, gridHeight, gridDepth];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    //only give the potential tiles list non-pathed tiles, since we are building the path separately
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

    private bool IsFaceNeighbour(Vector3Int dir)
    {
        //check if dir is a face-adjacent direction (1 unit away in 1 axis) -> for multi-paths
        //e.g. if end point for path 1 = 9,0,9, and path 2 starts at 1,0,0, the resulting 'dir' is > 1 unit
        return dir != Vector3Int.zero && Mathf.Abs(dir.x) + Mathf.Abs(dir.y) + Mathf.Abs(dir.z) == 1;
    }

    private void AddConnections(List<Vector3Int> path, Dictionary<Vector3Int, HashSet<Vector3Int>> multiPathMap)
    {
        //go through path points, add them to dictionary with its corresponding directions that have connections
        for (int i = 0; i < path.Count; i++)
        {
            Vector3Int current = path[i];

            //create a hashset for current point's connection directions if the dictionary doesn't have one -> "set"
            //set is the name of the hashset in the dictionary. this way, if tiles are overlapping, set gets points added to it; otherwise, set is created.
            if (!multiPathMap.TryGetValue(current, out var set))
            {
                set = new HashSet<Vector3Int>();
                multiPathMap[current] = set;
            }

            //previous tile; IsFaceNeighbour makes sure they connect
            //i.e. don't add the big gap between start/end points of each path if it isn't connected
            if (i > 0)
            {
                Vector3Int inDir = path[i - 1] - current;
                if (IsFaceNeighbour(inDir))
                {
                    set.Add(inDir);
                }
            }

            //next tile
            if (i < path.Count - 1)
            {
                Vector3Int outDir = path[i + 1] - current;
                if (IsFaceNeighbour(outDir))
                {
                    set.Add(outDir);
                }
            }
        }
    }

    private Dictionary<Vector3Int, HashSet<Vector3Int>> GeneratePaths()
    {
        //dictionary of grid point + connections (hashset so there's no dupes)
        Dictionary<Vector3Int, HashSet<Vector3Int>> connectors = new Dictionary<Vector3Int, HashSet<Vector3Int>>();

        //loop through every path specified in the editor, add the connections to the dictionary
        foreach (PathProperties path in paths)
        {
            //call path initializer to get path length
            path.Generate(gridWidth, gridDepth);

            //call pathfinding algorithm for each path
            List<Vector3Int> currentPath = TryCreatePath(path.start, path.end, path.useShortestPath);

            if (currentPath == null || currentPath.Count == 0)
                continue;

            AddConnections(currentPath, connectors);
        }

        return connectors;
    }

    private List<Vector3Int> TryCreatePath(Vector3Int start, Vector3Int end, bool useShortestPath = false)
    {
        if(useShortestPath)
        {
            return BFSShortestPath(start, end);
        }
        else
        {
            return DFSWithRandomization(start, end);
        }
    }

    private List<Vector3Int> GetNeighboursTowards(Vector3Int current, Vector3Int end)
    {
        //orders neighbours based on how close they are to the end tile
        return neighbourCoordinates2D.OrderBy(x => (current + x - end).sqrMagnitude).ToList();
    }
    private static List<Vector3Int> Shuffle(List<Vector3Int> source)
    {
        //uses System.Random to order the list randomly
        return source.OrderBy(x => rng.Next()).ToList();
    }

    private List<Vector3Int> DFSWithRandomization(Vector3Int start, Vector3Int end)
    {
        //Depth-First Search uses a stack for LIFO checking
        //it explores the entire branch of possibilities in a given direction before returning to the next (once a neighbour adds its neighbour, that is checked next)
        Stack<Vector3Int> open = new Stack<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        //BFS = Enqueue, DFS = Push
        open.Push(start);
        cameFrom[start] = start;

        while (open.Count > 0)
        {
            //BFS = Dequeue, DFS = Pop
            Vector3Int current = open.Pop();
            if (current == end) break;

            //shuffle neighbours for randomness, otherwise it'll follow the same path every time
            List<Vector3Int> neighbours = GetNeighboursTowards(current, end);
            neighbours = Shuffle(neighbours);

            foreach (var direction in neighbours)
            {
                Vector3Int newDir = current + direction;

                //skip if neighbour is out of bounds, above ground level or we've already checked it
                if (!IsInsideGrid(newDir) || newDir.y != 0 || cameFrom.ContainsKey(newDir)) 
                    continue;

                //check all 4 directions, but, if it is already in cameFrom dictionary, skip it; each tile is only visited once
                //i.e. this saves the TILE and corresponding DIRECTION
                //BFS = Enqueue, DFS = Push
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

    private List<Vector3Int> BFSShortestPath(Vector3Int start, Vector3Int end)
    {
        //BFS: same as DFS but uses a queue -> shortest path possible
        //explores the closest grid positions first by using FIFO (all neighbours for starting point are checked before moving to the next)
        Queue<Vector3Int> open = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        open.Enqueue(start);
        cameFrom[start] = start;

        while (open.Count > 0)
        {
            Vector3Int current = open.Dequeue();
            if (current == end) break;

            //don't shuffle neighbours for BFS
            List<Vector3Int> neighbours = GetNeighboursTowards(current, end);

            foreach (var direction in neighbours)
            {
                Vector3Int newDir = current + direction;
                if (!IsInsideGrid(newDir) || newDir.y != 0 || cameFrom.ContainsKey(newDir))
                    continue;
                open.Enqueue(newDir);
                cameFrom[newDir] = current;
            }
        }

        if (!cameFrom.ContainsKey(end))
            return null;

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

    private void CollapsePathTiles(Dictionary<Vector3Int, HashSet<Vector3Int>> connectionsAtPoint)
    {
        foreach (var gridPoint in connectionsAtPoint)
        {
            //get current grid point and its connection directions from dictionary
            Vector3Int currentPoint = gridPoint.Key;
            HashSet<Vector3Int> requiredConnectors = gridPoint.Value;

            List<WFCTile> pool;

            switch (requiredConnectors.Count)
            {
                //1 connection = path end
                case 1: 
                    pool = pathEndTiles; 
                    break;

                //2 connections = straight/corner
                case 2: 
                    pool = pathTiles; 
                    break;

                //3+ connections = crossroads
                default: 
                    pool = crossroadTiles; 
                    break; 
            }

            //go through tiles in the pool and prune the ones that have extra + misaligned connections based on required connection hashset
            WFCTile chosen = PickBestTile(pool, requiredConnectors);

            //manual collapse + propagation
            TileState state = grid[currentPoint.x, currentPoint.y, currentPoint.z];
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

        //if there's more than 1 'best fit' tile, use rng to choose
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
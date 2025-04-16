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
    [SerializeField] private List<WFCTile> airTiles;
    [SerializeField] private WFCTile emptyTile;
    [SerializeField] private WFCTile fallBackTile;

    private List<Vector3Int> toCollapse = new List<Vector3Int>();

    private Vector3Int[] neighbourCoordinates = new Vector3Int[]
    {
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
    };

    private void Start()
    {
        InitializeGrid();
        WFC();
    }

    private void InitializeGrid()
    {
        grid = new TileState[gridWidth, gridHeight, gridDepth];

        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                for (int z = 0; z < gridDepth; z++)
                {
                    grid[x, y, z] = new TileState
                    {
                        potentialTiles = new List<WFCTile>(groundTiles),
                        currentTile = null,
                        collapsed = false
                    };

                    if (y > 0)
                        grid[x, y, z].potentialTiles.AddRange(airTiles);

                    grid[x, y, z].potentialTiles.Add(emptyTile);
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
            Debug.LogError($"[CollapseAt] No valid Tiles at {coords}. Using fallback Tile.");
            state.currentTile = fallBackTile;
        }
        else
        {
            state.currentTile = GetWeightedRandomTile(state.potentialTiles);
        }

        state.potentialTiles.Clear();
        state.potentialTiles.Add(state.currentTile);
        state.collapsed = true;
    }

    private WFCTile GetWeightedRandomTile(List<WFCTile> potentialTiles)
    {
        if (potentialTiles == null || potentialTiles.Count == 0)
        {
            Debug.LogError("[GetWeightedRandomTile] potentialTiles list is null or empty. Returning null.");
            return fallBackTile;
        }

        return fallBackTile;
        /*int totalWeight = 0;
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

        return potentialTiles[0];*/
    }

    private void Propagate(Vector3Int coords)
    {
        toCollapse.Add(coords);

        while (toCollapse.Count > 0)
        {
            Vector3Int current = toCollapse[0];
            toCollapse.RemoveAt(0);

            foreach (Vector3Int neighbourOffset in neighbourCoordinates)
            {
                Vector3Int neighbourCoords = current + neighbourOffset;

                if (!IsInsideGrid(neighbourCoords))
                    continue;

                var neighbourState = grid[neighbourCoords.x, neighbourCoords.y, neighbourCoords.z];

                if (neighbourState.collapsed)
                    continue;

                List<WFCTile> valid = new List<WFCTile>(neighbourState.potentialTiles);

                foreach (var candidate in valid)
                {
                    if (!grid[current.x, current.y, current.z].currentTile.CanConnect(candidate, neighbourOffset))
                    {
                        neighbourState.potentialTiles.Remove(candidate);
                        if (!toCollapse.Contains(neighbourCoords))
                            toCollapse.Add(neighbourCoords);
                    }
                }
            }
        }
    }

    private bool IsInsideGrid(Vector3Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth &&
               gridPos.y >= 0 && gridPos.y < gridHeight &&
               gridPos.z >= 0 && gridPos.z < gridDepth;
    }

    private void Visualize()
    {
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                for (int z = 0; z < gridDepth; z++)
                    Instantiate(grid[x, y, z].currentTile.prefab, new Vector3(x, y, z), Quaternion.identity);
    }
}
public class TileState
{
    public List<WFCTile> potentialTiles;
    public WFCTile currentTile;
    public bool collapsed;
}

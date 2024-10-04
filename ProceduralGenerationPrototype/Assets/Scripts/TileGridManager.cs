using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TileGridManager : MonoBehaviour
{
    [SerializeField] Tile[] tiles;
    [SerializeField] int gridWidth = 5; //width of grid
    [SerializeField] int gridLength = 5; //length of grid
    Tile[,] grid;
    List<Tile>[,] possibleTiles; //list of possible tiles for each position in the grid

    private void Start()
    {
        CreateGrid();
        WaveFunctionCollapse();
    }

    void CreateGrid()
    {
        grid = new Tile[gridWidth, gridLength];
        possibleTiles = new List<Tile>[gridWidth, gridLength];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridLength; y++)
            {
                possibleTiles[x, y] = new List<Tile>(tiles);
                int choose = Random.Range(0, possibleTiles[x,y].Count);
                Tile newTile = possibleTiles[x, y][choose]; //Instantiate(possibleTiles[x, y][choose], position, Quaternion.identity); //fill grid with random pieces to start
                newTile.name = $"Tile {x}_{y}"; //name the tiles so it's not all tilename(clone), less confusing
                newTile.InitializeContactTypes();
                grid[x, y] = newTile;
            }
        }

    }

    void WaveFunctionCollapse()
    {
        //need to figure out how to add logic for when things don't go together
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridLength; y++)
            {
                List<Tile> possibleTilesForCell = GetPossibleTiles(x, y);

                int choose = Random.Range(0, possibleTilesForCell.Count);
                Vector3 position = new Vector3(x, 0, y);

                Tile chosenTile = Instantiate(possibleTilesForCell[choose], position, Quaternion.identity);
                chosenTile.name = $"Tile {x}_{y}";
                grid[x, y] = chosenTile;
            }
        }
    }

    void DestroyGrid()
    {
        if (grid != null)
        {
            foreach (Tile tile in grid)
            {
                DestroyImmediate(tile.gameObject);
            }
        }
    }

    List<Tile> GetPossibleTiles(int x, int y)
    {
        HashSet<Tile> tilesToRemove = new HashSet<Tile>();

        //Debug.Log($"Checking possible tiles at ({x}, {y}), initial count: {possibleTiles[x, y].Count}");

        foreach (Tile currentTile in possibleTiles[x, y])
        {
            bool valid = true;

            // Check left tile
            if (x > 1 && grid[x - 1, y] != null)
            {
                Tile leftTile = grid[x - 1, y];

                if (!currentTile.IsValidContact(leftTile, Vector2.left)) 
                {
                    valid = false;
                    //Debug.Log($"Tile at ({x}, {y}) is invalid due to left neighbor at ({x - 1}, {y})");
                }
            }

            // Check bottom tile
            if (y > 1 && grid[x, y - 1] != null)
            {
                Tile belowTile = grid[x, y - 1];

                if (!currentTile.IsValidContact(belowTile, Vector2.down))
                {
                    valid = false;
                    //Debug.Log($"Tile at ({x}, {y}) is invalid due to below neighbor at ({x}, {y - 1})");
                }
            }

            // Check right tile
            if (x < gridWidth - 1 && grid[x + 1, y] != null)
            {
                Tile rightTile = grid[x + 1, y];

                if (!currentTile.IsValidContact(rightTile, Vector2.right))
                {
                    valid = false;
                    //Debug.Log($"Tile at ({x}, {y}) is invalid due to right neighbor at ({x + 1}, {y})");
                }
            }

            // Check above tile
            if (y < gridLength - 1 && grid[x, y + 1] != null)
            {
                Tile aboveTile = grid[x, y + 1];

                if (!currentTile.IsValidContact(aboveTile, Vector2.up))
                {
                    valid = false;
                    //Debug.Log($"Tile at ({x}, {y}) is invalid due to above neighbor at ({x}, {y + 1})");
                }
            }

            // If the tile is invalid at any point, add it to the list of tiles to be removed
            if (!valid)
            {
                //tilesToRemove.Add(currentTile);
                //Debug.Log($"Removing tile {currentTile.name} at ({x}, {y}) due to invalid contact.");
            }
        }

        // Remove all invalid tiles after every tile is checked for each grid piece
        foreach (Tile tile in tilesToRemove)
        {
            possibleTiles[x, y].Remove(tile);
        }

        //Debug.Log($"Remaining possibilities at ({x}, {y}): {possibleTiles[x, y].Count}");

        return possibleTiles[x, y];
    }



    public void Regenerate()
    {
        DestroyGrid();
        CreateGrid();
        WaveFunctionCollapse();
    }
}

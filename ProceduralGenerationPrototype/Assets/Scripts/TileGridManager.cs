using System.Collections.Generic;
using UnityEngine;

public class TileGridManager : MonoBehaviour
{
    [SerializeField] Tile[] tiles;
    [SerializeField] int gridWidth = 5; //width of grid
    [SerializeField] int gridLength = 5; //length of grid
    Tile[,] grid;
    List<Tile>[,] possibleTiles; //list of possible tiles for each position in the grid
    List<GameObject> currentTiles;

    private void Start()
    {
        CreateGrid();
        WaveFunctionCollapse();
    }

    void CreateGrid()
    {
        grid = new Tile[gridWidth, gridLength];
        possibleTiles = new List<Tile>[gridWidth, gridLength];
        currentTiles = new List<GameObject>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridLength; y++)
            {
                possibleTiles[x, y] = new List<Tile>(tiles);
                int choose = Random.Range(0, possibleTiles[x,y].Count);
                Tile newTile = possibleTiles[x, y][choose]; //Instantiate(possibleTiles[x, y][choose], position, Quaternion.identity); //fill grid with random pieces to start
                newTile.name = $"Tile {x}_{y}"; //name the tiles so it's not all tilename(clone), less confusing
                grid[x, y] = newTile;
            }
        }

        foreach(Tile tile in grid)
        {
            if (tile != null)
            {
                currentTiles.Add(tile.gameObject);
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
                Destroy(tile.gameObject);
            }
        }
    }

    List<Tile> GetPossibleTiles(int x, int y)
    {
        HashSet<Tile> tilesToRemove = new HashSet<Tile>();
        Debug.Log($"Checking tile at ({x}, {y}), initial possibilities: {possibleTiles[x,y].Count}");

        foreach (Tile currentTile in possibleTiles[x, y])
        {
            bool valid = true;

            //check left tile
            if (x > 0 && grid[x - 1, y] != null)
            {
                Tile leftTile = grid[x - 1, y];

                if (!currentTile.IsValidContact(leftTile.GetContactType(), Vector2.left))
                {
                    valid = false;
                    Debug.Log($"Tile {currentTile.GetContactType()} at ({x}, {y}) removed because of left neighbor at ({x - 1}, {y})");
                }
            }

            //check bottom tile
            if (y > 0 && grid[x, y - 1] != null)
            {
                Tile belowTile = grid[x, y - 1];

                if (!currentTile.IsValidContact(belowTile.GetContactType(), Vector2.down))
                {
                    valid = false;
                    Debug.Log($"Tile {currentTile.GetContactType()} at ({x}, {y}) removed because of bottom neighbor at ({x}, {y - 1})");

                }
            }

            //check right tile
            if (x < gridWidth - 1 && grid[x + 1, y] != null)
            {
                Tile rightTile = grid[x + 1, y];

                if (!currentTile.IsValidContact(rightTile.GetContactType(), Vector2.right))
                {
                    valid = false;
                    Debug.Log($"Tile {currentTile.GetContactType()} at ({x}, {y}) removed because of right neighbor at ({x + 1}, {y})");
                }
            }

            //check above tile
            if (y < gridLength - 1 && grid[x, y + 1] != null)
            {
                Tile aboveTile = grid[x, y + 1];

                if (!currentTile.IsValidContact(aboveTile.GetContactType(), Vector2.up))
                {
                    valid = false;
                    Debug.Log($"Tile  {currentTile.GetContactType()}  at ({x}, {y}) removed because of above neighbor at ({x}, {y + 1})");
                }
            }

            //if the tile is invalid at any point, add it to the list of tiles to be removed
            if (!valid)
            {
                tilesToRemove.Add(currentTile);
            }
        }

        //remove all invalid tiles after every tile is checked for each grid piece
        foreach (Tile tile in tilesToRemove)
        {
            possibleTiles[x, y].Remove(tile);
        }

         Debug.Log($"Tile at ({x}, {y}) remaining possibilities: {possibleTiles[x, y].Count}");

        return possibleTiles[x, y];
    }


    public void Regenerate()
    {
        DestroyGrid();
        CreateGrid();
        WaveFunctionCollapse();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TileGridManager : MonoBehaviour
{
    [SerializeField] Tile[] tiles;
    [SerializeField] int gridWidth = 5; //width of grid
    [SerializeField] int gridLength = 5; //length of grid
    private Tile[,] grid;
    List<GameObject> currentTiles;

    private void Start()
    {
        currentTiles = new List<GameObject>();
        CreateGrid();
        WaveFunctionCollapse();
    }

    void CreateGrid()
    {
        grid = new Tile[gridWidth, gridLength];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridLength; y++)
            {
                Vector3 position = new Vector3(x, 0, y);
                Tile newTile = Instantiate(tiles[0], position, Quaternion.identity); //fill grid with floors to begin with
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
                Tile currentTile = grid[x, y];

                //check left tile
                if (x > 0)
                {
                    Tile leftTile = grid[x - 1, y];

                    if (!currentTile.IsValidContact(leftTile.GetContactType(), Vector2.right))
                    {
                        
                    }
                }

                //check bottom tile
                if (y > 0)
                {
                    Tile belowTile = grid[x, y - 1];

                    if (!currentTile.IsValidContact(belowTile.GetContactType(), Vector2.up))
                    {
                        
                    }
                }

                //check right tile
                if (x < gridWidth - 1) 
                {
                    Tile rightTile = grid[x + 1, y];

                    if (!currentTile.IsValidContact(rightTile.GetContactType(), Vector2.left))
                    {

                    }
                }

                //check above tile
                if (y < gridLength - 1)
                {
                    Tile aboveTile = grid[x, y + 1];

                    if (!currentTile.IsValidContact(aboveTile.GetContactType(), Vector2.down))
                    {

                    }
                }
            }
        }
    }

    void DestroyGrid()
    {
        if (currentTiles.Count > 0)
        {
            foreach (GameObject tile in currentTiles)
            {
                Destroy(tile);
            }
        }
    }

    public void Regenerate()
    {
        DestroyGrid();
        CreateGrid();
        WaveFunctionCollapse();
    }
}

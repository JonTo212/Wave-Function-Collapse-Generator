using System.Collections.Generic;
using UnityEngine;
public enum TileType
{
    Floor,
    Wall,
    Corner,
    Door
}

public class WaveFunctionCollapse : MonoBehaviour
{
    public int width = 10; //width of grid
    public int length = 10; //length of grid
    public TileType[,] grid; //2d array (e.g. (x,y) array)
    //public TileConstraints constraints = new TileConstraints(); //constraints
    public GameObject[] tilePrefabs; //actual tile objects to be instantiated
    bool regenerate;
    GameObject[] tiles;

    void Start()
    {
        tiles = new GameObject[width * length];
        Regenerate();
    }

    private void Update()
    {
        if(regenerate)
        {
            Regenerate();
        }
    }
    List<TileType> GetPossibleTiles(int x, int z) // Tile tile)
    {
        List<TileType> possibleTiles = new List<TileType>(); //creates a list of tile types that will be filled with the possible tiles that can be instantiated in each grid slot

        /*foreach (TileType tileType in tile.constraints) //for each tile type in the enum
        {
            bool isValid = true;

            if (x > 0 && grid[x - 1, z] != tileType) //checks the Constraints dictionary list to make sure that tiletype isn't in it
            {
                isValid = false; //checks left neighbouring tile to see if the current tile type is valid
            }

            if (x > 0 && grid[x + 1, z] != tileType)
            {
                isValid = false; //checks right neighbouring tile to see if the current tile type is valid
            }

            if (z > 0 && grid[x, z - 1] != tileType)
            {
                isValid = false; //checks bottom neighbouring tile to see if current tile type is valid
            }

            if (z > 0 && grid[x, z + 1] != tileType)
            {
                isValid = false; //checks upper neighbouring tile to see if current tile type is valid
            }

            if (isValid)
            {
                possibleTiles.Add(tileType); //adds possible tile types to the list of potential tiles for slot (x,z)
            }
        }*/

        /* (TileType tileType in System.Enum.GetValues(typeof(TileType))) //for each tile type in the enum
        {
            bool isValid = true;

            if (x > 0 && !constraints.Constraints[tileType].Contains(grid[x - 1, z])) //checks the Constraints dictionary list to make sure that tiletype isn't in it
            {
                isValid = false; //checks left neighbouring tile to see if the current tile type is valid
            }

            if (x > 0 && !constraints.Constraints[tileType].Contains(grid[x + 1, z]))
            {
                isValid = false; //checks right neighbouring tile to see if the current tile type is valid
            }

            if (z > 0 && !constraints.Constraints[tileType].Contains(grid[x, z - 1]))
            {
                isValid = false; //checks bottom neighbouring tile to see if current tile type is valid
            }

            if (z > 0 && !constraints.Constraints[tileType].Contains(grid[x, z + 1]))
            {
                isValid = false; //checks upper neighbouring tile to see if current tile type is valid
            }

            if (isValid)
            {
                possibleTiles.Add(tileType); //adds possible tile types to the list of potential tiles for slot (x,z)
            }
        }*/

        return possibleTiles;
    }

    Quaternion GetEdgeRotations(int x, int z) //sets up box on edges of grid
    {
        TileType tile = grid[x, z];
        Quaternion rotation = Quaternion.identity;

        if (tile == TileType.Wall)
        {
            if (z == length - 1) //bottom edge
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (x == 0) //left edge
            {
                rotation = Quaternion.Euler(0, 270, 0);
            }
            else if (x == width - 1) //right edge
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (z == 0) //top edge
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
        }

        else if (tile == TileType.Corner)
        {
            if (x == width - 1 && z == length - 1)
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (x == width - 1 && z == 0)
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (x == 0 && z == 0)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (x == 0 && z == length - 1)
            {
                rotation = Quaternion.Euler(0, 270, 0);
            }
        }

        return rotation;
    }

    Quaternion GetRotationForTile(int x, int z)
    {
        TileType tile = grid[x, z];
        Quaternion rotation = Quaternion.identity;

        if (tile == TileType.Wall)
        {
            bool hasWallTop = z > 0 && grid[x, z - 1] == TileType.Wall;
            bool hasWallRight = x < width - 1 && grid[x + 1, z] == TileType.Wall;

            if (!hasWallTop && !hasWallRight)
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (hasWallTop && hasWallRight)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (hasWallTop && !hasWallRight)
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (!hasWallTop && hasWallRight)
            {
                rotation = Quaternion.Euler(0, 270, 0);
            }
        }
        else if (tile == TileType.Corner)
        {
            bool hasWallTop = z > 0 && grid[x, z - 1] == TileType.Wall;
            bool hasWallRight = x < width - 1 && grid[x + 1, z] == TileType.Wall;

            if (!hasWallTop && !hasWallRight)
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (hasWallTop && hasWallRight)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (hasWallTop && !hasWallRight)
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (!hasWallTop && hasWallRight)
            {
                rotation = Quaternion.Euler(0, 270, 0);
            }
        }
        else if (tile == TileType.Door)
        {
            bool hasWallTop = z > 0 && grid[x, z - 1] == TileType.Wall;
            bool hasWallRight = x < width - 1 && grid[x + 1, z] == TileType.Wall;

            if (!hasWallTop && !hasWallRight)
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (hasWallTop && hasWallRight)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (hasWallTop && !hasWallRight)
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (!hasWallTop && hasWallRight)
            {
                rotation = Quaternion.Euler(0, 270, 0);
            }
        }

        return rotation;
    }


    public void Regenerate()
    {
        foreach (GameObject tile in tiles)
        {
            Destroy(tile);
        }

        grid = new TileType[width, length];
        GenerateGrid();
        VisualizeGrid();
        regenerate = false;
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                grid[x, z] = TileType.Floor; //base grid is all floors
            }
        }

        //set edges to be walls
        for (int x = 0; x < width; x++)
        {
            if (x == 0 || x == width - 1)
            {
                grid[x, 0] = TileType.Corner;
                grid[x, length - 1] = TileType.Corner;
            }
            else
            {
                grid[x, 0] = TileType.Wall; //bottom edge
                grid[x, length - 1] = TileType.Wall; //top edge
            }
        }

        for (int z = 0; z < length; z++)
        {
            if (z == 0 || z == width - 1)
            {
                grid[0, z] = TileType.Corner;
                grid[width - 1, z] = TileType.Corner;
            }
            else
            {
                grid[0, z] = TileType.Wall; //left edge
                grid[width - 1, z] = TileType.Wall; //right edge
            }
        }

        Collapse();
    }

    void Collapse()
    {
        for (int x = 1; x < width - 1; x++) //start at 1 and end at width - 1 because edges are already pre-set
        {
            for (int z = 1; z < length - 1; z++)
            {
                List<TileType> possibleTiles = GetPossibleTiles(x, z); //tilePrefabs[(int)grid[x, z]].GetComponent<Tile>());

                if (possibleTiles.Count > 0)
                {
                    grid[x, z] = possibleTiles[Random.Range(0, possibleTiles.Count)]; //randomly select from possible tiles, fill grid with tiles
                }
            }
        }
    }

    void VisualizeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                GameObject instantiatedTile = tilePrefabs[(int)grid[x, z]]; //get the corresponding tile at each grid slot
                Quaternion rotation = Quaternion.identity;

                if (x == 0 || x == width - 1 || z == 0 || z == length - 1)
                {
                    rotation = GetEdgeRotations(x, z);
                }
                else
                {
                    rotation = GetRotationForTile(x, z);
                }

                int index = x + z * length; //convert 2d array (x,z) to 1d array 

                tiles[index] = Instantiate(instantiatedTile, new Vector3(x, 0, z), rotation); //instantiate tile prefabs with rotation
            }
        }
    }


}

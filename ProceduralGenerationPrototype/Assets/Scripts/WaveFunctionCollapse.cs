using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Floor,
    Wall,
    Corner,
    Door
}

public class TileConstraints
{
    public Dictionary<TileType, List<TileType>> Constraints = new Dictionary<TileType, List<TileType>>();

    public TileConstraints()
    {
        Constraints[TileType.Floor] = new List<TileType> { TileType.Wall, TileType.Floor, TileType.Corner }; //floors can be beside anything
        Constraints[TileType.Wall] = new List<TileType> { TileType.Floor, TileType.Wall }; //walls can only be beside floors and walls
        Constraints[TileType.Corner] = new List<TileType> { TileType.Wall }; //corners can only be beside walls
        Constraints[TileType.Door] = new List<TileType> { TileType.Wall, TileType.Corner }; //corners can only be beside walls
    }
}

public class WaveFunctionCollapse : MonoBehaviour
{
    public int width = 10; //width of grid
    public int length = 10; //length of grid
    public TileType[,] grid; //2d array (e.g. (x,y) array)
    public TileConstraints constraints = new TileConstraints(); //Checks constraints
    public GameObject[] tilePrefabs; //actual tile objects to be instantiated

    void Start()
    {
        grid = new TileType[width, length];
        GenerateGrid();
        VisualizeGrid();
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
            if (x == 0 ||x == width - 1)
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
                grid[0, z] = TileType.Wall; // Left edge
                grid[width - 1, z] = TileType.Wall; // Right edge
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
                List<TileType> possibleTiles = GetPossibleTiles(x, z);
                if (possibleTiles.Count > 0)
                {
                    grid[x, z] = possibleTiles[Random.Range(0, possibleTiles.Count)]; //randomly select from possible tiles, fill grid with tiles
                }
            }
        }
    }

    List<TileType> GetPossibleTiles(int x, int z) //feed the x and z value of grid
    {
        List<TileType> possibleTiles = new List<TileType>(); //creates a list of tile types that will be filled with the possible tiles that can be instantiated in each grid slot

        foreach (TileType tileType in System.Enum.GetValues(typeof(TileType))) //for each tile type
        {
            bool isValid = true; 
            if (x > 0 && !constraints.Constraints[tileType].Contains(grid[x - 1, z])) isValid = false; //checks left neighbouring tile to see if the current tile type is valid
            if (z > 0 && !constraints.Constraints[tileType].Contains(grid[x, z - 1])) isValid = false; //checks bottom neighbouring tile to see if current tile type is valid

            if (isValid)
            {
                possibleTiles.Add(tileType); //adds possible tile types to the list of potential tiles for slot (x,z)
            }
        }

        return possibleTiles;
    }

    void VisualizeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                GameObject instantiatedTile = tilePrefabs[(int)grid[x, z]]; //get the corresponding tile at each grid slot
                Quaternion rotation = GetRotationForTile(x, z); //get rotation for walls and corners
                Instantiate(instantiatedTile, new Vector3(x, 0, z), rotation); //instantiate tile prefabs with rotation
            }
        }
    }

    Quaternion GetRotationForTile(int x, int z)
    {
        TileType tile = grid[x, z];
        Quaternion rotation = Quaternion.identity;

        if (tile == TileType.Wall)
        {
            if (x == 0) //left edge
            {
                rotation = Quaternion.Euler(0, 270, 0);
            }
            else if (x == width - 1) //right edge
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else if(z == 0) //top edge
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (z == length - 1) //bottom edge
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                if (x > 0 && grid[x - 1, z] == TileType.Floor) //if there's a floor to the left, rotate the wall to be vertical, otherwise leave it horizontal
                {
                    rotation = Quaternion.Euler(0, 90, 0);
                }
                else
                {
                    rotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

        else if (tile == TileType.Corner)
        {
            if (x == 0 && z == 0) //bottom left corner
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (x == width - 1 && z == 0) //bottom right corner
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (x == 0 && z == length - 1) //top left corner
            {
                rotation = Quaternion.Euler(0, 270, 0);
            }
            else if (x == width - 1 && z == length - 1) //top right corner
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                bool hasWallOnLeft = (x > 0 && grid[x - 1, z] == TileType.Wall);
                bool hasWallOnBottom = (z > 0 && grid[x, z - 1] == TileType.Wall);

                if (hasWallOnLeft && hasWallOnBottom)
                {
                    rotation = Quaternion.Euler(0, 0, 0);
                }
                else if (hasWallOnLeft)
                {
                    rotation = Quaternion.Euler(0, 90, 0); //rotate 90 degrees if there's a wall on the left
                }
                else if (hasWallOnBottom)
                {
                    rotation = Quaternion.Euler(0, 180, 0); //rotate 180 degrees if there's a wall on the bottom
                }
            }
        }

        return rotation;
    }
}

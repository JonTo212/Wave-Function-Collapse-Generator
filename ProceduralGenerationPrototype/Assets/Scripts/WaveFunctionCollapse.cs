using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Floor,
    Wall,
    Corner
    // Add more tile types as needed
}

public class TileConstraints
{
    public Dictionary<TileType, List<TileType>> Constraints = new Dictionary<TileType, List<TileType>>();

    public TileConstraints()
    {
        // Example constraints
        Constraints[TileType.Floor] = new List<TileType> { TileType.Wall, TileType.Floor, TileType.Corner };
        Constraints[TileType.Wall] = new List<TileType> { TileType.Corner, TileType.Floor }; // Walls should only be next to walls
        Constraints[TileType.Corner] = new List<TileType> { TileType.Wall, TileType.Corner };
    }
}

public class WaveFunctionCollapse : MonoBehaviour
{
    public int width = 10;
    public int length = 10;
    public TileType[,] grid;
    public TileConstraints constraints = new TileConstraints();
    public GameObject[] tilePrefabs; // Assign prefabs for each tile type in Unity Inspector

    void Start()
    {
        grid = new TileType[width, length];
        GenerateGrid();
        VisualizeGrid();
    }

    void GenerateGrid()
    {
        // Initialize the grid with empty tiles
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                grid[x, z] = TileType.Floor;
            }
        }

        // Perform WFC algorithm
        Collapse();
    }

    void Collapse()
    {
        // Simplified collapse algorithm: randomly assign tiles while respecting constraints
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                List<TileType> possibleTiles = GetPossibleTiles(x, z);
                if (possibleTiles.Count > 0)
                {
                    grid[x, z] = possibleTiles[Random.Range(0, possibleTiles.Count)];
                }
            }
        }
    }

    List<TileType> GetPossibleTiles(int x, int y)
    {
        List<TileType> possibleTiles = new List<TileType>();

        // Check constraints with neighboring tiles
        foreach (TileType tileType in System.Enum.GetValues(typeof(TileType)))
        {
            bool isValid = true;
            if (x > 0 && !constraints.Constraints[tileType].Contains(grid[x - 1, y])) isValid = false; // Left
            if (y > 0 && !constraints.Constraints[tileType].Contains(grid[x, y - 1])) isValid = false; // Down

            if (isValid)
            {
                possibleTiles.Add(tileType);
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
                GameObject tilePrefab = tilePrefabs[(int)grid[x, z]];
                Instantiate(tilePrefab, new Vector3(x, 0, z), Quaternion.identity);
            }
        }
    }
}

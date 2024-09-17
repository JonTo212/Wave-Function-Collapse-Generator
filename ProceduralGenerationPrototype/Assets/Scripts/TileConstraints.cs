using System.Collections.Generic;

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
        Constraints[TileType.Floor] = new List<TileType> { TileType.Wall, TileType.Floor, TileType.Corner, TileType.Door }; //floors can be beside anything
        Constraints[TileType.Wall] = new List<TileType> { TileType.Wall, TileType.Floor, TileType.Corner, TileType.Door }; //walls can be beside anything
        Constraints[TileType.Corner] = new List<TileType> { TileType.Wall, TileType.Floor }; //corners can only be beside walls and floors
        Constraints[TileType.Door] = new List<TileType> { TileType.Wall, TileType.Floor }; //doors can only be beside walls and floors
    }
}
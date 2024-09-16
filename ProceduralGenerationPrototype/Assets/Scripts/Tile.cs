using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Define tile properties (like allowed neighbors)
    public List<Tile> northConnections;
    public List<Tile> eastConnections;
    public List<Tile> southConnections;
    public List<Tile> westConnections;

    // You can expand this class with additional properties like rotation, textures, etc.
}

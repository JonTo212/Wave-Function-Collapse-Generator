using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    /* index 0 = upper constraint
     * index 1 = lower constraint
     * index 2 = left constraint
     * index 3 = right constraint
     */

    public TileType[] constraints; //need to add empty slot
    public int rotation; //don't really know how to use this yet -> +90 rot every time it doesn't fit together, but how do I check if it doesn't fit?

    //public TileType upConstraint;
    //public TileType downConstraint;
    //public TileType leftConstraint;
    //public TileType rightConstraint;
}

using System.Collections.Generic;
using UnityEngine;

public enum Directions
{
    Up, Down, Left, Right
}

[System.Serializable]
public class DirectionalConstraint
{
    public Directions direction; //directional component of incompatibility
    public List<string> incompatibleContactTypes; //incompatible types for corresponding direction

    public Vector2 GetDirection()
    {
        switch (direction)
        {
            case Directions.Up:
                return Vector2.up; // (0, 1)
            case Directions.Down:
                return Vector2.down; // (0, -1)
            case Directions.Left:
                return Vector2.left; // (-1, 0)
            case Directions.Right:
                return Vector2.right; // (1, 0)
            default:
                return Vector2.zero; // Fallback case
        }
    }
}

[System.Serializable]
public class TileConstraint
{
    public string contactType; //what contact type this is
    public List<DirectionalConstraint> directionalConstraints; //directional constraints for each face

    public bool IsValidContact(string otherContactType, Vector2 direction)
    {
        //check whether the provided direction is in the directional constraints
        var constraint = directionalConstraints.Find(c => c.GetDirection() == direction); 

        if (constraint != null)
        {
            //if the direction has constraints + if the provided string is NOT in the list of incompatible types, it is valid.
            return !constraint.incompatibleContactTypes.Contains(otherContactType); 
        }

        //if the direction has no constraints, it's valid
        return true;
    }
}
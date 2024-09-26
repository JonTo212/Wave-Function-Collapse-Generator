using System.Collections.Generic;
using UnityEngine;

public enum Directions
{
    Up, Down, Left, Right
}

[System.Serializable]

/*
 * this class creates a drop down menu using the enum 'directions', each corresponding with a direction
 * for each direction, there is also a list of compatible contacts
 */
public class DirectionalConstraint
{
    public Directions direction; //directional component of incompatibility
    public List<string> compatibleContactTypes; //incompatible types for corresponding direction

    public Vector2 GetDirection()
    {
        switch (direction)
        {
            case Directions.Up:
                return Vector2.up; //(0, 1)
            case Directions.Down:
                return Vector2.down; //(0, -1)
            case Directions.Left:
                return Vector2.left; //(-1, 0)
            case Directions.Right:
                return Vector2.right; //(1, 0)
            default:
                return Vector2.zero; //error case
        }
    }
}

/*
 * this class has a definition for what the current object's type is
 * it also has a reference to the directional constraints for the current object (i.e. direction + compatible string list)
 * finally, it has functions to:
 * -get the current object's type
 * -get the current object's compatible types in a given direction
 * -get whether the type in a given direction is valid or not
 */
public class Tile : MonoBehaviour
{
    [SerializeField] string contactType; //what contact type this is
    [SerializeField] List<DirectionalConstraint> directionalConstraints; //directional constraints for each face


    //returns the current object's contact type
    public string GetContactType()
    {
        return contactType;
    }


    //returns the list of incompatible types by direction
    public List<string> GetCompatibleTypes(Vector2 direction)
    {
        //convert from direction enum to vector2, then search for the corresponding constraint
        var directionalConstraint = directionalConstraints.Find(c => c.GetDirection() == direction);

        //spit out either the list of incompatible types or a new empty list of strings so there's no errors
        return directionalConstraint != null ? directionalConstraint.compatibleContactTypes : new List<string>(); 
    }


    //check whether the contact type in a given direction is usable
    public bool IsValidContact(string otherContactType, Vector2 direction)
    {
        //check whether the provided direction is in the directional constraints
        var constraint = directionalConstraints.Find(c => c.GetDirection() == direction); 

        if (constraint != null)
        {
            //if the direction has constraints + if the provided string is NOT in the list of incompatible types, it is valid.
            return constraint.compatibleContactTypes.Contains(otherContactType); 
        }

        //if the direction has no constraints, it's valid
        return true;
    }
}
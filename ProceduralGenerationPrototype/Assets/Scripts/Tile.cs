using System.Collections.Generic;
using UnityEngine;

public enum Directions
{
    Up, Down, Left, Right
}

[System.Serializable]

/*
 * this class creates a drop down menu using the enum 'directions', each corresponding with a direction
 * for each direction, there is also a list of incompatible contacts
 */
public class DirectionalConstraint
{
    public Directions direction; //directional component of incompatibility
    public List<string> incompatibleContactTypes; //incompatible types for corresponding direction

    public Vector2 GetDirection(Transform tileTransform)
    {
        switch (direction) //now checks the tile's directions (rather than a static vector2.up, vector2.right, etc.)
        {
            case Directions.Up:
                return ProjectToGrid(tileTransform.up); 
            case Directions.Down:
                return ProjectToGrid(-tileTransform.up); 
            case Directions.Left:
                return ProjectToGrid(-tileTransform.right);
            case Directions.Right:
                return ProjectToGrid(tileTransform.right);
            default:
                return Vector2.zero;
        }
    }

    Vector2 ProjectToGrid(Vector3 direction3D)
    {
        //convert from 3D to 2D
        //used to convert transform.direction to a vector2 (e.g. transform.up = (0, 1), but will account for rotations)
        return new Vector2(direction3D.x, direction3D.y).normalized;
    }
}

/*
 * this class has a definition for what the current object's type is
 * it also has a reference to the directional constraints for the current object (i.e. direction + incompatible string list)
 * finally, it has functions to:
 * -get the current object's type
 * -get the current object's incompatible types in a given direction
 * -get whether the type in a given direction is valid or not
 */
public class Tile : MonoBehaviour
{
    [SerializeField] List<string> contactTypes; //what contact type this is
    [SerializeField] List<DirectionalConstraint> directionalConstraints; //directional constraints for each face
    int currentRotation = 0;
    Dictionary<Vector2, string> directionToContactType;

    void Start()
    {
        InitializeContactTypes();
    }

    public void InitializeContactTypes()
    {
        // Using transform directions (dynamic based on rotation) instead of fixed vectors
        directionToContactType = new Dictionary<Vector2, string>
        {
            { ProjectToGrid(transform.up), contactTypes[0] }, //up
            { ProjectToGrid(-transform.up), contactTypes[1] }, //down
            { ProjectToGrid(transform.right), contactTypes[2] }, //right
            { ProjectToGrid(-transform.right), contactTypes[3] } //left
        };
    }

    Vector2 ProjectToGrid(Vector3 direction3D)
    {
        //convert from 3D to 2D
        //used to convert transform.direction to a vector2 (e.g. transform.up = (0, 1), but will account for rotations)
        return new Vector2(direction3D.x, direction3D.y).normalized;
    }


    //returns the current object's contact type
    public string GetContactType(Vector2 direction)
    {
        if (directionToContactType == null)
        {
            InitializeContactTypes();
        }

        return directionToContactType[direction];
    }

    public void Rotate()
    {
        currentRotation = (currentRotation + 1) % 4; //rotate clockwise for 360 degree checking
    }

    //returns the list of incompatible types by direction
    public List<string> GetIncompatibleTypes(Vector2 direction)
    {
        //convert from direction enum to vector2, then search for the corresponding constraint
        var directionalConstraint = directionalConstraints.Find(c => c.GetDirection(transform) == direction);

        //spit out either the list of incompatible types or a new empty list of strings so there's no errors
        return directionalConstraint != null ? directionalConstraint.incompatibleContactTypes : new List<string>(); 
    }

    //check whether the contact type in a given direction is usable
    public bool IsValidContact(Tile otherTile, Vector2 direction)
    {
        //get the contact type of the current tile in the given direction
        string currentTileContactType = GetContactType(direction);
        Debug.Log($"Checking contact type for current tile at {transform.position} in direction {direction}: {currentTileContactType}");

        //get the opposite direction for the neighbouring tile
        Vector2 oppositeDirection = -direction;

        //get the contact type of the other tile in the opposite direction
        string otherTileContactType = otherTile != null ? otherTile.GetContactType(oppositeDirection) : "";
        Debug.Log($"Checking contact type for other tile at {otherTile.transform.position} in direction {oppositeDirection}: {otherTileContactType}");

        //search for a directional constraint for the current tile in the given direction
        var currentTileConstraint = directionalConstraints.Find(c => c.GetDirection(transform) == direction);
        Debug.Log($"Current tile constraint in direction {direction}: {currentTileConstraint != null}");

        //search for a directional constraint for the other tile in the opposite direction, otherwise it's null
        var otherTileConstraint = otherTile != null ? otherTile.directionalConstraints.Find(c => c.GetDirection(otherTile.transform) == oppositeDirection) : null;
        Debug.Log($"Other tile constraint in direction {oppositeDirection}: {otherTileConstraint != null}");

        //check if current tile is valid
        bool currentTileValid = currentTileConstraint == null || !currentTileConstraint.incompatibleContactTypes.Contains(otherTileContactType);

        //check if the other tile is valid
        bool otherTileValid = otherTileConstraint == null || !otherTileConstraint.incompatibleContactTypes.Contains(currentTileContactType);

        //if botha re valid, then this connection can be made
        bool isValidConnection = currentTileValid && otherTileValid;
        Debug.Log($"Connection validity between tiles: {isValidConnection}");

        return isValidConnection;
    }

}
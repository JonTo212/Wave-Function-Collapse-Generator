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
    [SerializeField] Directions direction; //directional component of incompatibility
    public List<string> incompatibleContactTypes; //incompatible types for corresponding direction

    public Vector2 GetDirection(Transform tileTransform)
    {
        switch (direction) //now checks the tile's directions (rather than a static vector2.up, vector2.right, etc.)
        {
            case Directions.Up:
                return ProjectToGrid(tileTransform.forward); 
            case Directions.Down:
                return ProjectToGrid(-tileTransform.forward); 
            case Directions.Left:
                return ProjectToGrid(-tileTransform.right);
            case Directions.Right:
                return ProjectToGrid(tileTransform.right);
            default:
                return Vector2.zero;
        }
    }

    private Vector2 ProjectToGrid(Vector3 direction3D)
    {
        //convert from 3D to 2D
        //used to convert transform.direction to a vector2 (e.g. transform.up = (0, 1), but will account for rotations)
        return new Vector2(direction3D.x, direction3D.z);
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
    [SerializeField] private List<string> contactTypes; //what contact type this is
    [SerializeField] private List<DirectionalConstraint> directionalConstraints; //directional constraints for each face
    private int currentRotation = 0;
    private Dictionary<Vector2, string> directionToContactType;

    private void Start()
    {
        InitializeContactTypes();
    }

    public void InitializeContactTypes()
    {
        // Using transform directions (dynamic based on rotation) instead of fixed vectors
        directionToContactType = new Dictionary<Vector2, string>
        {
            { ProjectToGrid(transform.forward), contactTypes[0] }, //up
            { ProjectToGrid(-transform.forward), contactTypes[1] }, //down
            { ProjectToGrid(transform.right), contactTypes[2] }, //right
            { ProjectToGrid(-transform.right), contactTypes[3] } //left
        };
    }

    private Vector2 ProjectToGrid(Vector3 direction3D)
    {
        //convert from 3D to 2D
        //used to convert transform.direction to a vector2 (e.g. transform.up = (0, 1), but will account for rotations)
        return new Vector2(direction3D.x, direction3D.z);
    }

    public List<DirectionalConstraint> GetDirectionalConstraints()
    {
        return directionalConstraints;
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


    //returns the list of incompatible types by direction
    public List<string> GetIncompatibleTypes(Vector2 direction)
    {
        //convert from direction enum to vector2, then search for the corresponding constraint
        var directionalConstraint = directionalConstraints.Find(c => c.GetDirection(transform) == direction);

        //spit out either the list of incompatible types or a new empty list of strings so there's no errors
        return directionalConstraint != null ? directionalConstraint.incompatibleContactTypes : new List<string>(); 
    }

    private void Rotate()
    {
        currentRotation = (currentRotation + 1) % 4; //rotate clockwise for 360 degree checking
    }

    //check whether the contact type in a given direction is usable
    public bool IsValidContact(Tile otherTile, Vector2 direction)
    {
        //get the contact type of the current tile in the given direction
        string currentTileContactType = GetContactType(direction);

        //get the opposite direction for the neighbouring tile
        Vector2 oppositeDirection = -direction;

        //get the contact type of the other tile in the opposite direction
        string otherTileContactType = otherTile != null ? otherTile.GetContactType(oppositeDirection) : "";

        //search for a directional constraint for the current tile in the given direction, otherwise it's null
        var currentTileConstraint = directionalConstraints.Count > 0 ? directionalConstraints.Find(c => c.GetDirection(transform) == direction) : null;

        //search for a directional constraint for the other tile in the opposite direction, otherwise it's null
        var otherTileConstraint = otherTile.directionalConstraints.Count > 0 ? otherTile.directionalConstraints.Find(c => c.GetDirection(otherTile.transform) == oppositeDirection) : null;

        Debug.Log($"Current Tile Contact Type: {currentTileContactType}, Other Tile Contact Type: {otherTileContactType}");
        Debug.Log($"Current Tile Constraint: {currentTileConstraint}, Other Tile Constraint: {otherTileConstraint}");


        //check if current tile is valid -> if there's no constraints or the current tile's constraints aren't in the other tile's incompatible types, it's true
        bool currentTileValid = currentTileConstraint == null || !currentTileConstraint.incompatibleContactTypes.Contains(otherTileContactType);

        //check if the other tile is valid -> if there's no constraints or the other tile's constraints aren't in the current tile's incompatible types, it's true
        bool otherTileValid = otherTileConstraint == null || !otherTileConstraint.incompatibleContactTypes.Contains(currentTileContactType);

        //if both are valid, then this connection can be made
        bool isValidConnection = currentTileValid && otherTileValid;

        return isValidConnection;
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class GameObjectArray
{
    public GameObject[] gameObjects;
}

public class TileManager : MonoBehaviour
{
    private Dictionary<Vector2, GameObject[]> objectRulesDictionary = new Dictionary<Vector2, GameObject[]>();
    private Dictionary<GameObject, int>[] objectLookAtDictionaryArray;
    public GameObject[] tiles;
    [SerializeField] private GameObjectArray[] ruleObjects = new GameObjectArray[4];

    private void Awake()
    {
        objectLookAtDictionaryArray = new Dictionary<GameObject, int>[tiles.Length];
    }
    
    private void SetDictionaries()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            Dictionary<GameObject, int> objectLookAtDictionary = new Dictionary<GameObject, int>() 
            {
                { tiles[i], i } //the index in the tiles array is the dictionaryArray's corresponding index
            };

            //index 0 = { floor,  0 }
            //index 1 = { wall,   1 }
            //index 2 = { corner, 2 }
            //index 3 = { door,   3 }

            objectLookAtDictionaryArray[i] = objectLookAtDictionary;
        }
    }

    private void ManualDictionarySetup()
    {
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };


        for (int i = 0; i < directions.Length; i++)
        {
            //index 0 = up
            //index 1 = down
            //index 2 = left
            //index 3 = right
            //for each index, there is an array of gameobjects that are valid

            objectRulesDictionary.Add(directions[i], ruleObjects[i].gameObjects);
        }
    }
}


/*1. create 2d array
2. check all 4 coordinates
loop i < x*y
>i + 1 (right)
>i - 1 (left)
>i + x (down)
>i - x (up)
3. if all 4 cells are null

dict 1 <GO, int> -> int gives index

array of dictionaries[] -> pass index into this

dict 2 <vect2 dir, GO[]> -> dir = where this object is relative, GO array gives all possible tiles based on rules
>rule dictionary

compare rules with current GO array -> if elements match, it's fine, if at any point it doesn't match, remove it
>make a new list with possible tiles based on this check

I wrote this shit down from the call with Hudson, doesn't really make sense but don't wanna lose it
*/
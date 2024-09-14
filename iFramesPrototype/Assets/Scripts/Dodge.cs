using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodge : MonoBehaviour
{
    [SerializeField] Collider[] hitboxes;
    [HideInInspector] public bool dodging;
    [SerializeField] CharacterController characterController;
    [SerializeField] Movement movement;

    public void OnStartDodge()
    {
        foreach (Collider col in hitboxes)
        {
            col.enabled = false;
        }

        dodging = true;
    }

    public void OnDodgeEnd()
    {
        foreach (Collider col in hitboxes)
        {
            col.enabled = true;
        }

        dodging = false;
    }

    //note for tmr - ondodgeend is on frame 60/98, onattackend is 72/90 -> check if it's gonna cause issues
}

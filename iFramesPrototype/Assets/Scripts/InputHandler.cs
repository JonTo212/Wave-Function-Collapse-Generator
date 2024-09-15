using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [HideInInspector] public Vector3 moveInput;
    [HideInInspector] public bool attackInput;
    [HideInInspector] public bool dodgeInput;
    [SerializeField] Animator anim;
    [SerializeField] Movement movement;
    [SerializeField] Attack attack;
    [SerializeField] Dodge dodge;

    void Update()
    {
        GetInput();
        bool actionHappening = attack.attacking || dodge.dodging;

        //attack check
        if (attackInput && !actionHappening)
        {
            attack.OnStartAttack();
        }

        //dodge check
        if (dodgeInput && !actionHappening)
        {
            dodge.OnStartDodge();
        }

        //movement check - can't move if attacking/dodging
        if (!actionHappening)
        {
            movement.RotateCharacterModel(moveInput);
            movement.Move(moveInput);
        }
    }

    void GetInput()
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");

        moveInput = new Vector3(xInput, 0, zInput);
        dodgeInput = Input.GetKeyDown(KeyCode.LeftShift);
        attackInput = Input.GetMouseButtonDown(0);
    }
}

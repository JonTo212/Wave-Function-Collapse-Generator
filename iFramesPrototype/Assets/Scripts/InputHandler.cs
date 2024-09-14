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

        if(movement != null)
        {
            movement.RotateCharacterModel(moveInput);
            movement.Move(moveInput);
        }
        if (attack != null)
        {
            if(attackInput && !attack.attacking && !dodge.dodging)
            {
                attack.OnStartAttack();
            }
        }

        if(dodge != null)
        {
            if(dodgeInput && !dodge.dodging && !attack.attacking)
            {
                dodge.OnStartDodge();
            }
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

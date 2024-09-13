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

    void Update()
    {
        GetInput();
        UpdateAnimator();

        if(movement != null)
        {
            movement.RotateCharacterModel(moveInput);
            movement.Move(moveInput);
        }
        if (attack != null)
        {
            if(attackInput)
            {
                attack.OnStartAttack();
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

    void UpdateAnimator()
    {
        if(attackInput)
        {
            anim.SetTrigger("Attack");
            attackInput = false;
        }

        if (dodgeInput)
        {
            anim.SetTrigger("Dodge");
            dodgeInput = false;
        }

        anim.SetBool("Moving", moveInput != Vector3.zero);
    }
}

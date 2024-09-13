using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [HideInInspector] public Vector3 moveDir;
    [HideInInspector] public bool attackInput;
    [HideInInspector] public bool dodgeInput;
    [SerializeField] Animator anim;

    void Update()
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");

        moveDir = new Vector3(xInput, 0, zInput);
        dodgeInput = Input.GetKeyDown(KeyCode.LeftShift);
        attackInput = Input.GetMouseButtonDown(0);

        UpdateAnimator();
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

        anim.SetBool("Moving", moveDir != Vector3.zero);
    }
}

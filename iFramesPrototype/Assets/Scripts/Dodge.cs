using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodge : MonoBehaviour
{
    [SerializeField] Collider[] hitboxes;
    [HideInInspector] public bool dodging;
    [SerializeField] CharacterController characterController;
    [SerializeField] InputHandler input;
    [SerializeField] Movement movement;
    [SerializeField] float dodgeSpeed;
    [SerializeField] Animator anim;
    Vector3 dodgeStartMovementInput;

    public void OnStartDodge()
    {
        dodging = true;
        dodgeStartMovementInput = input.moveInput;
        OnIFramesStart();
        StartCoroutine(HandleDodge());
    }
    
    IEnumerator HandleDodge()
    {
        float dodgeTime = 0f;

        //Get animator transition + animation full duration
        AnimatorTransitionInfo transitionInfo = anim.GetAnimatorTransitionInfo(0);
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        float totalDodgeDuration = transitionInfo.duration + stateInfo.length;

        while (dodgeTime < totalDodgeDuration && dodging)
        {
            if (dodgeStartMovementInput != Vector3.zero)
            {
                characterController.Move(dodgeStartMovementInput * dodgeSpeed * Time.deltaTime);
            }
            else
            {
                characterController.Move(transform.forward * dodgeSpeed * Time.deltaTime);
            }
            dodgeTime += Time.deltaTime;

            yield return null;
        }
    }

    void OnDodgeEnd()
    {
        dodging = false;
    }

    void OnIFramesStart()
    {
        foreach (Collider col in hitboxes)
        {
            col.enabled = false;
        }
    }

    void OnIFramesEnd()
    {
        foreach (Collider col in hitboxes)
        {
            col.enabled = true;
        }
    }
}

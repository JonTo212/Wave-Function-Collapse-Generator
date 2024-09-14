using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    CharacterController characterController;
    [SerializeField] float moveSpeed;
    [SerializeField] float rotSpeed;
    [SerializeField] GameObject characterModel;
    [HideInInspector] public bool moving;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void RotateCharacterModel(Vector3 moveDir)
    {
        if (moveDir.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            characterModel.transform.rotation = Quaternion.Slerp(characterModel.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
        }
    }

    public void Move(Vector3 moveDir)
    {
        if (moveDir != Vector3.zero)
        {
            characterController.SimpleMove(moveDir * moveSpeed);
            moving = true;
        }
        else
        {
            moving = false;
        }
    }
}

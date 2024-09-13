using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    CharacterController characterController;
    [SerializeField] float moveSpeed;
    [SerializeField] float rotSpeed;
    [SerializeField] GameObject characterModel;

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
        characterController.SimpleMove(moveDir * moveSpeed);
    }
}

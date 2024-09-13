using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] GameObject hitbox;
    bool attacking;
    [HideInInspector] public int damage;
    public void OnStartAttack()
    {
        attacking = true;
        hitbox.SetActive(true);
    }

    public void OnAttackEnd()
    {
        attacking = false;
        hitbox.SetActive(false);
    }
}

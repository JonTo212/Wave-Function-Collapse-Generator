using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] GameObject hitbox;
    [HideInInspector] public bool attacking;
    public int damage;
    public void OnStartAttack()
    {
        attacking = true;
        hitbox.SetActive(true);
    }

    public void OnAttackEnd()
    {
        attacking = false;
        hitbox.GetComponent<HitboxDetect>().hit = false;
        hitbox.SetActive(false);
    }
}

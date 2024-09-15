using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int health;
    [HideInInspector] public bool alive;
    [SerializeField] int maxHealth;
    [HideInInspector] public bool gettingHit;
    [SerializeField] Attack attack;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        alive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            alive = false;
        }
    }

    public void OnTakeHitStart(int damage)
    {
        health -= damage;
        gettingHit = true;

        if(attack.attacking)
        {
            attack.OnAttackEnd();
        }
    }

    void OnTakeHitEnd()
    {
        gettingHit = false;
    }
}

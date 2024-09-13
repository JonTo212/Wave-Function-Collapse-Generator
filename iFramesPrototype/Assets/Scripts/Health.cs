using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [HideInInspector] public int health;
    [HideInInspector] public bool alive;
    [SerializeField] int maxHealth;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            alive = false;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float detectionRadius;
    [SerializeField] float detectionDelay;
    [SerializeField] float attackRadius;
    [SerializeField] Attack attack;
    [SerializeField] Health health;
    [SerializeField] float attackDelay;
    float squaredAttackRadius;
    float squaredDetectionRadius;
    float timer;
    float attackTimer;
    Vector3 toOther;
    Movement movement;

    void Start()
    {
        squaredDetectionRadius = detectionRadius * detectionRadius;
        squaredAttackRadius = attackRadius * attackRadius;
        movement = GetComponent<Movement>();
    }

    void Update()
    {
        if (!attack.attacking && !health.gettingHit)
        {
            DetectPlayer();
            movement.Move(toOther);
            movement.RotateCharacterModel(toOther);
        }
    }

    void DetectPlayer()
    {
        float distanceToPlayerSquared = (player.position - transform.position).sqrMagnitude;
        if (distanceToPlayerSquared <= squaredDetectionRadius)
        {
            Vector3 debugRay = transform.position - player.position;
            Debug.DrawRay(transform.position, debugRay, Color.red);

            timer += Time.deltaTime;
            if(timer > detectionDelay)
            {
                toOther = player.position - transform.position;

                if (distanceToPlayerSquared <= squaredAttackRadius)
                {
                    toOther = Vector3.zero;
                    attackTimer += Time.deltaTime;

                    if (attackTimer > attackDelay)
                    {
                        attack.OnStartAttack();
                        attackTimer = 0;
                    }
                }
            }
        }
        
        else
        {
            timer = 0;
            attackTimer = 0;
            toOther = Vector3.zero;
        }
    }
}
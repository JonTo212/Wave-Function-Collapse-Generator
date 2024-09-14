using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxDetect : MonoBehaviour
{
    Collider col;
    [HideInInspector] public bool hit;
    [SerializeField] Attack attack;
    [SerializeField] LayerMask enemyLayer;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(hit)
        {
            return;
        }

        LayerMask convertedMask = (1 << other.gameObject.layer);

        if (convertedMask == enemyLayer)
        {
            Health health = other.GetComponent<Health>();
            health.TakeDamage(attack.damage);
            hit = true;
        }
    }
}

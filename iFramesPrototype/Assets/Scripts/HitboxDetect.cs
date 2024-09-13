using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxDetect : MonoBehaviour
{
    Collider col;
    [SerializeField] Attack attack;

    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            Health health = other.GetComponent<Health>();
            health.TakeDamage(attack.damage);
        }
    }
}

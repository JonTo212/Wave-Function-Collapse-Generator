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
            CapsuleCollider[] childColliders = other.GetComponentsInChildren<CapsuleCollider>();

            foreach (CapsuleCollider collider in childColliders)
            {
                Health health = collider.GetComponentInParent<Health>();
                Dodge dodge = collider.GetComponentInParent<Dodge>();

                if (health != null && health.alive)
                {
                    if(dodge != null && dodge.dodging)
                    {
                        break;
                    }

                    health.OnTakeHitStart(attack.damage);
                    hit = true;
                    break;
                }
            }
        }
    }
}

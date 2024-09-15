using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Movement movement;
    [SerializeField] Attack attack;
    [SerializeField] Dodge dodge;

    // Update is called once per frame
    void Update()
    {
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        anim.SetBool("Moving", movement.moving);
        anim.SetBool("Attack", attack.attacking);

        if(dodge != null) //enemy doesn't have dodge script
        {
            anim.SetBool("Dodge", dodge.dodging);
        }
    }
}

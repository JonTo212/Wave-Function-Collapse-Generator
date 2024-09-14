using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float detectionRadius;
    [SerializeField] float detectionDelay;
    float squaredRadius;
    float timer;
    Vector3 toOther;
    Movement movement;

    void Start()
    {
        squaredRadius = detectionRadius * detectionRadius;
        movement = GetComponent<Movement>();
    }

    void Update()
    {
        DetectPlayer();
        movement.Move(toOther);
        movement.RotateCharacterModel(toOther);
    }

    void DetectPlayer()
    {
        if ((player.position - transform.position).sqrMagnitude <= squaredRadius)
        {
            toOther = player.position - transform.position;

            if (!Physics.Raycast(transform.position, toOther.normalized, toOther.magnitude))
            {
                Debug.DrawRay(transform.position, toOther, Color.red);
                timer += Time.deltaTime;
            }
        }
        
        else
        {
            timer = 0;
            toOther = Vector3.zero;
        }
    }
}


public class EnemyDetect : MonoBehaviour
{
    public Transform player;
    public float detectionRadius;
    public LayerMask obstacleMask;
    float squaredRadius;
    public float detectionDelay;
    float timer;
    [SerializeField] bool detected; //for visualization
    [SerializeField] TMP_Text popupText;
    [SerializeField] float textHeight;

    void Start()
    {
        squaredRadius = detectionRadius * detectionRadius;
    }

    void Update()
    {
        if (Detect())
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            detected = false;
            popupText.gameObject.SetActive(false);
        }

        if (timer > detectionDelay)
        {
            detected = true;
            //do enemy stuff here

            if (popupText != null)
            {
                popupText.gameObject.SetActive(true);

                //Convert enemy position to screen position
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * textHeight);

                //Update the position of the popupText to match the screen position
                popupText.rectTransform.position = screenPosition;
            }
        }
    }

    bool Detect()
    {
        if ((player.position - transform.position).sqrMagnitude <= squaredRadius)
        {
            Vector3 toOther = player.position - transform.position;

            if (Vector3.Dot(transform.forward, toOther.normalized) > Mathf.Cos(45 * Mathf.Deg2Rad))
            {
                if (!Physics.Raycast(transform.position, toOther.normalized, toOther.magnitude, obstacleMask))
                {
                    Debug.DrawRay(transform.position, toOther, Color.red);
                    return true;
                }
            }
        }
        return false;
    }
}

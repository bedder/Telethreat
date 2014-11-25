using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    public float AttackRate;
    public float AttackDamage;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.LogWarning("TEST (OnControllerColliderHit) - " + hit.collider.gameObject.name);
    }


    void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning("Test (OnCollisionEnter) - " + collision.collider.gameObject.name);
    }
}

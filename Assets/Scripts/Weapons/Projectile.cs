using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public float lifespan = 10f;
    public float damage = 1f;

    void Start()
    {
        Destroy(gameObject, lifespan);
    }

    void Update()
    {
    }

    void OnCollisionEnter(Collision collision) 
    {
        Damageable target = collision.gameObject.GetComponent<Damageable>();
        PlayerController playerTarget = collision.gameObject.GetComponent<PlayerController>();
        
        if (target != null)
        {
            target.damage(damage);
        }
        else if (playerTarget != null)
        {
            playerTarget.damage(damage);
        }

        Destroy(gameObject);
    }
}

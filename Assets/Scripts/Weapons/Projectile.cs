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
        Debug.LogWarning("Collision");
        Damageable target = collision.gameObject.GetComponent<Damageable>();
        PlayerController playerTarget = collision.gameObject.GetComponent<PlayerController>();
        
        if (target != null)
        {
            Debug.LogWarning("Hit damageable");
            target.damage(damage);
        }
        else if (playerTarget != null)
        {
            Debug.LogWarning("Hit player");
            playerTarget.damage(damage);
        }

        Debug.LogWarning("Missed!");

        Destroy(gameObject);
    }
}

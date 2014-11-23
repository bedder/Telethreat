using UnityEngine;
using System.Collections;

public class Damageable : MonoBehaviour {
    public float health = 1f;

    public void damage(float damage) {
        health -= damage;
        if (health <= 0) {
            kill();
        }
    }

    void kill() {
        Destroy(gameObject);
    }
}
using UnityEngine;
using System.Collections;

public class Explosive : MonoBehaviour {
    public float lifespan = 3;
    public float damage = 5;

    public float explosionRadius = 4;
    public float explosionDuration = 0.5f;
    public GameObject explosionPrefab;

    private float explosionTime;
    private bool isExploding = false;

	void Start () {
        explosionTime = Time.time + lifespan;
	}
	
	void Update () {
        if (Time.time > explosionTime) {
            explode();
        }
	}

    public void explode() {
        if (!isExploding) {
            isExploding = true;

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider collider in colliders) {
                Damageable damageable = collider.gameObject.GetComponent<Damageable>();
                if (damageable != null) {
                    damageable.damage(damage);
                }
                Explosive explosive = collider.gameObject.GetComponent<Explosive>();
                if (explosive != null) {
                    explosive.explode();
                }
            }

            // Spawn explosion prefab
            GameObject explosion;
            explosion = (GameObject)Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.transform.localScale = explosionRadius * Vector3.one;

            // Cleanup
            Destroy(gameObject);
            Destroy(explosion, explosionDuration);
        }
    }
}

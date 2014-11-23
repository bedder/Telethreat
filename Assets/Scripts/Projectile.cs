using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
    public float lifespan;

	void Start () {
        Destroy(gameObject, lifespan);
	}
	
	void Update () {
	}

    void OnCollisionEnter(Collision collision) {
        Destroy(gameObject);
    }
}

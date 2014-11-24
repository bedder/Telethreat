using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {
    public Transform barrelEnd;
    public float maxRange = 20f;
    public float scatterX = 0.1f;
    public float scatterY = 0.01f;
    public float timeBetweenShots = 0.1f;
    public int numberOfProjectiles = 1;

    private float nextAvailableShot = 0f;

    public void fire() {
        if (Time.time >= nextAvailableShot) {
            nextAvailableShot = Time.time + timeBetweenShots;
            for (int projectile = 0 ; projectile < numberOfProjectiles ; projectile++) {
                Vector3 scatterDirection = Random.onUnitSphere;
                scatterDirection.Scale(new Vector3(scatterX, scatterY, scatterX));

                Vector3 shotDirection = barrelEnd.forward + scatterDirection;
                shotDirection.Normalize();

                individualShot(ref shotDirection);
            }
        }
    }

    public virtual void individualShot(ref Vector3 shotDirection) {
        Debug.LogError("Using default version of Gun.individualShot()");
    }
}

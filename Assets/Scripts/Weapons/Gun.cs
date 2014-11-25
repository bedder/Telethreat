using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {
    public Transform barrelEnd;
    public float maxRange = 20f;
    public float scatterX = 0.1f;
    public float scatterY = 0.01f;
    public float timeBetweenShots = 0.1f;
    public int numberOfProjectiles = 1;
    public float energy = 100f;
    public float energyCost = 0f;

    private float nextAvailableShot = 0f;

    public void fire() {
        if (Time.time >= nextAvailableShot && energy > energyCost) {
            nextAvailableShot = Time.time + timeBetweenShots;
            energy -= energyCost;
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

    public void recharge(float rechargeAmount) {
        energy += rechargeAmount;
        energy = Mathf.Min(100f, energy);
    }
}

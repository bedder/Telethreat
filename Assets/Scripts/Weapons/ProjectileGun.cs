using UnityEngine;
using System.Collections;

public class ProjectileGun : Gun {
    public Rigidbody bullet;
    public float projectileForce = 1000;

    public override void individualShot(ref Vector3 shotDirection) {
        Rigidbody clone;
        clone = (Rigidbody)Instantiate(bullet, barrelEnd.position, Quaternion.LookRotation(shotDirection));
        clone.AddForce(shotDirection * projectileForce);
    }
}

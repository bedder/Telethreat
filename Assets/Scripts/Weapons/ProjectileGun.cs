using UnityEngine;
using System.Collections;

public class ProjectileGun : Gun {
    public Rigidbody[] bulletTypes;
    private Rigidbody bullet;

    public float projectileForce = 1000;

    public void Start() {
        if (bulletTypes.Length > 0)
            bullet = bulletTypes[0];
        else
            Debug.LogError("No bulletTypes are defined.");
    }

    public void setBulletType(int type) {
        if (type >= 0 && type < bulletTypes.Length) {
            bullet = bulletTypes[type];
        } else {
            Debug.LogError("Trying to switch to an unknown BulletType.");
        }
    }

    public override void individualShot(ref Vector3 shotDirection) {
        Rigidbody clone;
        clone = (Rigidbody)Instantiate(bullet, barrelEnd.position, Quaternion.LookRotation(shotDirection));
        clone.AddForce(shotDirection * projectileForce);
    }
}

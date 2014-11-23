using UnityEngine;
using System.Collections;

public class RaycastGun : Gun {
    public float damage = 1f;

    public override void individualShot(ref Vector3 shotDirection) {
        Ray ray = new Ray(barrelEnd.position, shotDirection);
        RaycastHit raycastHit;

        float distance = maxRange;
        if (Physics.Raycast(ray, out raycastHit, distance)) {
            distance = raycastHit.distance;

            Damageable target = raycastHit.collider.gameObject.GetComponent<Damageable>();
            if (target != null) {
                target.damage(damage);
            }
        }

        Debug.DrawRay(ray.origin, shotDirection * distance, Color.red, 1f);
    }
}

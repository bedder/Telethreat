using UnityEngine;
using System.Collections;

public class RaycastGun : Gun {
    public override void individualShot(ref Vector3 shotDirection) {
        Ray ray = new Ray(barrelEnd.position, shotDirection);
        RaycastHit raycastHit;

        float distance = maxRange;
        if (Physics.Raycast(ray, out raycastHit, distance)) {
            distance = raycastHit.distance;
        }

        Debug.DrawRay(ray.origin, shotDirection * distance, Color.red, 1f);
    }
}

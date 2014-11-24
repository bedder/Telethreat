using UnityEngine;
using System.Collections;

public class HeavyInfantryController : PlayerController {
    public Rigidbody grenade;
    public Transform leftHand;

    public override void performClassAction() { // Magic numbers chosen by magic.
        Rigidbody clone;
        Vector3 throwDirectionEuler = Vector3.Lerp(transform.forward, Vector3.up, 0.4f);
        clone = (Rigidbody)Instantiate(grenade, leftHand.position, Quaternion.LookRotation(throwDirectionEuler));
        clone.AddForce(throwDirectionEuler * 600);
    }
}
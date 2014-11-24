using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public Gun gun;
    public enum AimType { Keyboard, Mouse };
    public AimType aimType = AimType.Mouse;
    public float rotationSpeed = 450;
    public float walkSpeed = 10;
    public float runSpeed = 10;
    public float fallSpeed = 8;
    public float maxHealth = 100;
    public float maxArmour = 100;
    public float armourRegenDelay = 2;
    public float armourRegenRate = 10;

    private CharacterController characterController;
    private Camera camera;

    private Quaternion targetRotation;
    private float regenArmourTime;
    public float health;
    public float armour;

    public virtual void Start () {
        characterController = GetComponent<CharacterController>();
        camera = Camera.main;
        regenArmourTime = 0f;
        health = maxHealth;
        armour = maxArmour;
	}
	
	void Update () {
        regen();
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        performRotation(ref input);
        performMovement(ref input);
        performActions();
	}

    void regen() {
        if (Time.time > regenArmourTime && armour != maxArmour) {
            armour += (armourRegenRate * Time.deltaTime);
            armour = Mathf.Min(armour, maxArmour);
        }
    }

    void performRotation(ref Vector3 input) {
        // Set orientation
        if (aimType == AimType.Keyboard) {
            performRotationKeyboard(ref input);
        } else if (aimType == AimType.Mouse) {
            performRotationMouse();
        } else {
            Debug.LogError("Player AimType unknown.");
        }
    }

    void performRotationKeyboard(ref Vector3 input) {
        if (input != Vector3.zero) {
            targetRotation = Quaternion.LookRotation(input);
            transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
        }
    }

    void performRotationMouse() {
        Vector3 screenPosition = Input.mousePosition;
        Vector3 worldPosition = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camera.transform.position.y - transform.position.y));

        targetRotation = Quaternion.LookRotation(worldPosition - transform.position);
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
    }

    void performMovement(ref Vector3 input) {
        Vector3 velocity = input;
        velocity *= (velocity.magnitude==2 ? 0.7f : 1f);
        velocity *= (Input.GetButton("Run") ? runSpeed : walkSpeed);
        velocity += (Vector3.up * -fallSpeed);
        characterController.Move(velocity * Time.deltaTime);
    }

    void performActions() {
        if (Input.GetButton("Fire")) {
            gun.fire();
        }
        if (Input.GetButtonDown("ClassAction")) {
            performClassAction();
        }

        // DEBUG BINDINGS BELOW THIS POINT
        if (Input.GetButton("DEBUGDAMAGE")) {
            damage(10);
        }
    }

    public virtual void performClassAction() {
        Debug.Log("PlayerController::performClassAction does nothing by default. Inherited behavious scripts should be used to perform actions.");
    }

    void damage(float damage) {
        regenArmourTime = Time.time + armourRegenDelay;

        if (damage > armour) {
            health -= (damage - armour);
            armour = 0;
        } else {
            armour -= damage;
        }

        if (health <= 0) {
            kill();
        }
    }

    void kill() { // TODO
        Debug.LogWarning("Warning: code for handling player death is currently not implemented.");
        Destroy(gameObject);
    }
}

using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public enum AimType { Keyboard, Mouse };
    public AimType aimType = AimType.Keyboard;
    public float rotationSpeed = 450;
    public float walkSpeed = 10;
    public float runSpeed = 20;
    public float fallSpeed = 8;

    private CharacterController characterController;
    private Camera camera;

    private Quaternion targetRotation;

    void Start () {
        characterController = GetComponent<CharacterController>();
        camera = Camera.main;
	}
	
	void Update () {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        performRotation(ref input);
        performMovement(ref input);
        performActions();
	}

    void performRotation(ref Vector3 input) {
        // Set orientation
        if (aimType == AimType.Keyboard) {
            performRotationKeyboard(ref input);
        } else if (aimType == AimType.Mouse) {
            performRotationMouse();
        } else {
            Debug.Log("Player AimType unknown.");
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

    }
}

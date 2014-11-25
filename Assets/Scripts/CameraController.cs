using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public float cameraSpeed = 10f;
    
    private Transform cameraTarget;

    public Vector3 usableArea;
    private Vector3 desiredPosition;

	void Start () {
        desiredPosition = transform.position;
        cameraTarget = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	void Update () {
        Vector3 offset = transform.position - cameraTarget.position;

        if (offset.x > usableArea.x) {
            desiredPosition.x = cameraTarget.position.x + usableArea.x;
        } else if (offset.x < -usableArea.x) {
            desiredPosition.x = cameraTarget.position.x - usableArea.x;
        }

        if (offset.z > usableArea.z) {
            desiredPosition.z = cameraTarget.position.z + usableArea.z;
        } else if (offset.z < -usableArea.z) {
            desiredPosition.z = cameraTarget.position.z - usableArea.z;
        }
        offset = Time.deltaTime * cameraSpeed * (desiredPosition - transform.position);
        transform.Translate(offset.x, offset.z, offset.y);
	}
}

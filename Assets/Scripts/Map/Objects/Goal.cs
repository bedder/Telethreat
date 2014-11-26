using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    public float range = 1f;
    private Transform playerTransform;

	void Start () {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	void Update () {
        Vector3 delta = playerTransform.position - transform.position;
        if (playerTransform != null) {
            if (delta.magnitude <= range) {
                Debug.Log("In range of goal!"); // TODO
            }
        }
	}
}

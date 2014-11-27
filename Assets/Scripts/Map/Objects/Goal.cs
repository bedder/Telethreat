using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    public float range = 1f;

    private Transform playerTransform;
    private GameController gameController;

	void Start () {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        gameController = GameObject.FindObjectOfType<GameController>();
	}
	
	void Update () {
        Vector3 delta = playerTransform.position - transform.position;
        if (playerTransform != null) {
            if (delta.magnitude <= range) {
                Application.LoadLevel(gameController.nextLevel);
            }
        }
	}
}

using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {
    public float range = 1f;

    private Transform playerTransform;
    private GameController gameController;

	void Start () {
        gameController = GameObject.FindObjectOfType<GameController>();
		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	void Update () {
        if (playerTransform != null) {
            Vector3 delta = playerTransform.position - transform.position;
            if (delta.magnitude <= range) {
                Application.LoadLevel(gameController.nextLevel);
            }
        }
	}
}

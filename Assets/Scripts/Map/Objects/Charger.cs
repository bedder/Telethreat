using UnityEngine;
using System.Collections;

public class Charger : MonoBehaviour {
    public float chargeRate = 1;
    public float range = 5;

    private PlayerController player;

	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}
	
	void Update () {
        Vector3 offset = transform.position - player.transform.position;
        if (offset.magnitude < range) {
            player.recharge(chargeRate * Time.deltaTime);
        }
	}
}

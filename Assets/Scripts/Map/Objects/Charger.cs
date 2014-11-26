using UnityEngine;
using System.Collections;

public class Charger : MonoBehaviour {
    public float chargeRate = 1;
    public float range = 5;

    public GameObject chargerIndicator;
    public Material indicatorOff;
    public Material indicatorOn;

    public Transform orb;

    private PlayerController player;

	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}
	
	void Update () {
        Vector3 offset = transform.position - player.transform.position;
        if (offset.magnitude < range) {
            player.recharge(chargeRate * Time.deltaTime);
            chargerIndicator.renderer.material = indicatorOn;
            levitateOrb();
        } else {
            chargerIndicator.renderer.material = indicatorOff;
            dropOrb();
        }
	}

    void levitateOrb() {
        if (orb.position.y < 2) {
            orb.Translate(Time.deltaTime * Vector3.up);
        }
    }

    void dropOrb() {
        if (orb.position.y > 1) {
            orb.Translate(-Time.deltaTime * Vector3.up);
        }
    }
}

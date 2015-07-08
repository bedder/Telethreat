using UnityEngine;
using System.Collections;

public class Charger : MonoBehaviour {
    public float chargeRate = 1;
    public float range = 5;

    public GameObject chargerIndicator;
    public Material indicatorOff;
    public Material indicatorOn;

    public ParticleSystem orbIndicator;
    public Transform orb;

    private PlayerController player;
    private bool isEnabled = false;

	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        orbIndicator.Stop();
	}
	
	void Update () {
        if (player == null)
            return;
        Vector3 offset = transform.position - player.transform.position;
        bool inRange = offset.magnitude < range;
        if (inRange && !isEnabled) {
            isEnabled = true;
            player.recharge(chargeRate * Time.deltaTime);
            chargerIndicator.GetComponent<Renderer>().material = indicatorOn;
            orbIndicator.Play();
        } else if (!inRange && isEnabled) {
            isEnabled = false;
            chargerIndicator.GetComponent<Renderer>().material = indicatorOff;
            orbIndicator.Stop();
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

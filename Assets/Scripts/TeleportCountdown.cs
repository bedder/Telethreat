using UnityEngine;
using System.Collections;

public class TeleportCountdown : MonoBehaviour {
    public float timerLength = 15;

    private float lastReset;

	void Start () {
        lastReset = Time.time;
	}
	
	void Update () {
        if (timeLeft() == 0) {
            trigger();
        }
	}

    void trigger() {
        lastReset = Time.time;
    }

    public float timeLeft() {
        return Mathf.Max(lastReset + timerLength - Time.time, 0f);
    }
}

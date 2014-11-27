using UnityEngine;
using System.Collections;

public class TeleportCountdown : MonoBehaviour {
    public float timerLength = 15;

    private float lastReset;

    private bool alarmsActive = false;
    private float alarmPeriod = 5;
    private AlarmLight[] alarmLights;

    private float dimRamp = 1.5f;
    private float dimPeriod = 5.75f;
    private Light[] lights;
    public float[] lightIntensities;
    public bool[] isDimmable;

	void Start () {
        alarmLights = GameObject.FindObjectsOfType<AlarmLight>();

        lights = GameObject.FindObjectsOfType<Light>();
        lightIntensities = new float[lights.Length];
        isDimmable = new bool[lights.Length];
        for (int i = 0 ; i < lights.Length ; i++) {
            if (lights[i].GetComponentInParent<AlarmLight>() == null) {
                lightIntensities[i] = lights[i].intensity;
                isDimmable[i] = true;
            } else {
                isDimmable[i] = false;
            }
        }

        lastReset = Time.time;
	}
	
	void Update () {
        float remaining = timeLeft();
        if (remaining == 0) {
            deactivateAlarms();
            raiseLights();
            trigger();
            return;
        }
        if (!alarmsActive && remaining < alarmPeriod) {
            activateAlarms();
        }
        if (remaining < dimPeriod) {
            dimLights();
        } 
	}

    void trigger() {
        lastReset = Time.time;
    }

    void activateAlarms() {
        foreach (AlarmLight alarm in alarmLights) {
            alarm.activate();
        }
        alarmsActive = true;
    }

    void deactivateAlarms() {
        foreach (AlarmLight alarm in alarmLights) {
            alarm.deactivate();
        }
        alarmsActive = false;
    }

    void dimLights() {
        float factor = Mathf.Max(0, (timeLeft() - dimPeriod + dimRamp) / dimRamp);
        for (int i = 0 ; i < lights.Length ; i++) {
            if (isDimmable[i])
                lights[i].intensity = factor * lightIntensities[i];
        }
    }

    void raiseLights() {
        for (int i = 0 ; i < lights.Length ; i++) {
            if (isDimmable[i])
                lights[i].intensity = lightIntensities[i];
        }
    }

    public float timeLeft() {
        return Mathf.Max(lastReset + timerLength - Time.time, 0f);
    }
}

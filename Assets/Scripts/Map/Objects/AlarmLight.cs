using UnityEngine;
using System.Collections;

public class AlarmLight : MonoBehaviour {
    public Light[] lights;
    public float rotationSpeed = 180;

    private float targetIntensity;
    private bool activated = true;

    void Start () {
        targetIntensity = lights[0].intensity;
        deactivate();
	}

    void Update() {
        if (activated)
            transform.Rotate(new Vector3(0f, rotationSpeed * Time.deltaTime, 0f));
    }

    public void activate() {
        foreach (Light light in lights)
            light.intensity = targetIntensity;
        activated = true;
    }

    public void deactivate() {
        foreach (Light light in lights)
            light.intensity = 0f;
        activated = false;
    }
}

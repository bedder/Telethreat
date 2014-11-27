using UnityEngine;
using System.Collections;

public class AlarmLight : MonoBehaviour {
    public Light[] lights;
    public float rotationSpeed = 180;

    private float targetIntensity;
    private bool activated = true;
    private AudioSource audioSource;

    void Start () {
        targetIntensity = lights[0].intensity;
        deactivate();
		audioSource = GetComponent<AudioSource>();
	}

    void Update() {
        if (activated)
            transform.Rotate(new Vector3(0f, rotationSpeed * Time.deltaTime, 0f));
    }

    public void activate() {
        foreach (Light light in lights)
        {
            light.intensity = targetIntensity;
        }

        if(audioSource != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
        
        activated = true;
    }

    public void deactivate() {	
        foreach (Light light in lights) {
			light.intensity = 0f;
		}
		if(audioSource != null)
		{
			audioSource.Stop();
		}
        activated = false;
    }
}

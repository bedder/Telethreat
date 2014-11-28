using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
    public int nextLevel;
    public GameObject playerController;

    void Start () {
        GameController[] controllers = GameObject.FindObjectsOfType<GameController>();
        if (controllers.Length > 1)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject); // Keep between levels
        nextLevel = 1;
	}
}

using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
    public int nextLevel;
    public GameObject playerController;

    void Start () {
        DontDestroyOnLoad(gameObject); // Keep between levels
        nextLevel = 1;
	}
}

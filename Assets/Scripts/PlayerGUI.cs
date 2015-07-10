using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerGUI : MonoBehaviour {
    public RectTransform ArmourFill;
    public RectTransform HealthFill;
    public RectTransform WeaponFill;
    public Text WeaponText;
    public RectTransform CompassSprite;
    public GameObject DeathUi;
    public GameObject PauseUi;
    public Text CountdownText;

    private Goal goal;
    private TeleportCountdown countdown;
    private PlayerController player;
    private Gun gun;
    private GameController gameController;

    private float displayLevelNumberFor = 2f;
    private float displayLevelNumberUntil;

    private bool paused = false;
    private float lastPause = 0f;

    private float previousEnergy = -999f;
    private int previousWeapon = -999;

    void Start() {
        goal = GameObject.FindObjectOfType<Goal>();
        countdown = GameObject.FindObjectOfType<TeleportCountdown>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        gameController = GameObject.FindObjectOfType<GameController>();
        gun = player.gun;

        gameController.nextLevel++;
        displayLevelNumberUntil = Time.time + displayLevelNumberFor;

        DeathUi.SetActive(false);
    }

    void OnGUI() {
        updateValues();

        if (player != null && goal != null) {
            Vector3 delta = goal.transform.position - player.transform.position;
            float angle = Mathf.Atan2(delta.z, delta.x) * 180 / Mathf.PI + 90;
            CompassSprite.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        if (Input.GetButtonDown("Pause") && (Time.realtimeSinceStartup - lastPause) > 0.01f) {
            paused = !paused;
            lastPause = Time.realtimeSinceStartup; // TODO
                                                   // This is a bit of a hacky solution to prevent the game
                                                   // pausing and unpausing instantly on pressing Esc. It might
                                                   // just be a key bounce error on my laptop?
            Time.timeScale = paused ? 0 : 1;
            PauseUi.SetActive(paused);
        }
    }

    void updateValues() {
        if (player != null) {
            ArmourFill.localScale = new Vector2(player.armour / player.maxArmour, 1);
            HealthFill.localScale = new Vector2(player.health / player.maxHealth, 1);
        } else {
            ArmourFill.localScale = new Vector2(0, 1);
            HealthFill.localScale = new Vector2(0, 1);
            DeathUi.SetActive(true);
            if (Input.GetButton("Restart")) {
                gameController.nextLevel = Application.loadedLevel;
                Application.LoadLevel(gameController.nextLevel);
            }
            if (Input.GetButton("Exit")) {
                gameController.nextLevel = 1;
                Application.LoadLevel(0);
            }
        }
        if (gun != null && gun.energy != previousEnergy) {
            previousEnergy = gun.energy;
            WeaponFill.localScale = new Vector2(previousEnergy / 100f, 1);
        }
        if (player.weaponNumber != previousWeapon) {
            WeaponText.text = player.weaponName;
            previousWeapon = player.weaponNumber;
        }
        if (countdown != null) {
            TimeSpan time = TimeSpan.FromSeconds(countdown.timeLeft());
            CountdownText.text = String.Format("{0:D2}.{1:D3}", time.Seconds, time.Milliseconds);
        }
    }
}

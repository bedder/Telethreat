using UnityEngine;
using System.Collections;

public class PlayerGUI : MonoBehaviour {
    public Font timerFont;
    public Font mainFont;

    private TeleportCountdown countdown;
    private PlayerController player;
    private Gun gun;

    private int healthbarHeight = 20;
    private int healthbarWidth; // Set automatically w/ respect to screen width
    private int energybarHeight = 200;
    private int energybarWidth = 20;
    private int weaponlabelWidth = 256;
    private int weaponLabelHeight = 20;
    private int weaponIconHeight = 64;
    private int timerWidth = 450;
    private int timerHeight = 60;
    private int pad = 6;

    private Rect healthGroup;
    private Rect healthbarActual;
    private Rect healthbarFull;
    private Rect armourMeter;
    private Rect armourbarActual;
    private Rect armourbarFull;

    private Rect energyGroup;
    private Rect energybarActual;
    private Rect energybarFull;

    private Rect weaponGroup;
    private Rect weaponIcon;
    private Rect weaponLabel;

    private Rect timerGroup;
    private Rect timer;

    void Start() {
        countdown = gameObject.GetComponent<TeleportCountdown>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        gun = player.gun;

        healthbarWidth = (int)(Screen.width - (3 * pad)) / 2;
        
        healthGroup = new Rect(pad, pad, 2 * (pad + healthbarWidth), 2 * healthbarHeight);
        armourbarActual = new Rect(0, 0, healthbarWidth, healthbarHeight);
        armourbarFull   = new Rect(0, 0, healthbarWidth, healthbarHeight);
        healthbarActual = new Rect(pad + healthbarWidth, 0, healthbarWidth, healthbarHeight);
        healthbarFull   = new Rect(pad + healthbarWidth, 0, healthbarWidth, healthbarHeight);
        
        energyGroup = new Rect(Screen.width - pad - energybarWidth, Screen.height - pad - energybarHeight,
                               energybarWidth, energybarHeight);
        energybarActual = new Rect(0, 0, energybarWidth, energybarHeight);
        energybarFull   = new Rect(0, 0, energybarWidth, energybarHeight);

        weaponGroup = new Rect(Screen.width - 2 * pad - energybarWidth - weaponlabelWidth,
                               Screen.height - pad - weaponLabelHeight - weaponIconHeight,
                               weaponlabelWidth, weaponLabelHeight + weaponIconHeight);
        weaponIcon = new Rect(0, 0, weaponlabelWidth, weaponIconHeight);
        weaponLabel = new Rect(0, weaponIconHeight, weaponlabelWidth, weaponLabelHeight);

        timerGroup = new Rect((Screen.width - timerWidth) / 2, Screen.height - pad - timerHeight,
                              timerWidth, timerHeight);
        timer = new Rect(0, 0, timerWidth, timerHeight);
    }

    void OnGUI() {
        updateValues();

        GUIStyle leftStyle = new GUIStyle();
        GUIStyle rightStyle = new GUIStyle();
        leftStyle.alignment = TextAnchor.UpperLeft;
        leftStyle.font = mainFont;
        leftStyle.fontSize = 12;
        rightStyle.alignment = TextAnchor.UpperRight;
        rightStyle.font = mainFont;
        rightStyle.fontSize = 12;

        GUIStyle weaponLabelStyle = new GUIStyle();
        weaponLabelStyle.font = mainFont;
        weaponLabelStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle timerStyle = new GUIStyle();
        timerStyle.font = timerFont;
        timerStyle.fontSize = 64;
        timerStyle.alignment = TextAnchor.MiddleCenter;

        GUI.BeginGroup(healthGroup); // Health and Armour
            GUI.Box(armourbarFull, "");
            GUI.Box(armourbarActual, "");
            GUI.Box(healthbarFull, "");
            GUI.Box(healthbarActual, "");
            GUI.Label(new Rect(0, healthbarHeight, healthbarWidth - pad, healthbarHeight), "ARMOUR", rightStyle);
            GUI.Label(new Rect(healthbarWidth + 2 * pad, healthbarHeight, healthbarWidth - pad, healthbarHeight), "HEALTH", leftStyle);
        GUI.EndGroup();

        GUI.BeginGroup(energyGroup); // Energy bar
            GUI.Box(energybarFull, "");
            GUI.Box(energybarActual, "");
        GUI.EndGroup();

        GUI.BeginGroup(weaponGroup);
            if (player.weaponTexture.width == 128) {
                weaponIcon.x = 64;  weaponIcon.width = 128;
            } else {
                weaponIcon.x = 0;   weaponIcon.width = 256;
            }
            GUI.DrawTexture(weaponIcon, player.weaponTexture);
            GUI.Label(weaponLabel, player.weaponName, weaponLabelStyle);
        GUI.EndGroup();

        GUI.BeginGroup(timerGroup);
            GUI.Label(timer, countdown.timeLeft().ToString("00.0000"), timerStyle);
        GUI.EndGroup();
    }

    void updateValues() {
        if (player != null) {
            armourbarActual.x = healthbarWidth * (1 - player.armour / 100f);
            armourbarActual.width = healthbarWidth * (player.armour / 100f);
            healthbarActual.width = healthbarWidth * (player.health / 100f);
        }

        if (gun != null) {
            energybarActual.y = energybarHeight * (1 - gun.energy / 100f);
            energybarActual.height = energybarHeight * (gun.energy / 100f);
        }
    }
}

using UnityEngine;
using System.Collections;

public class GUISliders : MonoBehaviour {
    private PlayerController player;
    private Gun gun;
    private int healthbarHeight = 20;
    private int healthbarWidth; // Set automatically w/ respect to screen width
    private int energybarHeight = 200;
    private int energybarWidth = 10;
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

    void Start() {
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
    }

    void OnGUI() {
        updateValues();

        GUIStyle leftStyle = new GUIStyle();
        GUIStyle rightStyle = new GUIStyle();
        leftStyle.alignment = TextAnchor.UpperLeft;
        rightStyle.alignment = TextAnchor.UpperRight;

        GUI.BeginGroup(healthGroup); // Health and Armour
            GUI.Box(armourbarFull, "");
            GUI.Box(armourbarActual, "");
            GUI.Box(healthbarFull, "");
            GUI.Box(healthbarActual, "");
            GUI.Label(new Rect(0, healthbarHeight, healthbarWidth - pad, healthbarHeight), "ARMOUR", rightStyle);
            GUI.Label(new Rect(healthbarWidth + 2 * pad, healthbarHeight, healthbarWidth - pad, healthbarHeight), "HEALTH", leftStyle);
        GUI.EndGroup();

        GUI.BeginGroup(energyGroup);
            GUI.Box(energybarFull, "");
            GUI.Box(energybarActual, "");
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

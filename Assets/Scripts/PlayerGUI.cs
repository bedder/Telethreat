using UnityEngine;
using System.Collections;

public class PlayerGUI : MonoBehaviour {
    public Font timerFont;
    public Font mainFont;
    public Color fontColour = Color.white;
    public Color outlineColour = Color.black;

    public Color healthColour;
    public Color armourColour;
    public Color energyColour;
    private Color healthBackgroundColour;
    private Color armourBackgroundColour;
    private Color energyBackgroundColour;
    private GUIStyle barStyle;
    private Texture2D barTexture;

    private GUIStyle healthArmourStyle;
    private GUIStyle weaponStyle;
    private GUIStyle timerStyle;

    private Goal goal;
    private TeleportCountdown countdown;
    private PlayerController player;
    private Gun gun;
    private GameController gameController;

    private int healthbarHeight = 20;
    private int healthbarWidth; // Set automatically w/ respect to screen width
    private int energybarHeight = 200;
    private int energybarWidth = 20;
    private int weaponlabelWidth = 400;
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
    private Rect weaponLabel;

    private Rect timerGroup;
    private Rect timer;

    private Rect compass;
    private Vector2 compassPivot;
    public Texture2D compassTexture;

    private GUIStyle deathMessageStyle;

    private float displayLevelNumberFor = 2f;
    private float displayLevelNumberUntil;

    void Start() {
        goal = GameObject.FindObjectOfType<Goal>();
        countdown = GameObject.FindObjectOfType<TeleportCountdown>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        gameController = GameObject.FindObjectOfType<GameController>();
        gun = player.gun;

        displayLevelNumberUntil = Time.time + displayLevelNumberFor;

        barStyle = new GUIStyle();
        barTexture = new Texture2D(1, 1);
        barStyle.normal.background = barTexture;
        healthBackgroundColour = new Color(healthColour.r, healthColour.g, healthColour.b, healthColour.a / 2);
        armourBackgroundColour = new Color(armourColour.r, armourColour.g, armourColour.b, armourColour.a / 2);
        energyBackgroundColour = new Color(energyColour.r, energyColour.g, energyColour.b, energyColour.a / 2);

        healthArmourStyle = new GUIStyle();
        healthArmourStyle.font = mainFont;
        healthArmourStyle.fontSize = 12;

        weaponStyle = new GUIStyle();
        weaponStyle.font = mainFont;
        weaponStyle.fontSize = 20;
        weaponStyle.alignment = TextAnchor.MiddleRight;

        timerStyle = new GUIStyle();
        timerStyle.font = timerFont;
        timerStyle.fontSize = 64;
        timerStyle.alignment = TextAnchor.MiddleCenter;

        deathMessageStyle = new GUIStyle();
        deathMessageStyle.font = mainFont;
        deathMessageStyle.fontSize = 80;
        deathMessageStyle.alignment = TextAnchor.MiddleCenter;

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
        weaponLabel = new Rect(0, weaponIconHeight, weaponlabelWidth, weaponLabelHeight);

        timerGroup = new Rect((Screen.width - timerWidth) / 2, Screen.height - pad - timerHeight + 2,
                              timerWidth, timerHeight);
        timer = new Rect(0, 0, timerWidth, timerHeight);

        if (compassTexture != null) {
            float resize = 3;
            compass = new Rect(pad, Screen.height - compassTexture.height / resize - pad, compassTexture.width / 3, compassTexture.height / 3);
            compassPivot = new Vector2(compass.x + (compass.width / 2), compass.y + (compass.height / 2));
        }
    }

    void OnGUI() {
        updateValues();

        if (compassTexture != null && player != null && goal != null) {
            Vector3 delta = goal.transform.position - player.transform.position;
            float angle = Mathf.Atan2(delta.z, delta.x) * 180 / Mathf.PI + 90;
            GUIUtility.RotateAroundPivot(-angle, compassPivot);
            GUI.DrawTexture(compass, compassTexture);
            GUIUtility.RotateAroundPivot(angle, compassPivot);
        }

        GUI.BeginGroup(healthGroup); // Health and Armour
            setBarColour(healthBackgroundColour);
            GUI.Box(healthbarFull, "", barStyle);
            setBarColour(armourBackgroundColour);
            GUI.Box(armourbarFull, "", barStyle);

            setBarColour(healthColour);
            GUI.Box(healthbarActual, "", barStyle);
            setBarColour(armourColour);
            GUI.Box(armourbarActual, "", barStyle);

            healthArmourStyle.alignment = TextAnchor.UpperRight;
            drawText(new Rect(0, healthbarHeight, healthbarWidth - pad, healthbarHeight), "ARMOUR", healthArmourStyle, 1);
            healthArmourStyle.alignment = TextAnchor.UpperLeft;
            drawText(new Rect(healthbarWidth + 2 * pad, healthbarHeight, healthbarWidth - pad, healthbarHeight), "HEALTH", healthArmourStyle, 1);
        GUI.EndGroup();

        GUI.BeginGroup(energyGroup); // Energy bar
            setBarColour(energyBackgroundColour);
            GUI.Box(energybarFull, "", barStyle);
            setBarColour(energyColour);
            GUI.Box(energybarActual, "", barStyle);
        GUI.EndGroup();

        GUI.BeginGroup(weaponGroup);
            drawText(weaponLabel, player.weaponName, weaponStyle, 1);
        GUI.EndGroup();

        GUI.BeginGroup(timerGroup);
            drawText(timer, countdown.timeLeft().ToString("00.0000"), timerStyle, 2);
        GUI.EndGroup();

        if (Time.time < displayLevelNumberUntil) {
            drawText(new Rect(0, 0, Screen.width, Screen.height), "Level " + (gameController.nextLevel - 1), deathMessageStyle, 3);
        }
    }

    void updateValues() {
        if (player != null) {
            armourbarActual.x = healthbarWidth * (1 - player.armour / 100f);
            armourbarActual.width = healthbarWidth * (player.armour / 100f);
            healthbarActual.width = healthbarWidth * (player.health / 100f);
        } else {
            armourbarActual.width = 0;
            healthbarActual.width = 0;
            drawText(new Rect(0, 0, Screen.width, Screen.height), "You died.\nPress R to restart\nor Escape to exit", deathMessageStyle, 4);
            if (Input.GetButton("Restart")) {
                Application.LoadLevel(Application.loadedLevel);
            }
            if (Input.GetButton("Exit")) {
                Application.LoadLevel(0);
            }
        }

        if (gun != null) {
            energybarActual.y = energybarHeight * (1 - gun.energy / 100f);
            energybarActual.height = energybarHeight * (gun.energy / 100f);
        }
    }

    void drawText(Rect area, string text, GUIStyle style, int outline) {
        style.normal.textColor = outlineColour;
        area.x -= outline;
        GUI.Label(area, text, style);
        area.x += 2 * outline;
        GUI.Label(area, text, style);
        area.x -= outline;
        area.y -= outline;
        GUI.Label(area, text, style);
        area.y += 2 * outline;
        GUI.Label(area, text, style);
        area.y -= outline;
        style.normal.textColor = fontColour;
        GUI.Label(area, text, style);
    }

    void setBarColour(Color colour) {
        barTexture.SetPixel(0, 0, colour);
        barTexture.Apply();
        barStyle.normal.background = barTexture;
    }
}

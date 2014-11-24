using UnityEngine;
using System.Collections;

public class GUISliders : MonoBehaviour {
    private PlayerController player;
    private int barHeight = 20;
    private int barWidth;
    private int barPad = 6;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        barWidth = (int)(Screen.width - (3 * barPad)) / 2;
    }

    void OnGUI() {
        float health = player.health / 100f;
        float armour = player.armour / 100f;
        GUIStyle leftStyle = new GUIStyle();
        GUIStyle rightStyle = new GUIStyle();
        leftStyle.alignment = TextAnchor.UpperLeft;
        rightStyle.alignment = TextAnchor.UpperRight;

        GUI.BeginGroup(new Rect(barPad, barPad, 2 * (barPad + barWidth), 2 * barHeight)); // Armour
            GUI.Box(new Rect(0, 0, barWidth, barHeight), "");
            GUI.Box(new Rect(barWidth + barPad, 0, barWidth, barHeight), "");
            GUI.Box(new Rect((barWidth * (1 - armour)), 0, barWidth * armour, barHeight), "");
            GUI.Box(new Rect(barWidth + barPad, 0, barWidth * health, barHeight), "");
            GUI.Label(new Rect(0, barHeight, barWidth - barPad, barHeight), "ARMOUR", rightStyle);
            GUI.Label(new Rect(barWidth + 2 * barPad, barHeight, barWidth - barPad, barHeight), "HEALTH", leftStyle);
        GUI.EndGroup();

        
        
    }
}

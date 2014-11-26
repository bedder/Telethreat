using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
    private int padX = 20;
    private int padY = 5;
    private int buttonWidth = 200;
    private int buttonHeight = 50;
    
    public Texture2D logo;
    private Rect logoLocation;

    private Rect buttonsLocation;
    private Rect playButton;
    private Rect quitButton;

    private bool showClasses = false;
    private Rect classesLocation;
    private Rect lightButton;
    private Rect heavyButton;
    private Rect enginButton;
    private Rect medicButton;

    private int selectedClass;

    private Rect infopaneLocation;
    private Rect startButton;

    private string[] classNames;
    private string[] classDescription;

	void Start () {
        int offset = padY;
        if (logo != null) {
            logoLocation = new Rect((Screen.width - logo.width) / 2, padY, logo.width, logo.height);
            offset += logo.height + padY;
        }

        buttonsLocation = new Rect(padX, offset, buttonWidth, 2 * buttonHeight + padY);
        playButton = new Rect(0, 0, buttonWidth, buttonHeight);
        quitButton = new Rect(0, buttonHeight + padY, buttonWidth, buttonHeight);

        classesLocation = new Rect(2 * padX + buttonWidth, offset, buttonWidth, 4 * buttonHeight + 3 * padY);
        lightButton = new Rect(0, 0, buttonWidth, buttonHeight);
        heavyButton = new Rect(0, 1 * (buttonHeight + padY), buttonWidth, buttonHeight);
        enginButton = new Rect(0, 2 * (buttonHeight + padY), buttonWidth, buttonHeight);
        medicButton = new Rect(0, 3 * (buttonHeight + padY), buttonWidth, buttonHeight);

        infopaneLocation = new Rect(classesLocation.x + classesLocation.width + padX, offset, 0, Screen.height - offset - padY);
        infopaneLocation.width = Screen.width - infopaneLocation.x - padX;
        startButton = new Rect(infopaneLocation.width - buttonWidth, infopaneLocation.height - buttonHeight, buttonWidth, buttonHeight);

        classNames = new string[4] { "Light Infantry", "Heavy Infantry", "Engineer", "Medic" };
        classDescription = new string[4] { "Lorem Ipsum Dolor Sit Amet", "The Quick Brown Fox Jumped Over The Lazy Dog", "Once Upon A Midnight Dreay While I Pondered Weak And Weary", "Twinkle Twinkle Little Star" };
	}
	
    void OnGUI() {
        if (logo != null) {
            GUI.DrawTexture(logoLocation, logo);
        }

        GUI.BeginGroup(buttonsLocation);
            if (GUI.Button(playButton, "Start"))
                showClasses = true;
            if (GUI.Button(quitButton, "Exit"))
                Application.Quit();
        GUI.EndGroup();

        if (showClasses) {
            GUI.BeginGroup(classesLocation);
                if (GUI.Button(lightButton, "Light Infatnry"))
                    selectedClass = 0;
                if (GUI.Button(heavyButton, "Heavy Infantry"))
                    selectedClass = 1;
                if (GUI.Button(enginButton, "Engineer"))
                    selectedClass = 2;
                if (GUI.Button(medicButton, "Medic"))
                    selectedClass = 3;
            GUI.EndGroup();

            GUI.BeginGroup(infopaneLocation);
                GUI.Label(new Rect(0, 0, infopaneLocation.width, 20), classNames[selectedClass]);
                GUI.Label(new Rect(0, 25, infopaneLocation.width, 100), classDescription[selectedClass]);
                if (GUI.Button(startButton, "Start!")) {
                    switch (selectedClass) { // TODO: Work out how to load the level with different classes
                        case 0:
                            Application.LoadLevel(1);
                            break;
                        case 1:
                            Application.LoadLevel(1);
                            break;
                        case 2:
                            Application.LoadLevel(1);
                            break;
                        case 3:
                            Application.LoadLevel(1);
                            break;
                    }
                }
            GUI.EndGroup();
        }
	
	}
}

﻿using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
    public GUIStyle buttonStyle;
    public GUIStyle classNameStyle;
    public GUIStyle classDescriptionStyle;

    private int padX = 20;
    private int padY = 5;
    private int buttonWidth = 100;
    private int classNameWidth = 300;
    private int classDescriptionWidth = 300;
    private int buttonHeight = 20;
    
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
    private Rect className;
    private Rect classDescription;

    private Rect startButton;

    private string[] classNames;
    private string[] classDescriptions;

	void Start () {
        int offset = padY;
        if (logo != null) {
            logoLocation = new Rect((Screen.width - logo.width) / 2, padY, logo.width, logo.height);
            offset += logo.height + padY;
        }

        int requiredSpace = 2 * padX + buttonWidth + classNameWidth + classDescriptionWidth;

        buttonsLocation = new Rect((Screen.width - requiredSpace) / 2, offset, buttonWidth, 2 * buttonHeight + padY);
        playButton = new Rect(0, 0, buttonWidth, buttonHeight);
        quitButton = new Rect(0, buttonHeight + padY, buttonWidth, buttonHeight);

        classesLocation = new Rect(buttonsLocation.x + buttonsLocation.width + padX, offset, classNameWidth, 4 * buttonHeight + 3 * padY);
        lightButton = new Rect(0, 0, classNameWidth, buttonHeight);
        heavyButton = new Rect(0, 1 * (buttonHeight + padY), classNameWidth, buttonHeight);
        enginButton = new Rect(0, 2 * (buttonHeight + padY), classNameWidth, buttonHeight);
        medicButton = new Rect(0, 3 * (buttonHeight + padY), classNameWidth, buttonHeight);

        infopaneLocation = new Rect(classesLocation.x + classesLocation.width + padX, offset, classDescriptionWidth, 75 + padX + buttonHeight);
        className = new Rect(0, 0, infopaneLocation.width, 20);
        classDescription = new Rect(0, 25, infopaneLocation.width, 50);
        startButton = new Rect(infopaneLocation.width - buttonWidth, 75, buttonWidth, buttonHeight);

        classNames = new string[4] { "Light Infantry", "Heavy Infantry", "Engineer", "Medic" };
        classDescriptions = new string[4] { "Lorem Ipsum Dolor Sit Amet", "The Quick Brown Fox Jumped Over The Lazy Dog", "Once Upon A Midnight Dreay While I Pondered Weak And Weary", "Twinkle Twinkle Little Star" };
	}
	
    void OnGUI() {
        if (logo != null) {
            GUI.DrawTexture(logoLocation, logo);
        }

        GUI.BeginGroup(buttonsLocation);
            if (GUI.Button(playButton, "Play", buttonStyle))
                showClasses = true;
            if (GUI.Button(quitButton, "Exit", buttonStyle))
                Application.Quit();
        GUI.EndGroup();

        if (showClasses) {
            GUI.BeginGroup(classesLocation);
                if (GUI.Button(lightButton, classNames[0], buttonStyle))
                    selectedClass = 0;
                if (GUI.Button(heavyButton, classNames[1], buttonStyle))
                    selectedClass = 1;
                if (GUI.Button(enginButton, classNames[2], buttonStyle))
                    selectedClass = 2;
                if (GUI.Button(medicButton, classNames[3], buttonStyle))
                    selectedClass = 3;
            GUI.EndGroup();

            GUI.BeginGroup(infopaneLocation);
                GUI.Label(new Rect(0, 0, infopaneLocation.width, 20), classNames[selectedClass], classNameStyle);
                GUI.Label(new Rect(0, 25, infopaneLocation.width, 100), classDescriptions[selectedClass], classDescriptionStyle);
                if (GUI.Button(startButton, "Start!", buttonStyle)) {
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

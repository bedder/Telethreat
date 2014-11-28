using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
    public GUIStyle buttonStyle;
    public GUIStyle classNameStyle;
    public GUIStyle classDescriptionStyle;

    public GameObject lightInfantryPrefab;
    public GameObject heavyInfantryPrefab;
    public GameObject engineerPrefab;
    public GameObject medicPrefab;
    private GameController gameController;

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
        gameController = GameObject.FindObjectOfType<GameController>();

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
        classDescription = new Rect(0, 25, infopaneLocation.width, 100);
        startButton = new Rect(infopaneLocation.width - buttonWidth, 100, buttonWidth, buttonHeight);

        classNames = new string[4] { "Light Infantry", "Heavy Infantry", "Engineer", "Medic" };
        classDescriptions = new string[4] { "Want to run around? The light infantry might be for you. He's not quite as heavily armoured as the other classes but he does have the ability to sprint. Handy with all these enemies around.",
                                            "Everyone likes explosions, it's a fact of life, but the heavy infantry likes them a little bit more than the average person. Throw grenades without the enrgy cost by pressing the class key (Default: ENTER or F).", "In theory the engineer is able to slow down the teleportation timer, meaning that you can control the flow of the game a bit better. But engineering is hard, and this feature is still in the process of being implemented.", "Medics are great for multiplayer as they allow you to heal your allies. Multiplayer is a stretch-goal on our kickstarter. So in reality the medic is just a worse infantryman." };
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
                GUI.Label(className, classNames[selectedClass], classNameStyle);
                GUI.Label(classDescription, classDescriptions[selectedClass], classDescriptionStyle);
                if (GUI.Button(startButton, "Start!", buttonStyle)) {
                    switch (selectedClass) { // TODO: Work out how to load the level with different classes
                        case 0:
                            gameController.playerController = lightInfantryPrefab;
                            break;
                        case 1:
                            gameController.playerController = heavyInfantryPrefab;
                            break;
                        case 2:
                            gameController.playerController = engineerPrefab;
                            break;
                        case 3:
                            gameController.playerController = medicPrefab;
                            break;
                    }
                    Application.LoadLevel(gameController.nextLevel);
                }
            GUI.EndGroup();
        }
	}
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    public GameObject[] PlayerControllers;
    public GameObject ExitButton;
    public GameObject ClassSelect;
    public Text DescriptionText;

    private int selectedClass;
    private GameController gameController;

	void Start () {
        gameController = GameObject.FindObjectOfType<GameController>();

        if (Application.isWebPlayer)
            ExitButton.SetActive(false);
	}

    public void ShowClasses() {
        ClassSelect.SetActive(true);
    }

    public void SelectClass(int i) {
        selectedClass = i;
        updateDescriptionText();
    }

    public void StartTheGame() {
        gameController.playerController = PlayerControllers[selectedClass];
        Application.LoadLevel(1);
    }

    public void LoadAbout() {
        Application.OpenURL("http://bedder.co.uk/games/telethreat/details/");
    }

    private void updateDescriptionText() {
        switch (selectedClass) {
            case 0:
                DescriptionText.text =
                    "Want to run around? The <color=#FF76FB>light "
                    + "infantry</color> might be for you. He's not quite as "
                    + "heavily armoured as the other classes but he does have "
                    + "the ability to sprint. Handy with all these enemies "
                    + "around.";
                break;
            case 1:
                DescriptionText.text =
                    "Everyone likes explosions, it's a fact of life, but the "
                    + "<color=#FF76FB>heavy infantry</color> likes them a little "
                    + "bit more than the average person. Throw grenades without "
                    + "the energy cost by pressing the class key (Default: "
                    + "ENTER or F).";
                break;
            case 2:
                DescriptionText.text =
                    "In theory the <color=#FF76FB>engineer</color> is able to "
                    + "slow down the teleportation timer, meaning that you can "
                    + "control the flow of the game a bit better. But "
                    + "engineering is hard, and this feature is still in the "
                    + "process of being implemented.";
                break;
            case 3:
                DescriptionText.text =
                    "<color=#FF76FB>Medics</color> are great for multiplayer as "
                    + "they allow you to heal your allies. Multiplayer is a "
                    + "stretch-goal on our kickstarter. So in reality the medic "
                    + "is just a worse infantryman.";
                break;
        }
    }


}

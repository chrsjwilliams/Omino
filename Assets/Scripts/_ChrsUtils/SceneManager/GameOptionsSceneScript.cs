using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOptionsSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;

    private const float SECONDS_TO_WAIT = 0.01f;
    private bool[] humanPlayers;

    private int levelSelected;
    [SerializeField]
    private GameObject levelButtonParent;
    private Button[] levelButtons;
    [SerializeField]
    private Image levelSelectionIndicator;
    [SerializeField]
    private Slider winWeightSlider;
    [SerializeField]
    private Slider structureWeightSlider;
    [SerializeField]
    private Slider blueprintWeightSlider;
    [SerializeField]
    private Button[] joinButtons;
    private Text[] joinButtonTexts;
    private Color[] baseColors;

    private TaskManager _tm = new TaskManager();

    private bool aiOptionsActive;
    [SerializeField]
    private GameObject aiOptionsMenu;

    internal override void OnEnter(TransitionData data)
    {
        if (PlayerPrefs.HasKey("winWeight"))
        {
            winWeightSlider.value = PlayerPrefs.GetFloat("winWeight");
        }
        if (PlayerPrefs.HasKey("structWeight"))
        {
            structureWeightSlider.value = PlayerPrefs.GetFloat("structWeight");
        }
        if (PlayerPrefs.HasKey("blueprintWeight"))
        {
            blueprintWeightSlider.value = PlayerPrefs.GetFloat("blueprintWeight");
        }
        levelButtons = levelButtonParent.GetComponentsInChildren<Button>();
        SelectLevel(0);
        aiOptionsMenu.SetActive(false);
        humanPlayers = new bool[2] { false, false };
        joinButtonTexts = new Text[2] {
            joinButtons[0].GetComponentInChildren<Text>(),
            joinButtons[1].GetComponentInChildren<Text>()
        };
        baseColors = new Color[2] { Services.GameManager.Player1ColorScheme[0],
                        Services.GameManager.Player2ColorScheme[0] };

        for (int i = 0; i < 2; i++)
        {
            joinButtons[i].GetComponent<Image>().color = (baseColors[i] + Color.white) / 2;
        }
    }

    internal override void OnExit()
    {
        PlayerPrefs.Save();
    }

    public void SetWinWeight()
    {
        Services.GameManager.SetWinWeight(winWeightSlider.value);
    }

    public void SetStructureWeight()
    {
        Services.GameManager.SetStructureWeight(structureWeightSlider.value);
    }

    public void SetBlueprintWeight()
    {
        Services.GameManager.SetBlueprintWeight(blueprintWeightSlider.value);
    }

    public void StartGame()
    {
        Services.GameManager.SetUserPreferences(levelSelected);
        _tm.Do
        (
                    new Wait(SECONDS_TO_WAIT))
              .Then(new ActionTask(ChangeScene)
        );
        
    }

    private void ChangeScene()
    {
        Services.GameManager.SetNumPlayers(humanPlayers);
        Services.Scenes.Swap<GameSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
    }

    public void SelectLevel(int levelNum)
    {
        levelSelected = levelNum;
        MoveLevelSelector(levelNum);
    }

    void MoveLevelSelector(int levelNum)
    {
        levelSelectionIndicator.transform.position = 
            levelButtons[levelNum].transform.position;
    }

    public void ToggleAIOptionsMenu()
    {
        aiOptionsActive = !aiOptionsActive;
        aiOptionsMenu.SetActive(aiOptionsActive);
    }

    public void ToggleHumanPlayer(int playerNum)
    {
        int index = playerNum - 1;
        humanPlayers[index] = !humanPlayers[index];
        string colorName = playerNum == 1 ? "Blue" : "Pink";
        if (humanPlayers[index])
        {
            joinButtonTexts[index].text = colorName + " Player \n Human \n Tap to Withdraw";
            joinButtons[index].GetComponent<Image>().color = baseColors[index];
        }
        else
        {
            joinButtonTexts[index].text = colorName + " Player \n CPU \n Tap to Join";
            joinButtons[index].GetComponent<Image>().color = (baseColors[index] + Color.white) / 2;
        }
    }
}

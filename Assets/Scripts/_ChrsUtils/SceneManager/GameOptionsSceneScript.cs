using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private Slider attackWeightSlider;
    [SerializeField]
    private Slider blueprintDestructionWeightSlider;
    [SerializeField]
    private Slider disconnectionWeightSlider;
    [SerializeField]
    private Slider destructorForBlueprintWeightSlider;

    [SerializeField]
    private Button[] joinButtons;
    private TextMeshProUGUI[] joinButtonJoinTexts;
    private TextMeshProUGUI[] joinButtonPlayerTypeTexts;
    [SerializeField]
    private Slider[] aiLevelSliders;
    private TextMeshProUGUI[] aiLevelTexts;
    private Color[] baseColors;
    [SerializeField]
    private float defaultWinWeight;
    [SerializeField]
    private float defaultStructWeight;
    [SerializeField]
    private float defaultBlueprintWeight;
    [SerializeField]
    private float defaultAttackWeight;
    [SerializeField]
    private float defaultBlueprintDestructionWeight;
    [SerializeField]
    private float defaultDisconnectionWeight;
    [SerializeField]
    private float defaultDestructorForBlueprintWeight;

    private float timeElapsed;
    private const float textPulsePeriod = 0.35f;
    private const float textPulseMaxScale = 1.075f;
    private bool pulsingUp = true;

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
        else
        {
            winWeightSlider.value = defaultWinWeight;
        }
        if (PlayerPrefs.HasKey("structWeight"))
        {
            structureWeightSlider.value = PlayerPrefs.GetFloat("structWeight");
        }
        else
        {
            structureWeightSlider.value = defaultStructWeight;
        }
        if (PlayerPrefs.HasKey("blueprintWeight"))
        {
            blueprintWeightSlider.value = PlayerPrefs.GetFloat("blueprintWeight");
        }
        else
        {
            blueprintWeightSlider.value = defaultBlueprintWeight;
        }
        if (PlayerPrefs.HasKey("attackWeight"))
        {
            attackWeightSlider.value = PlayerPrefs.GetFloat("attackWeight");
        }
        else
        {
            attackWeightSlider.value = defaultAttackWeight;
        }
        if (PlayerPrefs.HasKey("blueprintDestructionWeight"))
        {
            blueprintDestructionWeightSlider.value = PlayerPrefs.GetFloat("blueprintDestructionWeight");
        }
        else
        {
            blueprintDestructionWeightSlider.value = defaultBlueprintDestructionWeight;
        }
        if (PlayerPrefs.HasKey("disconnectionWeight"))
        {
            disconnectionWeightSlider.value = PlayerPrefs.GetFloat("disconnectionWeight");
        }
        else
        {
            disconnectionWeightSlider.value = defaultDisconnectionWeight;
        }
        if (PlayerPrefs.HasKey("destructorForBlueprintWeight"))
        {
            destructorForBlueprintWeightSlider.value = PlayerPrefs.GetFloat("destructorForBlueprintWeight");
        }
        else
        {
            destructorForBlueprintWeightSlider.value = defaultDestructorForBlueprintWeight;
        }


        levelButtons = levelButtonParent.GetComponentsInChildren<Button>();
        SelectLevel(0);
        aiOptionsMenu.SetActive(false);
        humanPlayers = new bool[2] { false, false };
        joinButtonPlayerTypeTexts = new TextMeshProUGUI[2] {
            joinButtons[0].GetComponentInChildren<TextMeshProUGUI>(),
            joinButtons[1].GetComponentInChildren<TextMeshProUGUI>()
        };
        joinButtonJoinTexts = new TextMeshProUGUI[2] {
            joinButtons[0].GetComponentsInChildren<TextMeshProUGUI>()[1],
            joinButtons[1].GetComponentsInChildren<TextMeshProUGUI>()[1]
        };
        baseColors = new Color[2] { Services.GameManager.Player1ColorScheme[0],
                        Services.GameManager.Player2ColorScheme[0] };
        aiLevelTexts = new TextMeshProUGUI[2];
        for (int i = 0; i < 2; i++)
        {
            joinButtons[i].GetComponent<Image>().color = (baseColors[i] + Color.white) / 2;
            aiLevelTexts[i] = aiLevelSliders[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        if (Services.GameManager.tutorialMode)
        {
            ToggleHumanPlayer(1);
            for(int i = 0; i < 2; i++)
            {
                aiLevelSliders[i].gameObject.SetActive(false);
            }
            levelButtonParent.SetActive(false);
            levelSelectionIndicator.gameObject.SetActive(false);
            levelSelected = 4;
            joinButtons[1].gameObject.SetActive(false);
            aiLevelSliders[1].gameObject.SetActive(false);
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

    public void SetAttackWeight()
    {
        Services.GameManager.SetAttackWeight(attackWeightSlider.value);
    }

    public void SetBlueprintDestructionWeight()
    {
        Services.GameManager.SetBlueprintDestructionWeight(blueprintDestructionWeightSlider.value);
    }

    public void SetDisconnectionWeight()
    {
        Services.GameManager.SetDisconnectionWeight(disconnectionWeightSlider.value);
    }

    public void SetDestructorForBlueprintWeight()
    {
        Services.GameManager.SetDestructorForBlueprintWeight(destructorForBlueprintWeightSlider.value);
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
        if (pulsingUp)
        {
            timeElapsed += Time.deltaTime;
        }
        else
        {
            timeElapsed -= Time.deltaTime;
        }
        if (timeElapsed >= textPulsePeriod)
        {
            pulsingUp = false;
        }
        if(timeElapsed <= 0)
        {
            pulsingUp = true;
        }
        for (int i = 0; i < joinButtonJoinTexts.Length; i++)
        {
            if (!humanPlayers[i] && !Services.GameManager.tutorialMode)
            {
                joinButtonJoinTexts[i].transform.localScale =
                    Vector3.Lerp(Vector3.one, textPulseMaxScale * Vector3.one,
                    EasingEquations.Easing.QuadEaseOut(timeElapsed / textPulsePeriod));
                //joinButtonJoinTexts[i].color = Color.Lerp(new Color(1,1,1,0.8f), Color.white,
                //    EasingEquations.Easing.QuadEaseOut(timeElapsed / textPulsePeriod));

            }
            else
            {
                joinButtonJoinTexts[i].transform.localScale = Vector3.one;
            }
        }
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
        if (Services.GameManager.tutorialMode) humanPlayers[index] = true;
        else humanPlayers[index] = !humanPlayers[index];

        if (humanPlayers[index])
        {
            joinButtonPlayerTypeTexts[index].text = "Human\n ";
            joinButtonJoinTexts[index].text = "\nTap to Withdraw";
            joinButtons[index].GetComponent<Image>().color = baseColors[index];
            joinButtonJoinTexts[index].color = Color.white;
            joinButtonPlayerTypeTexts[index].color = Color.white;
            aiLevelSliders[index].gameObject.SetActive(false);
        }
        else if (!Services.GameManager.tutorialMode)
        {
            joinButtonPlayerTypeTexts[index].text = "CPU\n ";
            joinButtonJoinTexts[index].text = "\nTap to Join";
            joinButtons[index].GetComponent<Image>().color = (baseColors[index] + Color.white) / 2;
            joinButtonJoinTexts[index].color = Color.black;
            joinButtonPlayerTypeTexts[index].color = Color.black;
            aiLevelSliders[index].gameObject.SetActive(true);
        }
    }

    public void SetAILevel(int playerNum)
    {
        int level = Services.GameManager.tutorialMode? 
                        1 : Mathf.RoundToInt(aiLevelSliders[playerNum - 1].value);
        Services.GameManager.aiLevels[playerNum - 1] = level;
        aiLevelTexts[playerNum - 1].text = "AI Level: " + level;
    }
}

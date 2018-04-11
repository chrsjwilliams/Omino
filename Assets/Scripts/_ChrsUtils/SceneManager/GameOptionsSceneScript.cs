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
    private Button[] joinButtons;
    private TextMeshProUGUI[] joinButtonJoinTexts;
    [SerializeField]
    private GameObject[] aiLevelButtonZones;
    private Button[][] aiLevelButtons;
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

    internal override void OnEnter(TransitionData data)
    {
        Services.GameManager.SetWinWeight(defaultWinWeight);
        Services.GameManager.SetStructureWeight(defaultStructWeight);
        Services.GameManager.SetBlueprintWeight(defaultBlueprintWeight);
        Services.GameManager.SetAttackWeight(defaultAttackWeight);
        Services.GameManager.SetBlueprintDestructionWeight(defaultBlueprintDestructionWeight);
        Services.GameManager.SetDisconnectionWeight(defaultDisconnectionWeight);
        Services.GameManager.SetDestructorForBlueprintWeight(defaultDestructorForBlueprintWeight);
        
        levelButtons = levelButtonParent.GetComponentsInChildren<Button>();
        levelButtonParent.SetActive(false);
        humanPlayers = new bool[2] { false, false };
        joinButtonJoinTexts = new TextMeshProUGUI[2] {
            joinButtons[0].GetComponentInChildren<TextMeshProUGUI>(),
            joinButtons[1].GetComponentInChildren<TextMeshProUGUI>()
        };
        baseColors = new Color[2] { Services.GameManager.Player1ColorScheme[0],
                        Services.GameManager.Player2ColorScheme[0] };
        aiLevelTexts = new TextMeshProUGUI[2];
        aiLevelButtons = new Button[2][];

        for (int i = 0; i < 2; i++)
        {
            joinButtons[i].GetComponent<Image>().color = (baseColors[i] + Color.white) / 2;
            Button[] buttons = aiLevelButtonZones[i].GetComponentsInChildren<Button>();
            aiLevelButtons[i] = new Button[buttons.Length];
            for (int j = 0; j < buttons.Length; j++)
            {
                aiLevelButtons[i][j] = buttons[j];
            }
            aiLevelTexts[i] = aiLevelButtonZones[i].GetComponentInChildren<TextMeshProUGUI>();
            aiLevelButtonZones[i].SetActive(false);
        }

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.TwoPlayers:
                StartTwoPlayerMode();
                break;
            case TitleSceneScript.GameMode.PlayerVsAI:
                StartPlayerVsAIMode();
                break;
            case TitleSceneScript.GameMode.Demo:
                StartDemoMode();
                break;
            case TitleSceneScript.GameMode.Tutorial:
                StartTutorialMode();
                break;
            default:
                break;
        }
    }

    internal override void OnExit()
    {
     //   PlayerPrefs.Save();
    }

    private void StartPlayerVsAIMode()
    {
        for (int i = 0; i < joinButtons.Length; i++)
        {
            joinButtons[i].gameObject.SetActive(false);
        }
        levelButtonParent.SetActive(true);
        levelSelectionIndicator.gameObject.SetActive(true);
        SideChooseEntrance entrance = new SideChooseEntrance(joinButtons, false);
        Services.GeneralTaskManager.Do(entrance);
    }

    private void StartTutorialMode()
    {
        levelSelected = 4;
        StartPlayerVsAIMode();
    }

    private void StartTwoPlayerMode()
    {
        for (int i = 0; i < 2; i++)
        {
            joinButtons[i].gameObject.SetActive(false);
            humanPlayers[i] = true;
        }
        SlideInLevelButtons();
    }

    private void StartDemoMode()
    {
        for (int i = 0; i < 2; i++)
        {
            joinButtons[i].gameObject.SetActive(false);
            humanPlayers[i] = false;
            Services.GameManager.aiLevels[i] = AIPlayer.AiLevels[2];
        }
        SlideInLevelButtons();
    }

    private void SlideInLevelButtons()
    {
        if (humanPlayers[0] && !humanPlayers[1])
        {
            levelButtonParent.transform.eulerAngles = new Vector3(0, 0, -90);
        }
        else if (!humanPlayers[0] && humanPlayers[1])
        {
            levelButtonParent.transform.eulerAngles = new Vector3(0, 0, 90);
        }
        levelButtonParent.SetActive(true);
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].gameObject.SetActive(false);
        }
        levelSelectionIndicator.gameObject.SetActive(false);
        GameObject levelSelectText = 
            levelButtonParent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        levelSelectText.SetActive(false);
        LevelSelectTextEntrance entrance = 
            new LevelSelectTextEntrance(levelSelectText);
        LevelSelectButtonEntranceTask buttonEntrance =
            new LevelSelectButtonEntranceTask(levelButtons);
        //entrance.Then(buttonEntrance);
        Services.GeneralTaskManager.Do(entrance);
        Services.GeneralTaskManager.Do(buttonEntrance);
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
        levelSelectionIndicator.gameObject.SetActive(true);
        levelSelected = levelNum;
        MoveLevelSelector(levelNum);
        StartGame();
    }

    void MoveLevelSelector(int levelNum)
    {
        levelSelectionIndicator.transform.position = 
            levelButtons[levelNum].transform.position;
    }

    public void ToggleHumanPlayer(int playerNum)
    {
        int index = playerNum - 1;
        humanPlayers[index] = true;

        joinButtonJoinTexts[index].text = "SIDE CHOSEN";
        joinButtons[index].GetComponent<Image>().color = baseColors[index];
        joinButtonJoinTexts[index].color = Color.white;

        SideChooseEntrance exit = new SideChooseEntrance(joinButtons, true);
        for (int i = 0; i < joinButtons.Length; i++)
        {
            joinButtons[i].enabled = false;
        }
        if (Services.GameManager.mode == TitleSceneScript.GameMode.PlayerVsAI)
        {
            exit.Then(new AILevelSlideIn(aiLevelTexts[index], aiLevelButtons[index],
                playerNum == 1, false));
        }
        else
        {
            exit.Then(new ActionTask(StartGame));
        }
        Services.GeneralTaskManager.Do(exit);
    }

    public void SetP1AILevel(int level)
    {
        SetAILevel(1, level);
    }

    public void SetP2AILevel(int level)
    {
        SetAILevel(2, level);
    }

    private void SetAILevel(int playerNum, int level)
    {
        Services.GameManager.aiLevels[playerNum - 1] = AIPlayer.AiLevels[level];
        AILevelSlideIn slideOut = new AILevelSlideIn(aiLevelTexts[playerNum % 2],
            aiLevelButtons[playerNum % 2], playerNum != 1, true);
        slideOut.Then(new ActionTask(SlideInLevelButtons));
        Services.GeneralTaskManager.Do(slideOut);
    }

    public void ReturnToTitle()
    {
        Services.Scenes.Swap<TitleSceneScript>();
    }
}

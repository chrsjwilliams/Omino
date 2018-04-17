using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class GameOptionsSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;

    private const float SECONDS_TO_WAIT = 0.01f;
    private bool[] humanPlayers;

    public static string progressFileName
    {
        get {
            return Application.persistentDataPath + Path.DirectorySeparatorChar +
              "progress.txt";
        }
    }

    private Level levelSelected;
    [SerializeField]
    private GameObject levelButtonParent;
    private LevelButton[] levelButtons;
    [SerializeField]
    private GameObject campaignLevelButtonParent;
    private LevelButton[] campaignLevelButtons;
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
        
        levelButtons = levelButtonParent.GetComponentsInChildren<LevelButton>();
        levelButtonParent.SetActive(false);
        campaignLevelButtons = campaignLevelButtonParent.GetComponentsInChildren<LevelButton>();
        campaignLevelButtonParent.SetActive(false);
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
            case TitleSceneScript.GameMode.Campaign:
                StartCampaignMode();
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
        SideChooseEntrance entrance = new SideChooseEntrance(joinButtons, false);
        Services.GeneralTaskManager.Do(entrance);
    }

    private void SetLevelProgress(int progress)
    {
        for (int i = 0; i < campaignLevelButtons.Length; i++)
        {
            LevelButton button = campaignLevelButtons[i].GetComponent<LevelButton>();
            if (i > progress)
            {
                button.unlocked = false;
                button.gameObject.SetActive(false);
            }
            else if (i <= progress)
            {
                button.unlocked = true;
                button.gameObject.SetActive(true);
                button.GetComponentsInChildren<Image>()[1].enabled = i < progress;
            }
        }
    }

    public void UnlockAllLevels()
    {
        SetLevelProgress(4);
        File.WriteAllText(GameOptionsSceneScript.progressFileName,"4");
    }

    public void LockAllLevels()
    {
        SetLevelProgress(0);
        File.WriteAllText(GameOptionsSceneScript.progressFileName, "0");
    }

    private void StartCampaignMode()
    {
        int progress = 0;
        if (File.Exists(progressFileName))
        {
            string fileText = File.ReadAllText(progressFileName);
            int.TryParse(fileText, out progress);
        }
        SetLevelProgress(progress);
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
        GameObject buttonParent =
            Services.GameManager.mode == TitleSceneScript.GameMode.Campaign ?
             campaignLevelButtonParent : levelButtonParent;
        LevelButton[] buttons =
            Services.GameManager.mode == TitleSceneScript.GameMode.Campaign ?
            campaignLevelButtons : levelButtons;

        if (humanPlayers[0] && !humanPlayers[1])
        {
            buttonParent.transform.eulerAngles = new Vector3(0, 0, -90);
        }
        else if (!humanPlayers[0] && humanPlayers[1])
        {
            buttonParent.transform.eulerAngles = new Vector3(0, 0, 90);
        }
        buttonParent.SetActive(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }
        levelSelectionIndicator.gameObject.SetActive(false);
        GameObject levelSelectText = 
            buttonParent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        levelSelectText.SetActive(false);
        LevelSelectTextEntrance entrance = 
            new LevelSelectTextEntrance(levelSelectText);
        LevelSelectButtonEntranceTask buttonEntrance =
            new LevelSelectButtonEntranceTask(buttons);
        //entrance.Then(buttonEntrance);
        Services.GeneralTaskManager.Do(entrance);
        Services.GeneralTaskManager.Do(buttonEntrance);
    }

    public void StartGame()
    {
        Services.GameManager.SetUserPreferences(levelSelected);
        Task changeScene = new WaitUnscaled(0.01f);
        changeScene.Then(new ActionTask(ChangeScene));

        _tm.Do(changeScene);
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
            if (!humanPlayers[i])
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

    public void SelectLevel(LevelButton levelButton)
    {
        levelSelectionIndicator.gameObject.SetActive(true);
        levelSelected = levelButton.level;
        levelSelectionIndicator.transform.position = levelButton.transform.position;
        StartGame();
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
            //exit.Then(new ActionTask(StartGame));
            exit.Then(new ActionTask(SlideInLevelButtons));
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

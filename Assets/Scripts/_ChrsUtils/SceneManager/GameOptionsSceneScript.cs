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
    public bool[] humanPlayers { get; private set; }

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
    private LevelButton[] backButton;
    [SerializeField]
    private GameObject playButton;
    [SerializeField]
    private GameObject[] aiLevelButtonZones;
    private Button[][] aiLevelButtons;
    private TextMeshProUGUI[] aiLevelTexts;
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
    [SerializeField]
    private float defaultDangerWeight;

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
        Services.GameManager.SetDangerWeight(defaultDangerWeight);

        backButton[0].gameObject.SetActive(false);
        levelButtons = levelButtonParent.GetComponentsInChildren<LevelButton>();
        levelButtonParent.SetActive(false);
        campaignLevelButtons = campaignLevelButtonParent.GetComponentsInChildren<LevelButton>();
        campaignLevelButtonParent.SetActive(false);
        humanPlayers = new bool[2] { false, false };
        aiLevelTexts = new TextMeshProUGUI[1];
        aiLevelButtons = new Button[1][];

        for (int i = 0; i < aiLevelButtonZones.Length; i++)
        {
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
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        TaskTree aiLevelSelect = new TaskTree( new EmptyTask(),
            new TaskTree(new AILevelSlideIn(aiLevelTexts[0], aiLevelButtons[0], true, false)), 
            new TaskTree(new LevelSelectButtonEntranceTask(backButton)));
        Services.GeneralTaskManager.Do(aiLevelSelect);
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
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        int progress = 0;
        if (File.Exists(progressFileName))
        {
            string fileText = File.ReadAllText(progressFileName);
            int.TryParse(fileText, out progress);
        }
        SetLevelProgress(progress);
        SlideInLevelButtons();
        _tm.Do(new LevelSelectButtonEntranceTask(backButton));
    }

    private void StartTwoPlayerMode()
    {
        for (int i = 0; i < 2; i++)
        {
            humanPlayers[i] = true;
        }
        SlideInLevelButtons();
        _tm.Do(new LevelSelectButtonEntranceTask(backButton));
    }

    private void StartDemoMode()
    {
        for (int i = 0; i < 2; i++)
        {
            humanPlayers[i] = false;
            Services.GameManager.aiLevels[i] = AIPlayer.AiLevels[2];
        }
        SlideInLevelButtons();
        _tm.Do(new LevelSelectButtonEntranceTask(backButton));
    }

    private void RemoveOpposingPlayerMenuText(LevelButton[] buttons)
    {
        if (buttons == campaignLevelButtons) return;
        
        for (int i =0; i < buttons.Length; i++)
        {
            TextMeshProUGUI[] buttonText = buttons[i].GetComponentsInChildren<TextMeshProUGUI>();
            for (int k = 0; k < buttonText.Length; k++)
            {
                if(buttonText[k].name.Contains("2") &&
                    humanPlayers[0] != humanPlayers[1])
                {
                    buttonText[k].gameObject.SetActive(false);
                }
            }
        }
    }

    private void SlideInLevelButtons()
    {
        GameObject buttonParent =
            Services.GameManager.mode == TitleSceneScript.GameMode.Campaign ?
             campaignLevelButtonParent : levelButtonParent;
        LevelButton[] buttons =
            Services.GameManager.mode == TitleSceneScript.GameMode.Campaign ?
            campaignLevelButtons : levelButtons;

        buttonParent.transform.eulerAngles = new Vector3(0, 0, 0);
        RemoveOpposingPlayerMenuText(buttons);
        if (humanPlayers[0] && !humanPlayers[1])
        {
            buttonParent.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (!humanPlayers[0] && humanPlayers[1])
        {
            buttonParent.transform.eulerAngles = new Vector3(0, 0, 180);
        }
        buttonParent.SetActive(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }
        playButton.SetActive(false);
        //levelSelectionIndicator.gameObject.SetActive(true);
        GameObject levelSelectText = 
            buttonParent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        levelSelectText.SetActive(false);
        LevelSelectTextEntrance entrance = 
            new LevelSelectTextEntrance(levelSelectText);
        LevelSelectButtonEntranceTask buttonEntrance =
            new LevelSelectButtonEntranceTask(buttons, playButton);
        _tm.Do(entrance);
        _tm.Do(buttonEntrance);
    }

    public void StartGame()
    {
        Services.GameManager.SetCurrentLevel(levelSelected);
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
    }

    public void SelectLevel(LevelButton levelButton)
    {
        //levelSelectionIndicator.gameObject.SetActive(true);
        levelSelected = levelButton.level;
        //levelSelectionIndicator.transform.position = levelButton.transform.position;
        StartGame();
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
    
    public void UIClick()
    {
        Services.AudioManager.CreateTempAudio(Services.Clips.UIClick, 0.55f);
    }
    
    public void UIButtonPressedSound()
    {
        Services.AudioManager.CreateTempAudio(Services.Clips.UIButtonPressed, 0.55f);
    }
}

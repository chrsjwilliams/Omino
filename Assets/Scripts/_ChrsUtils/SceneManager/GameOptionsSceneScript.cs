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
    private GameObject dungeonRunMenu;
    private LevelButton[] dungeonRunLevelButtons;
    [SerializeField]
    private GameObject techSelectMenu; 

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

    [SerializeField]
    private bool optionsMenuActive;
    [SerializeField]
    private GameObject optionButtonParent;
    [SerializeField]
    private LevelButton[] theOptionButton;
    [SerializeField]
    private GameObject optionMenu;

    [SerializeField]
    private GameObject[] optionButtons;

    [SerializeField]
    private HandicapSystem handicapSystem;
    [SerializeField]
    private GameObject handicapOptions;

    [SerializeField]
    private Button musicButton;
    [SerializeField]
    private Button soundFXButton;
    [SerializeField]
    private Button blueprintAssistButton;

    private bool selectingTech;

    [SerializeField]
    private Button[] techPowerUpsToChoose;
    [SerializeField]
    private GameObject[] currentDungeonRunTechZone;
    //private List<Structure> currentDungeonRunTech;
    private TextMeshProUGUI[] currentTechText;
    private Button[][] dungeonRunTechMenu;
    [SerializeField]
    private EloUIManager eloUI;

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

        dungeonRunMenu.SetActive(false);

        techPowerUpsToChoose = techSelectMenu.GetComponentsInChildren<Button>();
        techSelectMenu.SetActive(false);

        humanPlayers = new bool[2] { false, false };
        aiLevelTexts = new TextMeshProUGUI[1];
        aiLevelButtons = new Button[1][];

        optionsMenuActive = false;
        optionMenu.SetActive(false);
        //optionButtonParent.SetActive(true);
        TurnOnOptionButtons(false);
        eloUI.gameObject.SetActive(false);

        currentTechText = new TextMeshProUGUI[1];
        dungeonRunTechMenu = new Button[1][];
        selectingTech = false;

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

        for (int i = 0; i < currentDungeonRunTechZone.Length; i++)
        {
            Button[] buttons = currentDungeonRunTechZone[i].GetComponentsInChildren<Button>();
            dungeonRunTechMenu[i] = new Button[buttons.Length];
            for(int j = 0; j < buttons.Length; j++)
            {
                dungeonRunTechMenu[i][j] = buttons[j];
            }

            currentTechText[i] = currentDungeonRunTechZone[i].GetComponentInChildren<TextMeshProUGUI>();
            currentDungeonRunTechZone[i].SetActive(false);
        }

        handicapSystem.Init();

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.TwoPlayers:
                StartTwoPlayerMode();
                break;
            case TitleSceneScript.GameMode.PlayerVsAI:
                if (Services.GameManager.eloTrackingMode)
                    StartEloMode();
                else
                    StartPlayerVsAIMode();
                break;
            case TitleSceneScript.GameMode.Demo:
                StartDemoMode();
                break;
            case TitleSceneScript.GameMode.Campaign:
                StartCampaignMode();
                break;
            case TitleSceneScript.GameMode.DungeonRun:
                StartDungeonRunMode();
                break;
            default:
                break;
        }


    }

    internal override void OnExit()
    {
        //Services.GameManager.SetHandicapType(handicapSystem.useBlueprintHandicap);
        Services.GameManager.SetHandicapValues(handicapSystem.handicapValues);

     //   PlayerPrefs.Save();
    }


    private void  TurnOnOptionButtons(bool isOn)
    {
        foreach(GameObject button in optionButtons)
        {
            button.SetActive(isOn);
        }
    }

    private void StartPlayerVsAIMode()
    {
        optionButtonParent.SetActive(false);
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        TaskTree aiLevelSelect = new TaskTree( new EmptyTask(),
            new TaskTree(new AILevelSlideIn(aiLevelTexts[0], aiLevelButtons[0], true, false)), 
            new TaskTree(new LevelSelectButtonEntranceTask(backButton)),
            new TaskTree(new SlideInOptionsMenuTask(handicapOptions)));
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
    }

    private void StartTwoPlayerMode()
    {
        TurnOnOptionButtons(true);
        for (int i = 0; i < 2; i++)
        {
            humanPlayers[i] = true;
        }
        SlideInLevelButtons();      
    }

    private void StartDungeonRunMode()
    {
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        int dungeonRunProgress = 0;
        int completedDungeonRuns = 0;
        if (File.Exists(progressFileName))
        {
            string fileText = File.ReadAllText(progressFileName);
            int.TryParse(fileText, out dungeonRunProgress);
            int.TryParse(fileText, out completedDungeonRuns);
        }
        
        TaskTree dungeonRunChallengeSelect = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(dungeonRunMenu)),
            new TaskTree(new AILevelSlideIn(currentTechText[0], dungeonRunTechMenu[0], true, false)),
            new TaskTree(new LevelSelectButtonEntranceTask(backButton)));
            //new TaskTree(new SlideInOptionsMenuTask(handicapOptions)));
        Services.GeneralTaskManager.Do(dungeonRunChallengeSelect);

    }

    private void SetDungeonRunProgress(int progress)
    {
        //  Change Challenge Level Text
        //  Retrieve all saved tech from runs in progress
    }

    private void SetCompletedDungeonRunProgress(int completedRuns)
    {
        //  this would be nice
    }

    private void StartEloMode()
    {
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        eloUI.gameObject.SetActive(true);
        eloUI.SetUI(ELOManager.eloData);
        Services.GameManager.aiLevels[1] = AILEVEL.HARD;
        TurnOnOptionButtons(true);
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
        GameObject buttonParent;
        LevelButton[] buttons;

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.Campaign:
                buttonParent = campaignLevelButtonParent;
                buttons = campaignLevelButtons;
                break;
            default:
                buttonParent = levelButtonParent;
                buttons = levelButtons;
                break;
        }

        

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
        LevelSelectButtonEntranceTask backButtonEntrance =
                new LevelSelectButtonEntranceTask(backButton);

        _tm.Do(entrance);        
        _tm.Do(buttonEntrance);
        _tm.Do(backButtonEntrance);
        
        
        SlideOutOptionsButton(false);
    }

    public void SlideOutOptionsButton(bool slideOut)
    {
        LevelSelectButtonEntranceTask slideOptionButtonTask =
                new LevelSelectButtonEntranceTask(theOptionButton, null, slideOut);
        _tm.Do(slideOptionButtonTask);
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
        TurnOffOptionMenu();
        TurnOnOptionButtons(true);
        Services.GameManager.aiLevels[playerNum - 1] = AIPlayer.AiLevels[level];
        AILevelSlideIn slideOut = new AILevelSlideIn(aiLevelTexts[playerNum % 2],
            aiLevelButtons[playerNum % 2], playerNum != 1, true);
        slideOut.Then(new ActionTask(SlideInLevelButtons));
        Services.GeneralTaskManager.Do(slideOut);
    }

    public void TurnOffOptionMenu()
    {
        optionMenu.SetActive(false);
    }

    public void ToggleOptionMenu()
    {

        optionsMenuActive = !optionsMenuActive;
        //optionButtonParent.SetActive(optionsMenuActive);
        levelButtonParent.SetActive(false);
        optionMenu.SetActive(optionsMenuActive);
        if (optionsMenuActive)
        {
            //_BlueprintAssistAppearanceToggle();
            //_MusicButtonAppearanceToggle();
            //_SFXButtonAppearanceToggle();
            SlideOutOptionsButton(true);

        }
    }

    public void Back()
    {
        if (optionsMenuActive)
        {
            ToggleOptionMenu();
            SlideInLevelButtons();
        }
        else
        {
            Services.Scenes.Swap<TitleSceneScript>();
        }
    }
    
    public void UIClick()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 0.55f);
    }
    
    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }

    private void SetOptionButtonStatus(Button button, bool status)
    {
        button.GetComponent<Image>().color = status ?
            Services.GameManager.Player2ColorScheme[0] :
            Services.GameManager.Player2ColorScheme[1];
        TextMeshProUGUI textMesh = button.GetComponentInChildren<TextMeshProUGUI>();
        string textContent = textMesh.text;
        string[] textSplit = textContent.Split('<', '>');
        if (textSplit.Length > 1)
        {
            for (int i = 0; i < textSplit.Length; i++)
            {
                if (textSplit[i] == "s")
                {
                    textContent = textSplit[i + 1];
                    break;
                }
            }
        }
        if (!status)
        {
            textContent = "<s>" + textContent + "</s>";
        }

        textMesh.text = textContent;

    }

    public void ToggleBlueprintAssist()
    {
        Services.GameManager.ToggleBlueprintAssist();
        SetOptionButtonStatus(blueprintAssistButton, Services.GameManager.BlueprintAssistEnabled);
        
    }

    public void ToggleMusic()
    {
        Services.AudioManager.ToggleMusic();
        SetOptionButtonStatus(musicButton, Services.GameManager.MusicEnabled);
        
    }

    public void ToggleSoundFX()
    {
        Services.AudioManager.ToggleSoundEffects();
        SetOptionButtonStatus(soundFXButton, Services.GameManager.SoundEffectsEnabled);
        
    }

    
    private void _BlueprintAssistAppearanceToggle()
    {
        GameObject button = GameObject.Find("ToggleBlueprintAssist");

        if (Services.GameManager.BlueprintAssistEnabled)
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[0];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Blueprint Assist";

        }
        else
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>Blueprint Assist</s>";
        }
    }

    private void _MusicButtonAppearanceToggle()
    {
        GameObject button = GameObject.Find("ToggleMusic");

        if (Services.GameManager.MusicEnabled)
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[0];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Music";

        }
        else
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>Music</s>";
        }
    }

    private void _SFXButtonAppearanceToggle()
    {
        GameObject button = GameObject.Find("ToggleSound");

        if (Services.GameManager.SoundEffectsEnabled)
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[0];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Sound FX";

        }
        else
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>Sound FX</s>";
        }
    }
}

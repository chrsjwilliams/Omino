using System;
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
    private Button backButton;
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
    private GameObject optionMenu;

    [SerializeField]
    private GameObject[] optionButtons;
    [SerializeField]
    private Button theOptionButton;

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

    //  Dungeon Run Challenge Menu
    [SerializeField]
    private TextMeshProUGUI dungeonRun_ChallengeLevelText;
    [SerializeField]
    private TextMeshProUGUI dungeonRun_CurrentTechZone;
    private Button[][] dungeonRun_CurrentTechMenu;
    [SerializeField]
    private Button dungeonRun_StartChallengeButton;

    //  Dungeon Run Tech Select Menu
    [SerializeField]
    private GameObject techSelect_TechSelectZone;
    private TextMeshProUGUI techSelect_SelectTechText;
    private Button[][] techSelect_MenuButtons;
    [SerializeField]
    private Image[] techSelect_currentUIIcons;
    private Image[][] techSelect_currentTechIcons;
    

    [SerializeField]
    private EloUIManager eloUI;

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

        backButton.gameObject.SetActive(true);

        levelButtons = levelButtonParent.GetComponentsInChildren<LevelButton>();
        levelButtonParent.SetActive(false);
        campaignLevelButtons = campaignLevelButtonParent.GetComponentsInChildren<LevelButton>();
        campaignLevelButtonParent.SetActive(false);

        dungeonRunMenu.SetActive(false);

        techSelectMenu.SetActive(false);

        humanPlayers = new bool[2] { false, false };
        aiLevelTexts = new TextMeshProUGUI[1];
        aiLevelButtons = new Button[1][];

        optionsMenuActive = false;
        optionMenu.SetActive(false);
        TurnOnOptionButtons(true);
        eloUI.gameObject.SetActive(false);

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
            case TitleSceneScript.GameMode.Practice:
                StartPlayerVsAIMode();
                break;
            case TitleSceneScript.GameMode.Demo:
                StartDemoMode();
                break;
            case TitleSceneScript.GameMode.Tutorial:
                StartCampaignMode();
                break;
            case TitleSceneScript.GameMode.DungeonRun:
                StartDungeonRunMode();
                break;
            case TitleSceneScript.GameMode.Elo:
                StartEloMode();
                break;
            default:
                break;
        }

        HandicapSystem.Init();
    }

    private void SetUpDungeonRunTechSelectMenu()
    {
        techSelect_MenuButtons = new Button[1][];

        Button[] techSelectButtons = techSelect_TechSelectZone.GetComponentsInChildren<Button>();
        techSelect_MenuButtons[0] = new Button[techSelectButtons.Length];

        List<BuildingType> techToChooseFrom = DungeonRunManager.GetTechBuildingSelection();

        techSelect_currentTechIcons = new Image[1][];

        Image[] currentTechIcons = techSelect_currentUIIcons;
        techSelect_currentTechIcons[0] = new Image[currentTechIcons.Length];

        

        for(int i = 0; i < currentTechIcons.Length; i++)
        {
            techSelect_currentTechIcons[0][i] = currentTechIcons[i];
            techSelect_currentTechIcons[0][i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = "None";
            techSelect_currentTechIcons[0][i].GetComponentsInChildren<Image>()[1].color = Services.GameManager.NeutralColor;
        }

        for (int i = 0; i < DungeonRunManager.dungeonRunData.currentTech.Count; i++)
        {
            BuildingType selectedType = DungeonRunManager.dungeonRunData.currentTech[i];
            TechBuilding tech = DungeonRunManager.GetBuildingFromType(selectedType);
            techSelect_currentTechIcons[0][i].GetComponent<Image>().color = Services.GameManager.Player1ColorScheme[0];
            techSelect_currentTechIcons[0][i].GetComponentsInChildren<Image>()[1].color = Color.white;
            techSelect_currentTechIcons[0][i].GetComponentInChildren<TextMeshProUGUI>().text = tech.GetName();
            techSelect_currentTechIcons[0][i].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(tech.buildingType);
        }

        

        for (int j = 0; j < techToChooseFrom.Count; j++)
        {

            BuildingType selectedType = techToChooseFrom[j];
            
            techSelect_MenuButtons[0][j] = techSelectButtons[j];

            techSelect_MenuButtons[0][j].GetComponentsInChildren<TextMeshProUGUI>()[0].text = DungeonRunManager.GetBuildingFromType(selectedType).GetName();
            techSelect_MenuButtons[0][j].GetComponentsInChildren<TextMeshProUGUI>()[1].text = DungeonRunManager.GetBuildingFromType(selectedType).GetDescription();

            techSelect_MenuButtons[0][j].GetComponent<Image>().color = Services.GameManager.NeutralColor;
            techSelect_MenuButtons[0][j].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(techToChooseFrom[j]);
        }

        techSelect_SelectTechText = techSelect_TechSelectZone.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void SetUpDungeonRunChallengeMenu()
    {

        dungeonRun_CurrentTechMenu = new Button[1][];

        Button[] techButtons = dungeonRun_CurrentTechZone.GetComponentsInChildren<Button>();
        dungeonRun_CurrentTechMenu[0] = new Button[techButtons.Length];

        for (int j = 0; j < techButtons.Length; j++)
        {
            dungeonRun_CurrentTechMenu[0][j] = techButtons[j];

            dungeonRun_CurrentTechMenu[0][j].interactable = false;
            dungeonRun_CurrentTechMenu[0][j].GetComponent<Image>().color = Services.GameManager.NeutralColor;
            dungeonRun_CurrentTechMenu[0][j].GetComponentInChildren<TextMeshProUGUI>().text = "None";
            dungeonRun_CurrentTechMenu[0][j].GetComponentsInChildren<Image>()[1].color = Services.GameManager.NeutralColor;
            dungeonRun_CurrentTechMenu[0][j].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(BuildingType.NONE);
        }

        for (int i = 0; i < DungeonRunManager.dungeonRunData.currentTech.Count; i++)
        {
            BuildingType selectedType = DungeonRunManager.dungeonRunData.currentTech[i];
            TechBuilding tech = DungeonRunManager.GetBuildingFromType(selectedType);
            dungeonRun_CurrentTechMenu[0][i].interactable = true;
            dungeonRun_CurrentTechMenu[0][i].GetComponent<Image>().color = Services.GameManager.Player1ColorScheme[0];
            dungeonRun_CurrentTechMenu[0][i].GetComponentsInChildren<Image>()[1].color = Color.white;
            dungeonRun_CurrentTechMenu[0][i].GetComponentInChildren<TextMeshProUGUI>().text = tech.GetName();
            dungeonRun_CurrentTechMenu[0][i].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(tech.buildingType);
        }
    }

    internal override void OnExit()
    {
        //Services.GameManager.SetHandicapType(handicapSystem.useBlueprintHandicap);
        Services.GameManager.SetHandicapValues(HandicapSystem.handicapValues);

     //   PlayerPrefs.Save();
    }


    private void  TurnOnOptionButtons(bool isOn)
    {
        foreach(GameObject button in optionButtons)
        {
            button.SetActive(isOn);
        }
    }

    private void TurnOnHandicapOptions(bool isOn)
    {
        handicapOptions.SetActive(isOn);
    }

    private void StartPlayerVsAIMode()
    {
        optionButtonParent.SetActive(false);
        TurnOnOptionButtons(false);
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        TaskTree aiLevelSelect = new TaskTree( new EmptyTask(),
            new TaskTree(new AILevelSlideIn(aiLevelTexts[0], aiLevelButtons[0], true, false)), 
            //new TaskTree(new LevelSelectButtonEntranceTask(backButton)),
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

        TurnOnHandicapOptions(false);

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
        TurnOnHandicapOptions(false);

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
        TurnOnOptionButtons(true);

        if (DungeonRunManager.dungeonRunData.selectingNewTech)
        {
            SetUpDungeonRunTechSelectMenu();
            TaskTree techSelectMenuTasks = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(techSelectMenu)),
                new TaskTree(new AILevelSlideIn(techSelect_SelectTechText, techSelect_MenuButtons[0], true, false)),
                new TaskTree(new LevelSelectButtonEntranceTask(backButton)));
            Services.GeneralTaskManager.Do(techSelectMenuTasks);
        }
        else
        {
            SetDungeonRunProgress(DungeonRunManager.dungeonRunData.challenegeNum);
            SetUpDungeonRunChallengeMenu();
            TaskTree dungeonRunChallengeSelect = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(dungeonRunMenu)),
                new TaskTree(new AILevelSlideIn(dungeonRun_CurrentTechZone, dungeonRun_CurrentTechMenu[0], true, false)),
                new TaskTree(new LevelSelectButtonEntranceTask(backButton)));
            Services.GeneralTaskManager.Do(dungeonRunChallengeSelect);
        }
       

    }

    private void SetDungeonRunProgress(int progress)
    {
        if (DungeonRunManager.dungeonRunData.completedRun)
        {
            dungeonRun_ChallengeLevelText.text = "Run Complete";
            dungeonRun_StartChallengeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Try Again";
        }
        else
        {
            dungeonRun_StartChallengeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Challenge";
           dungeonRun_ChallengeLevelText.text = "Challenge " + progress + "/" + DungeonRunManager.MAX_DUNGEON_CHALLENGES;
        }
    }

    private void SetCompletedDungeonRunProgress(int completedRuns)
    {
        //  this would be nice
    }

    private void StartEloMode()
    {
        TurnOnHandicapOptions(false);
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        eloUI.gameObject.SetActive(true);
        eloUI.SetUI(ELOManager.eloData);
        Services.GameManager.aiLevels[1] = AILEVEL.HARD;
        TurnOnOptionButtons(true);
        //_tm.Do(new LevelSelectButtonEntranceTask(backButton));
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
            case TitleSceneScript.GameMode.Tutorial:
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
        //_tm.Do(backButtonEntrance);
        
        
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
        if(DungeonRunManager.dungeonRunData.completedRun)
        {
            DungeonRunManager.ResetDungeonRun();
        }

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

    public void DungeonRunSelectTech(TextMeshProUGUI buildingType)
    {
        BuildingType selectedType = BuildingType.NONE;
        string key = buildingType.text.Replace(" ", "");
        key = key.ToUpper();
        foreach (var type in Enum.GetValues(typeof(BuildingType)))
        {
            if(key == type.ToString())
            {
                selectedType = (BuildingType)type;
            }
        }
        DungeonRunManager.AddSelectedTech(selectedType);

        TaskTree slideOutTechSelectMenuTasks = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(techSelectMenu, true)),
                new TaskTree(new AILevelSlideIn(techSelect_SelectTechText, techSelect_MenuButtons[0], true, true)));
        Services.GeneralTaskManager.Do(slideOutTechSelectMenuTasks);
        Services.GeneralTaskManager.Do(new ActionTask(StartDungeonRunMode));
    }

    public void ResetDungeonRun()
    {
        if (DungeonRunManager.dungeonRunData.selectingNewTech)
        {
            TaskTree slideOutTechSelectMenuTasks = new TaskTree(new EmptyTask(),
                    new TaskTree(new LevelSelectTextEntrance(techSelectMenu, true)),
                    new TaskTree(new AILevelSlideIn(techSelect_SelectTechText, techSelect_MenuButtons[0], true, true)));
            Services.GeneralTaskManager.Do(slideOutTechSelectMenuTasks);  
        }

        DungeonRunManager.ResetDungeonRun();
        Services.GeneralTaskManager.Do(new ActionTask(StartDungeonRunMode));
    }

    public void TurnOffOptionMenu()
    {
        optionMenu.SetActive(false);
    }

    public void ToggleOptionMenu()
    {
        optionsMenuActive = !optionsMenuActive;
       // optionButtonParent.SetActive(false);
        levelButtonParent.SetActive(false);
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
        {
            eloUI.gameObject.SetActive(!optionsMenuActive);
        }
        if(Services.GameManager.mode == TitleSceneScript.GameMode.Tutorial)
        {
            campaignLevelButtonParent.SetActive(!optionsMenuActive);
        }
        if (Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            if (DungeonRunManager.dungeonRunData.selectingNewTech)
            {
                techSelectMenu.SetActive(!optionsMenuActive);
            }
            else
            {
                dungeonRunMenu.SetActive(!optionsMenuActive);
            }
        }
        optionMenu.SetActive(optionsMenuActive);
        if (optionsMenuActive)
        {
            SlideOutOptionsButton(true);
        }
    }

    public void Back()
    {
        if (optionsMenuActive)
        {
            ToggleOptionMenu();
			if (Services.GameManager.mode == TitleSceneScript.GameMode.TwoPlayers ||
			            Services.GameManager.mode == TitleSceneScript.GameMode.Practice ||
			            Services.GameManager.mode == TitleSceneScript.GameMode.Demo) {
				SlideInLevelButtons ();
			} else {
				SlideOutOptionsButton (false);
			}
			if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
			{
				eloUI.gameObject.SetActive(true);
			}
        }
        else
        {
            Services.GameManager.mode = TitleSceneScript.GameMode.NONE;

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
}

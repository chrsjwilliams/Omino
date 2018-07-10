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
    private TextMeshProUGUI challengeLevelText;
    [SerializeField]
    private Button[] techPowerUpsToChoose;
    [SerializeField]
    private TextMeshProUGUI currentDungeonRunTechZone;
    [SerializeField]
    private GameObject selectTechZone;
    private TextMeshProUGUI selectTechText;
    private TextMeshProUGUI currentTechText;
    private Button[][] dungeonRunTechMenu;
    private Button[][] techSelectMenuButtons;
    [SerializeField]
    private Image[] techSelectMenuCurrentTechSprites;
    [SerializeField]
    private TextMeshProUGUI currentTechTechSelectZone;
    [SerializeField]
    private Image[] techSelectMenuOwnedTechIcons;
    private Image[][] currentTechIconMenu;
    [SerializeField]
    private Button startChallengeButton;

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

        techPowerUpsToChoose = techSelectMenu.GetComponentsInChildren<Button>();
        techSelectMenu.SetActive(false);

        humanPlayers = new bool[2] { false, false };
        aiLevelTexts = new TextMeshProUGUI[1];
        aiLevelButtons = new Button[1][];

        optionsMenuActive = false;
        optionMenu.SetActive(false);
        TurnOnOptionButtons(true);
        eloUI.gameObject.SetActive(false);

        selectingTech = true;

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
        //DungeonRunManager.dungeonRunData.selectingNewTech = true;

                
        handicapSystem.Init();

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
            case TitleSceneScript.GameMode.DungeonRun:
                StartDungeonRunMode();
                break;
            case TitleSceneScript.GameMode.Elo:
                StartEloMode();
                break;
            default:
                break;
        }


    }

    private void SetUpDungeonRunTechSelectMenu()
    {
        techSelectMenuButtons = new Button[1][];

        Button[] techSelectButtons = selectTechZone.GetComponentsInChildren<Button>();
        techSelectMenuButtons[0] = new Button[techSelectButtons.Length];

        List<BuildingType> techToChooseFrom = DungeonRunManager.GetTechBuildingSelection();

        currentTechIconMenu = new Image[1][];

        Image[] currentTechIcons = techSelectMenuOwnedTechIcons;
        currentTechIconMenu[0] = new Image[currentTechIcons.Length];

        

        for(int i = 0; i < currentTechIcons.Length; i++)
        {
            currentTechIconMenu[0][i] = currentTechIcons[i];
            Debug.Log(currentTechIconMenu[0][i].name);
            currentTechIconMenu[0][i].GetComponentInChildren<TextMeshProUGUI>().text = "None";
            currentTechIconMenu[0][i].GetComponentsInChildren<Image>()[1].color = Services.GameManager.NeutralColor;
        }

        for (int i = 0; i < DungeonRunManager.dungeonRunData.currentTech.Count; i++)
        {
            BuildingType selectedType = DungeonRunManager.dungeonRunData.currentTech[i];
            TechBuilding tech = DungeonRunManager.GetBuildingFromType(selectedType);
            currentTechIconMenu[0][i].GetComponent<Image>().color = Services.GameManager.Player1ColorScheme[0];
            currentTechIconMenu[0][i].GetComponentsInChildren<Image>()[1].color = Color.white;
            currentTechIconMenu[0][i].GetComponentInChildren<TextMeshProUGUI>().text = tech.label;
            currentTechIconMenu[0][i].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(tech.buildingType);
        }

        for (int j = 0; j < techToChooseFrom.Count; j++)
        {

            BuildingType selectedType = techToChooseFrom[j];
            techSelectMenuButtons[0][j] = techSelectButtons[j];

            techSelectMenuButtons[0][j].GetComponentInChildren<TextMeshProUGUI>().text = DungeonRunManager.GetBuildingFromType(selectedType).label;
            techSelectMenuButtons[0][j].GetComponent<Image>().color = Services.GameManager.NeutralColor;
            techSelectMenuButtons[0][j].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(techToChooseFrom[j]);
        }

        selectTechText = selectTechZone.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void SetUpDungeonRunChallengeMenu()
    {

        dungeonRunTechMenu = new Button[1][];

        Button[] techButtons = currentDungeonRunTechZone.GetComponentsInChildren<Button>();
        dungeonRunTechMenu[0] = new Button[techButtons.Length];

        for (int j = 0; j < techButtons.Length; j++)
        {
            dungeonRunTechMenu[0][j] = techButtons[j];

            dungeonRunTechMenu[0][j].interactable = false;
            dungeonRunTechMenu[0][j].GetComponentInChildren<TextMeshProUGUI>().text = "None";
            dungeonRunTechMenu[0][j].GetComponentsInChildren<Image>()[1].color = Services.GameManager.NeutralColor;
        }

        for (int i = 0; i < DungeonRunManager.dungeonRunData.currentTech.Count; i++)
        {
            BuildingType selectedType = DungeonRunManager.dungeonRunData.currentTech[i];
            TechBuilding tech = DungeonRunManager.GetBuildingFromType(selectedType);
            dungeonRunTechMenu[0][i].interactable = true;
            dungeonRunTechMenu[0][i].GetComponent<Image>().color = Services.GameManager.Player1ColorScheme[0];
            dungeonRunTechMenu[0][i].GetComponentsInChildren<Image>()[1].color = Color.white;
            dungeonRunTechMenu[0][i].GetComponentInChildren<TextMeshProUGUI>().text = tech.label;
            dungeonRunTechMenu[0][i].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(tech.buildingType);
        }
        currentTechText = currentDungeonRunTechZone.GetComponent<TextMeshProUGUI>();
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
        TurnOnOptionButtons(true);

        if (DungeonRunManager.dungeonRunData.selectingNewTech)
        {
            SetUpDungeonRunTechSelectMenu();
            TaskTree techSelectMenuTasks = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(techSelectMenu)),
                new TaskTree(new AILevelSlideIn(selectTechText, techSelectMenuButtons[0], true, false)),
                new TaskTree(new LevelSelectButtonEntranceTask(backButton)));
            Services.GeneralTaskManager.Do(techSelectMenuTasks);
        }
        else
        {
            SetDungeonRunProgress(DungeonRunManager.dungeonRunData.challenegeNum);
            SetUpDungeonRunChallengeMenu();
            TaskTree dungeonRunChallengeSelect = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(dungeonRunMenu)),
                new TaskTree(new AILevelSlideIn(currentDungeonRunTechZone, dungeonRunTechMenu[0], true, false)),
                new TaskTree(new LevelSelectButtonEntranceTask(backButton)));
            Services.GeneralTaskManager.Do(dungeonRunChallengeSelect);
        }
       

    }

    private void SetDungeonRunProgress(int progress)
    {
        if (DungeonRunManager.dungeonRunData.completedRun)
        {
            challengeLevelText.text = "Run Complete";
            startChallengeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Try Again";
        }
        else
        {
            startChallengeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Challenge";
           challengeLevelText.text = "Challenge " + progress + "/" + DungeonRunManager.MAX_DUNGEON_CHALLENGES;
        }
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
        //_tm.Do(backButtonEntrance);
        
        
        SlideOutOptionsButton(false);
    }

    public void SlideOutOptionsButton(bool slideOut)
    {
        //LevelSelectButtonEntranceTask slideOptionButtonTask =
        //        new LevelSelectButtonEntranceTask(theOptionButton, null, slideOut);
        //_tm.Do(slideOptionButtonTask);
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
        //TechBuilding selectedTech = DungeonRunManager.GetBuildingFromType(selectedType);
        DungeonRunManager.AddSelectedTech(selectedType);

        TaskTree slideOutTechSelectMenuTasks = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(techSelectMenu, true)),
                new TaskTree(new AILevelSlideIn(selectTechText, techSelectMenuButtons[0], false, true)));
        Services.GeneralTaskManager.Do(slideOutTechSelectMenuTasks);
        StartDungeonRunMode();
    }

    public void ResetDungeonRun()
    {
        DungeonRunManager.ResetDungeonRun();
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
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
        {
            eloUI.gameObject.SetActive(!optionsMenuActive);
        }
        if(Services.GameManager.mode == TitleSceneScript.GameMode.Campaign)
        {
            campaignLevelButtonParent.SetActive(!optionsMenuActive);
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
            if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
            {
                eloUI.gameObject.SetActive(true);
            }
            else
            {
                SlideInLevelButtons();
            }
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
}

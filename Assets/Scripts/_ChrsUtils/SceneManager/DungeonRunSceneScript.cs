using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class DungeonRunSceneScript : Scene<TransitionData>
{
    public bool[] humanPlayers { get; private set; }
    public static string progressFileName
    {
        get
        {
            return Application.persistentDataPath + Path.DirectorySeparatorChar +
              "progress.txt";
        }
    }

    [SerializeField]
    private GameObject dungeonRunMenu;

    //  Dungeon Run Challenge Menu
    [SerializeField]
    private TextMeshProUGUI challengeLevelText;
    [SerializeField]
    private TextMeshProUGUI currentTechZone;
    [SerializeField]
    private TextMeshProUGUI streakCounter;
    [SerializeField]
    private TextMeshProUGUI completeionCounter;
    private Button[][] currentTechMenu;
    [SerializeField]
    private Button startChallengeButton;

    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;

    private TaskManager _tm = new TaskManager();

    // Use this for initialization
    void Start()
    {

    }

    internal override void OnEnter(TransitionData data)
    {
        Services.CameraController.SetPosition(new Vector3(9.5f, 9.5f, -10));
        dungeonRunMenu.SetActive(false);
        backButton.SetActive(false);
        optionButton.SetActive(false);

        int dungeonRunProgress = 0;
        int completedDungeonRuns = 0;
        if (File.Exists(progressFileName))
        {
            string fileText = File.ReadAllText(progressFileName);
            int.TryParse(fileText, out dungeonRunProgress);
            int.TryParse(fileText, out completedDungeonRuns);
        }
        streakCounter.text = "streak: " + DungeonRunManager.dungeonRunStreak;
        completeionCounter.text = "completions: " + DungeonRunManager.totalCompletions;
        humanPlayers = new bool[2] { false, false };
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        SetDungeonRunProgress(DungeonRunManager.dungeonRunData.challengeNum);
        SetUpDungeonRunChallengeMenu();
        TaskTree dungeonRunChallengeSelect = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(dungeonRunMenu)),
            new TaskTree(new AILevelSlideIn(currentTechZone, currentTechMenu[0], true, false)),
            new TaskTree(new LevelSelectTextEntrance(backButton,true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton)));
        _tm.Do(dungeonRunChallengeSelect);

    }

    internal override void OnExit()
    {
        Services.GameManager.SetHandicapValues(HandicapSystem.handicapValues);
    }

    internal override void ExitTransition()
    {
        TaskTree dungeonRunExit = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(dungeonRunMenu, false, true)),
            new TaskTree(new AILevelSlideIn(currentTechZone, currentTechMenu[0], true, true)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton, false, true)));
        _tm.Do(dungeonRunExit);
    }

    private void SetDungeonRunProgress(int progress)
    {
        if (DungeonRunManager.dungeonRunData.completedRun)
        {
            challengeLevelText.text = "run complete";
            startChallengeButton.GetComponentInChildren<TextMeshProUGUI>().text = "try again";
        }
        else
        {
            startChallengeButton.GetComponentInChildren<TextMeshProUGUI>().text = "start challenge";
            challengeLevelText.text = "challenge " + progress + "/" + DungeonRunManager.MAX_DUNGEON_CHALLENGES;
        }
    }

    private void SetUpDungeonRunChallengeMenu()
    {

        currentTechMenu = new Button[1][];

        Button[] techButtons = currentTechZone.GetComponentsInChildren<Button>();
        currentTechMenu[0] = new Button[techButtons.Length];

        for (int j = 0; j < techButtons.Length; j++)
        {
            currentTechMenu[0][j] = techButtons[j];

            currentTechMenu[0][j].interactable = false;
            currentTechMenu[0][j].GetComponent<Image>().color = Services.GameManager.NeutralColor;
            currentTechMenu[0][j].GetComponentInChildren<TextMeshProUGUI>().text = "none";
            currentTechMenu[0][j].GetComponentsInChildren<Image>()[1].color = Services.GameManager.NeutralColor;
            currentTechMenu[0][j].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(BuildingType.NONE);
        }

        for (int i = 0; i < DungeonRunManager.dungeonRunData.currentTech.Count; i++)
        {
            BuildingType selectedType = DungeonRunManager.dungeonRunData.currentTech[i];
            TechBuilding tech = TechBuilding.GetBuildingFromType(selectedType);
            Button button = currentTechMenu[0][i];
            button.interactable = true;
            Image buttonSubImage = button.GetComponentsInChildren<Image>()[1];
            button.GetComponent<Image>().color = Services.GameManager.Player1ColorScheme[0];
            buttonSubImage.color = Color.white;
            string techName = "";
            if (tech != null) techName = tech.GetName().ToLower();
            button.GetComponentInChildren<TextMeshProUGUI>().text = techName;
            buttonSubImage.sprite = Services.TechDataLibrary.GetIcon(selectedType);
        }
    }

    public void StartGame()
    {
        if (DungeonRunManager.dungeonRunData.completedRun)
        {
            DungeonRunManager.ResetDungeonRun();
        }

        Services.GameManager.SetCurrentLevel(null);
        Task changeScene = new WaitUnscaled(0.01f);
        changeScene.Then(new ActionTask(ChangeScene));

        _tm.Do(changeScene);
    }

    private void ChangeScene()
    {
        Services.GameManager.SetNumPlayers(humanPlayers);
        Services.Scenes.Swap<GameSceneScript>();
    }

    public void UIClick()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 0.55f);
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }

    // Update is called once per frame
    void Update()
    {
        _tm.Update();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechSelectSceneScript : Scene<TransitionData>
{
    [SerializeField]
    private GameObject techSelectMenu;

    //  Dungeon Run Tech Select Menu
    [SerializeField]
    private GameObject techSelectZone;
    private TextMeshProUGUI selectTechText;
    private Button[][] menuButtons;
    [SerializeField]
    private Image[] currentUIIcons;
    private Image[][] currentTechIcons;

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
        SetUpDungeonRunTechSelectMenu();
        TaskTree techSelectMenuTasks = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(techSelectMenu)),
            new TaskTree(new AILevelSlideIn(selectTechText, menuButtons[0], true, false)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton)));
        _tm.Do(techSelectMenuTasks);
    }

    internal override void OnExit()
    {
    }

    internal override void ExitTransition()
    {
        TaskTree techSelectMenuExit = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(techSelectMenu, false, true)),
            new TaskTree(new AILevelSlideIn(selectTechText, menuButtons[0], true, true)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton, false, true)));
        _tm.Do(techSelectMenuExit);
    }

    private void SetUpDungeonRunTechSelectMenu()
    {
        menuButtons = new Button[1][];

        Button[] techSelectButtons = techSelectZone.GetComponentsInChildren<Button>();
        menuButtons[0] = new Button[techSelectButtons.Length];

        List<BuildingType> techToChooseFrom = DungeonRunManager.GetTechBuildingSelection();

        this.currentTechIcons = new Image[1][];

        Image[] currentTechIcons = currentUIIcons;
        this.currentTechIcons[0] = new Image[currentTechIcons.Length];



        for (int i = 0; i < currentTechIcons.Length; i++)
        {
            this.currentTechIcons[0][i] = currentTechIcons[i];
            this.currentTechIcons[0][i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = "None";
            this.currentTechIcons[0][i].GetComponentsInChildren<Image>()[1].color = Services.GameManager.NeutralColor;
        }

        for (int i = 0; i < DungeonRunManager.dungeonRunData.currentTech.Count; i++)
        {
            BuildingType selectedType = DungeonRunManager.dungeonRunData.currentTech[i];
            TechBuilding tech = DungeonRunManager.GetBuildingFromType(selectedType);
            this.currentTechIcons[0][i].GetComponent<Image>().color = Services.GameManager.Player1ColorScheme[0];
            this.currentTechIcons[0][i].GetComponentsInChildren<Image>()[1].color = Color.white;
            this.currentTechIcons[0][i].GetComponentInChildren<TextMeshProUGUI>().text = tech.GetName();
            this.currentTechIcons[0][i].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(tech.buildingType);
        }



        for (int j = 0; j < techToChooseFrom.Count; j++)
        {

            BuildingType selectedType = techToChooseFrom[j];

            menuButtons[0][j] = techSelectButtons[j];

            menuButtons[0][j].GetComponentsInChildren<TextMeshProUGUI>()[0].text = DungeonRunManager.GetBuildingFromType(selectedType).GetName();
            menuButtons[0][j].GetComponentsInChildren<TextMeshProUGUI>()[1].text = DungeonRunManager.GetBuildingFromType(selectedType).GetDescription();

            menuButtons[0][j].GetComponent<Image>().color = Services.GameManager.NeutralColor;
            menuButtons[0][j].GetComponentsInChildren<Image>()[1].sprite = Services.TechDataLibrary.GetIcon(techToChooseFrom[j]);
        }

        selectTechText = techSelectZone.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SelectTech(TextMeshProUGUI buildingType)
    {
        BuildingType selectedType = BuildingType.NONE;
        string key = buildingType.text.Replace(" ", "");
        key = key.ToUpper();
        foreach (var type in Enum.GetValues(typeof(BuildingType)))
        {
            if (key == type.ToString())
            {
                selectedType = (BuildingType)type;
            }
        }
        DungeonRunManager.AddSelectedTech(selectedType);

        TaskTree slideOutTechSelectMenuTasks = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(techSelectMenu, true)),
                new TaskTree(new AILevelSlideIn(selectTechText, menuButtons[0], true, true)));
        Services.GeneralTaskManager.Do(slideOutTechSelectMenuTasks);
        //Services.GeneralTaskManager.Do(new ActionTask(StartDungeonRunMode));
        
        //  Then transition to Dungeon Run Challenge Menu
    }

    // Update is called once per frame
    void Update()
    {
        _tm.Update();
    }
}

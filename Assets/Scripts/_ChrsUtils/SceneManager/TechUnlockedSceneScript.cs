using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechUnlockedSceneScript : Scene<TransitionData>
{
    [SerializeField]
    private GameObject newTechScreen;

    [SerializeField]
    private TextMeshProUGUI newTechName;
    [SerializeField]
    private Image newTechIcon;
    [SerializeField]
    private TextMeshProUGUI newTechTooltip;


    [SerializeField]
    private GameObject continueButton;


    private TaskManager _tm = new TaskManager();

    // Use this for initialization
    void Start()
    {

    }

    internal override void OnEnter(TransitionData data)
    {
        TaskTree techSelectMenuTasks = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(newTechScreen)),
            new TaskTree(new LevelSelectTextEntrance(continueButton)));
        _tm.Do(techSelectMenuTasks);
    }

    internal override void OnExit()
    {
    }

    internal override void ExitTransition()
    {
        TaskTree techSelectMenuExit = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(newTechScreen, false, true)),
            new TaskTree(new LevelSelectTextEntrance(continueButton, false, true)));
        _tm.Do(techSelectMenuExit);
    }

    public void SetNewUnlockedTech(TechBuilding tech)
    {
        newTechName.text = tech.GetName();
        newTechIcon.sprite = Services.TechDataLibrary.GetIcon(tech.buildingType);
        newTechTooltip.text = tech.GetDescription();

    }


    // Update is called once per frame
    void Update()
    {
        _tm.Update();
    }
}

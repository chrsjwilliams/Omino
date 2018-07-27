using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EloSceneScript : Scene<TransitionData>
{
    public bool[] humanPlayers { get; private set; }
    private Level levelSelected;

    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;

    [SerializeField]
    private EloUIManager eloUI;

    private TaskManager _tm = new TaskManager();

    // Use this for initialization
    void Start()
    {

    }

    internal override void OnEnter(TransitionData data)
    {
        humanPlayers = new bool[2] { false, false };
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        eloUI.gameObject.SetActive(false);
        eloUI.SetUI(ELOManager.eloData);
        Services.GameManager.aiLevels[1] = AILEVEL.HARD;

        TaskTree eloEntrance = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(eloUI.gameObject)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton)));
        _tm.Do(eloEntrance);
    }

    internal override void OnExit()
    {
        Services.GameManager.SetHandicapValues(HandicapSystem.handicapValues);
    }

    internal override void ExitTransition()
    {
        TaskTree eloxit = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(eloUI.gameObject, false, true)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton, false, true)));
        _tm.Do(eloxit);
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

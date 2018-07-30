using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSelectSceneScript : Scene<TransitionData>
{
    public bool[] humanPlayers { get; private set; }

    private Level levelSelected;
    [SerializeField]
    private GameObject levelButtonParent;
    private LevelButton[] levelButtons;
    [SerializeField]
    private GameObject playButton;

    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;

    private TaskManager _tm = new TaskManager();


    internal override void OnEnter(TransitionData data)
    {
        levelButtons = levelButtonParent.GetComponentsInChildren<LevelButton>();
        levelButtonParent.SetActive(false);
        humanPlayers = new bool[2];

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.TwoPlayers:
                for (int i = 0; i < 2; i++)
                {
                    humanPlayers[i] = true;
                }
                break;
            case TitleSceneScript.GameMode.Demo:
                for (int i = 0; i < 2; i++)
                {
                    humanPlayers[i] = false;
                }
                break;
            default:
                humanPlayers[0] = true;
                humanPlayers[1] = false;
                break;
        }

        levelButtonParent.transform.eulerAngles = new Vector3(0, 0, 0);
        RemoveOpposingPlayerMenuText(levelButtons);
        if (humanPlayers[0] && !humanPlayers[1])
        {
            levelButtonParent.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (!humanPlayers[0] && humanPlayers[1])
        {
            levelButtonParent.transform.eulerAngles = new Vector3(0, 0, 180);
        }
        levelButtonParent.SetActive(true);
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].gameObject.SetActive(false);
        }

        GameObject levelSelectText =
            levelButtonParent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        levelSelectText.SetActive(false);
        playButton.SetActive(false);

        TaskTree mapSelectEntrance = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(levelSelectText, true)),
            new TaskTree(new LevelSelectButtonEntranceTask(levelButtons,playButton)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton)));
        _tm.Do(mapSelectEntrance);

        
    }

    internal override void OnExit()
    {
        Services.GameManager.SetHandicapValues(HandicapSystem.handicapValues);

    }

    internal override void ExitTransition()
    {
        GameObject levelSelectText =
            levelButtonParent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        levelSelectText.SetActive(false);

        TaskTree mapSelectExit = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(levelSelectText,false, true)),
            new TaskTree(new LevelSelectButtonEntranceTask(levelButtons, null, true)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton, false, true)));

        _tm.Do(mapSelectExit);
    }

    public void SelectLevel(LevelButton levelButton)
    {
        //levelSelectionIndicator.gameObject.SetActive(true);
        levelSelected = levelButton.level;
        //levelSelectionIndicator.transform.position = levelButton.transform.position;
        StartGame();
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

    private void RemoveOpposingPlayerMenuText(LevelButton[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            TextMeshProUGUI[] buttonText = buttons[i].GetComponentsInChildren<TextMeshProUGUI>();
            for (int k = 0; k < buttonText.Length; k++)
            {
                if (buttonText[k].name.Contains("2") &&
                    humanPlayers[0] != humanPlayers[1])
                {
                    buttonText[k].gameObject.SetActive(false);
                }
            }
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

    // Update is called once per frame
    void Update () {
        _tm.Update();
	}
}

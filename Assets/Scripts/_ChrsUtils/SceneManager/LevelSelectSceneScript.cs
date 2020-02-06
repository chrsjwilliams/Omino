using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectSceneScript : Scene<TransitionData>
{
    private bool levelsLoaded = false;
    public bool[] humanPlayers { get; private set; }

    private Level levelSelected;
    [SerializeField]
    private GameObject levelButtonParent;
    public GameObject levelButtonPrefaB;
    private LevelButton[] levelButtons;
    [SerializeField]
    private GameObject playButton;
    [SerializeField]
    private GameObject scrollPanel;
    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;
    [SerializeField]
    private GameObject editDeleteMenu;
    [SerializeField]
    private PressAndHoldButton deleteButton;

    private int levelButtonSpacing = 300;
    private Vector2 unselectedButtonScale = new Vector2(0.33f, 0.33f);
    private Vector2 selectedButtonScale = new Vector2(0.66f, 0.66f);

    private TaskManager _tm = new TaskManager();


    internal override void OnEnter(TransitionData data)
    {
        if (levelsLoaded) return;
        levelsLoaded = true;
        Services.GameEventManager.Register<RefreshLevelSelectSceneEvent>(OnLevelSelectSceneRefresh);
        levelButtonParent.SetActive(false);
        backButton.SetActive(false);
        optionButton.SetActive(false);
        editDeleteMenu.SetActive(false);

        if (Services.GameManager.mode == TitleSceneScript.GameMode.Edit)
        {
            levelButtons = new LevelButton[LevelManager.levelInfo.customLevels.Count + 1];
            if (LevelManager.levelInfo.customLevels.Count < LevelManager.TOTAL_NUM_OF_CUSTOM_MAPS)
            {
                levelButtons[0] = Instantiate(levelButtonPrefaB, scrollPanel.transform).GetComponent<LevelButton>();
                levelButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                levelButtons[0].GetComponent<RectTransform>().localScale = selectedButtonScale;
                levelButtons[0].name = "create new";
                levelButtons[0].GetComponent<Image>().sprite = Services.LevelDataLibrary.GetLevelImage(levelButtons[0].name);
                foreach (TextMeshProUGUI text in levelButtons[0].GetComponentsInChildren<TextMeshProUGUI>())
                {
                    text.text = levelButtons[0].name.ToLower();
                }

                int levelButtonIndex = 1;
                foreach (LevelData levelData in LevelManager.levelInfo.customLevels)
                {
                    levelButtons[levelButtonIndex] = Instantiate(levelButtonPrefaB, scrollPanel.transform).GetComponent<LevelButton>();
                    levelButtons[levelButtonIndex].level = levelData.CreateLevel();
                    levelButtons[levelButtonIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(levelButtonSpacing * levelButtonIndex, 0);
                    levelButtons[levelButtonIndex].GetComponent<RectTransform>().localScale = unselectedButtonScale;
                    levelButtons[levelButtonIndex].name = levelData.levelName;
                    levelButtons[levelButtonIndex].GetComponent<Image>().sprite = Services.LevelDataLibrary.GetLevelImage(levelButtons[levelButtonIndex].name);
                    levelButtons[levelButtonIndex].GetComponent<Image>().color = new Color(160f / 256f, 160f / 256f, 160f / 256f);

                    foreach (TextMeshProUGUI text in levelButtons[levelButtonIndex].GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        text.text = levelButtons[levelButtonIndex].name.ToLower();
                    }

                    levelButtonIndex++;
                }
            }
            else
            {
                levelButtons = new LevelButton[LevelManager.levelInfo.customLevels.Count];
                int levelButtonIndex = 0;
                foreach(LevelData levelData in LevelManager.levelInfo.customLevels)
                {
                    levelButtons[levelButtonIndex] = Instantiate(levelButtonPrefaB, scrollPanel.transform).GetComponent<LevelButton>();
                    levelButtons[levelButtonIndex].level = levelData.CreateLevel();
                    levelButtons[levelButtonIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(levelButtonSpacing * levelButtonIndex, 0);
                    levelButtons[levelButtonIndex].GetComponent<RectTransform>().localScale = unselectedButtonScale;
                    levelButtons[levelButtonIndex].name = levelData.levelName;
                    levelButtons[levelButtonIndex].GetComponent<Image>().sprite = Services.LevelDataLibrary.GetLevelImage(levelButtons[levelButtonIndex].name);
                    foreach (TextMeshProUGUI text in levelButtons[levelButtonIndex].GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        text.text = levelButtons[levelButtonIndex].name.ToLower();
                    }

                    levelButtonIndex++;
                }
                levelButtons[0].GetComponent<RectTransform>().localScale = selectedButtonScale;

            }
        }
        else if(Services.GameManager.mode == TitleSceneScript.GameMode.DungeonEdit)
        {
            levelButtons = new LevelButton[LevelManager.levelInfo.dungeonLevels.Count + 1];

            levelButtons[0] = Instantiate(levelButtonPrefaB, scrollPanel.transform).GetComponent<LevelButton>();
            levelButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            levelButtons[0].GetComponent<RectTransform>().localScale = selectedButtonScale;
            levelButtons[0].name = "create new";
            levelButtons[0].GetComponent<Image>().sprite = Services.LevelDataLibrary.GetLevelImage(levelButtons[0].name);
            foreach (TextMeshProUGUI text in levelButtons[0].GetComponentsInChildren<TextMeshProUGUI>())
            {
                text.text = levelButtons[0].name.ToLower();
            }

                int levelButtonIndex = 1;
                foreach (LevelData levelData in LevelManager.levelInfo.dungeonLevels.Values)
                {
                    levelButtons[levelButtonIndex] = Instantiate(levelButtonPrefaB, scrollPanel.transform).GetComponent<LevelButton>();
                    levelButtons[levelButtonIndex].level = levelData.CreateLevel();
                    levelButtons[levelButtonIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(levelButtonSpacing * levelButtonIndex, 0);
                    levelButtons[levelButtonIndex].GetComponent<RectTransform>().localScale = unselectedButtonScale;
                    levelButtons[levelButtonIndex].name = levelData.levelName;
                    levelButtons[levelButtonIndex].GetComponent<Image>().sprite = Services.LevelDataLibrary.GetLevelImage(levelButtons[levelButtonIndex].name);
                    levelButtons[levelButtonIndex].GetComponent<Image>().color = new Color(160f / 256f, 160f / 256f, 160f / 256f);

                    foreach (TextMeshProUGUI text in levelButtons[levelButtonIndex].GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        text.text = levelButtons[levelButtonIndex].name.ToLower();
                    }

                    levelButtonIndex++;
                }
            }
        else
        {
            levelButtons = new LevelButton[LevelManager.levelInfo.levelDictionary.Count + 1 - LevelManager.levelInfo.dungeonLevels.Count];

            levelButtons[0] = Instantiate(levelButtonPrefaB, scrollPanel.transform).GetComponent<LevelButton>();
            levelButtons[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            levelButtons[0].GetComponent<RectTransform>().localScale = selectedButtonScale;
            levelButtons[0].name = "procedural";
            levelButtons[0].GetComponent<Image>().sprite = Services.LevelDataLibrary.GetLevelImage(levelButtons[0].name);
            levelButtons[0].GetComponent<Image>().color = new Color(160f / 256f, 160f / 256f, 160f / 256f);


            foreach (TextMeshProUGUI text in levelButtons[0].GetComponentsInChildren<TextMeshProUGUI>())
            {
                text.text = levelButtons[0].name;
            }
            int levelButtonIndex = 1;
            foreach (KeyValuePair<string, LevelData> entry in LevelManager.levelInfo.levelDictionary)
            {
                if (entry.Key.Contains("Dungeon")) continue;
                levelButtons[levelButtonIndex] = Instantiate(levelButtonPrefaB, scrollPanel.transform).GetComponent<LevelButton>();
                levelButtons[levelButtonIndex].level = entry.Value.CreateLevel();
                levelButtons[levelButtonIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(levelButtonSpacing * levelButtonIndex, 0);
                levelButtons[levelButtonIndex].GetComponent<RectTransform>().localScale = unselectedButtonScale;
                levelButtons[levelButtonIndex].name = entry.Key;
                levelButtons[levelButtonIndex].GetComponent<Image>().sprite = Services.LevelDataLibrary.GetLevelImage(levelButtons[levelButtonIndex].name);
                levelButtons[levelButtonIndex].GetComponent<Image>().color = new Color(160f / 256f, 160f / 256f, 160f / 256f);
                foreach (TextMeshProUGUI text in levelButtons[levelButtonIndex].GetComponentsInChildren<TextMeshProUGUI>())
                {
                    text.text = levelButtons[levelButtonIndex].name.ToLower();
                }
                levelButtonIndex++;
            }
        }

        levelButtonParent.SetActive(false);
        humanPlayers = new bool[2];

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.TwoPlayers:
            case TitleSceneScript.GameMode.HyperVS:
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

        Services.CameraController.SetPosition(new Vector3(9.5f, 9.5f, -10));

        TaskTree mapSelectEntrance = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(levelSelectText, true)),
            new TaskTree(new LevelSelectButtonEntranceTask(levelButtons,playButton)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton)));
        _tm.Do(mapSelectEntrance);

        
    }

    internal override void OnExit()
    {
        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.HyperSOLO:
            case TitleSceneScript.GameMode.HyperVS:
                Services.GameManager.SetHandicapValues(HandicapSystem.hyperModeValues);
                break;
            default:
                Services.GameManager.SetHandicapValues(HandicapSystem.handicapValues);
                break;
        }
    }

    internal override void ExitTransition()
    {
        /*
        GameObject levelSelectText =
            levelButtonParent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        levelSelectText.SetActive(false);

        TaskTree mapSelectExit = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(levelSelectText,false, true)),
            new TaskTree(new LevelSelectButtonEntranceTask(levelButtons, null, true)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton, false, true)));

        _tm.Do(mapSelectExit);
        */
    }

    public void SelectLevel(LevelButton levelButton)
    {
        
        //levelSelectionIndicator.gameObject.SetActive(true);
        levelSelected = levelButton.level;
        //levelSelectionIndicator.transform.position = levelButton.transform.position;
        if ((Services.GameManager.mode == TitleSceneScript.GameMode.Edit || Services.GameManager.mode == TitleSceneScript.GameMode.DungeonEdit) && 
            levelSelected != null)
        {

            deleteButton.SetSelectedLevel(levelSelected);
            ToggleEditDeleteMenu(true);
        }
        else
        {
            if(levelSelected != null) levelSelected.setOverwriteMode();
            StartGame();
        }
    }

    public void OnLevelSelectSceneRefresh(RefreshLevelSelectSceneEvent e)
    {
        Services.Scenes.Swap<LevelSelectSceneScript>();
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
        switch(Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.Edit:
            case TitleSceneScript.GameMode.DungeonEdit:
                if (levelSelected != null) Services.Scenes.Swap<EditSceneScript>();
                else Services.Scenes.Swap<EditOptionsSceneScript>();
                break;
            default:
                Services.Scenes.Swap<GameSceneScript>();
                break;
        }    
    }

    private void RemoveOpposingPlayerMenuText(LevelButton[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            TextMeshProUGUI[] buttonText = buttons[i].GetComponentsInChildren<TextMeshProUGUI>(true);
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

    public void ToggleEditDeleteMenu(bool active)
    {
        editDeleteMenu.SetActive(active);
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
        if (Input.GetKeyDown(KeyCode.I)) ChangeScene();
	}
}

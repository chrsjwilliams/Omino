using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;



/*
 *                  
 *                  
 *      Think of ways to gate the other modes...
 *      
 *              Finish Tutorial for "quick play"
 *              Play quick play 4 times for challenge mode
 *              Get Elo rating of 120 to unlock Dungeon Run
 *              
 *                  
 */
public class TutorialLevelSceneScript : Scene<TransitionData>
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
    private Level levelSelected;
    
    [SerializeField]
    private GameObject tutorialLevelButtonParent;
    [SerializeField]
    private LevelButton[] tutorialLevelButtons;
    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;

    // Use this for initialization
    void Start ()
    {
		
	}

    private TaskManager _tm = new TaskManager();

    internal override void OnEnter(TransitionData data)
    {
        //tutorialLevelButtons = tutorialLevelButtonParent.GetComponentsInChildren<LevelButton>();
        tutorialLevelButtonParent.SetActive(false);
        backButton.SetActive(false);
        optionButton.SetActive(false);
        humanPlayers = new bool[2];
        humanPlayers[0] = true;
        humanPlayers[1] = false;
        int progress = 0;
        if (File.Exists(progressFileName))
        {
            string fileText = File.ReadAllText(progressFileName);
            int.TryParse(fileText, out progress);
        }
        SetLevelProgress(progress);

        tutorialLevelButtonParent.transform.eulerAngles = new Vector3(0, 0, 0);
        if (humanPlayers[0] && !humanPlayers[1])
        {
            tutorialLevelButtonParent.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (!humanPlayers[0] && humanPlayers[1])
        {
            tutorialLevelButtonParent.transform.eulerAngles = new Vector3(0, 0, 180);
        }
        tutorialLevelButtonParent.SetActive(true);
        for (int i = 0; i < tutorialLevelButtons.Length; i++)
        {
            tutorialLevelButtons[i].gameObject.SetActive(false);
        }

        GameObject levelSelectText =
            tutorialLevelButtonParent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        levelSelectText.SetActive(false);

        TaskTree tutorialEntrance = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(levelSelectText)),
            new TaskTree(new LevelSelectButtonEntranceTask(tutorialLevelButtons)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton)));
        _tm.Do(tutorialEntrance);
    }

    internal override void OnExit()
    {
        Services.GameManager.SetHandicapValues(HandicapSystem.handicapValues);

    }

    internal override void ExitTransition()
    {
        TaskTree tutorialExit = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(tutorialLevelButtonParent,false, true)),
            new TaskTree(new LevelSelectButtonEntranceTask(tutorialLevelButtons,null, true)),
            new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
            new TaskTree(new LevelSelectTextEntrance(optionButton, false , true)));
        _tm.Do(tutorialExit);
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

    public void SelectLevel(LevelButton levelButton)
    {
        if (levelButton.unlocked)
        {
            //levelSelectionIndicator.gameObject.SetActive(true);
            levelSelected = levelButton.level;
            //levelSelectionIndicator.transform.position = levelButton.transform.position;
            StartGame();
        }
        else
        {
            return;
        }
        
    }

    private void SetLevelProgress(int progress)
    {
        for (int i = 0; i < tutorialLevelButtons.Length; i++)
        {
            LevelButton button = tutorialLevelButtons[i].GetComponent<LevelButton>();
            int lockIndex = button.GetComponentsInChildren<Image>().Length - 1;
            int completionSymbolIndex = button.GetComponentsInChildren<Image>().Length - 2;
            if (i > progress)
            {
                button.unlocked = false;
                button.GetComponentsInChildren<Image>()[lockIndex].enabled = true;
                button.GetComponentsInChildren<Image>()[completionSymbolIndex].enabled = false;
            }
            else if (i < progress)
            {
                button.unlocked = true;
                button.GetComponentsInChildren<Image>()[completionSymbolIndex].enabled = true;
                button.GetComponentsInChildren<Image>()[lockIndex].enabled = false;
            }
            else if(i == progress)
            {
                button.unlocked = true;
                button.GetComponentsInChildren<Image>()[completionSymbolIndex].enabled = false;
                button.GetComponentsInChildren<Image>()[lockIndex].enabled = false;

            }
        }
    }

    private void SlideInLevelButtons()
    {
        

    }



    public void UnlockAllLevels()
    {
        SetLevelProgress(5);
        Services.GameManager.UnlockAllModes();
        File.WriteAllText(GameOptionsSceneScript.progressFileName, "5");
    }

    public void LockAllLevels()
    {
        SetLevelProgress(0);
        Services.GameManager.ModeUnlockReset();
        File.WriteAllText(GameOptionsSceneScript.progressFileName, "0");
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
    void Update ()
    {
        _tm.Update();
	}
}

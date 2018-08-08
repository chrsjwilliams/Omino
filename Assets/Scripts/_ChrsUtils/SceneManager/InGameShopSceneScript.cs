using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class InGameShopSceneScript : Scene<TransitionData>
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
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;

    // Use this for initialization
    void Start()
    {

    }

    private TaskManager _tm = new TaskManager();

    /*
     *      Look at hyper mode for color selection
     * 
     */ 

    internal override void OnEnter(TransitionData data)
    {
        //tutorialLevelButtons = tutorialLevelButtonParent.GetComponentsInChildren<LevelButton>();
        //tutorialLevelButtonParent.SetActive(false);
        //backButton.SetActive(false);
        //optionButton.SetActive(false);
        //humanPlayers = new bool[2];
        //humanPlayers[0] = true;
        //humanPlayers[1] = false;
        //int progress = 0;
        //if (File.Exists(progressFileName))
        //{
        //    string fileText = File.ReadAllText(progressFileName);
        //    int.TryParse(fileText, out progress);
        //}
        //SetLevelProgress(progress);

        //tutorialLevelButtonParent.transform.eulerAngles = new Vector3(0, 0, 0);
        //if (humanPlayers[0] && !humanPlayers[1])
        //{
        //    tutorialLevelButtonParent.transform.eulerAngles = new Vector3(0, 0, 0);
        //}
        //else if (!humanPlayers[0] && humanPlayers[1])
        //{
        //    tutorialLevelButtonParent.transform.eulerAngles = new Vector3(0, 0, 180);
        //}
        //tutorialLevelButtonParent.SetActive(true);
        //for (int i = 0; i < tutorialLevelButtons.Length; i++)
        //{
        //    tutorialLevelButtons[i].gameObject.SetActive(false);
        //}

        //GameObject levelSelectText =
        //    tutorialLevelButtonParent.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        //levelSelectText.SetActive(false);

        //TaskTree tutorialEntrance = new TaskTree(new EmptyTask(),
        //    new TaskTree(new LevelSelectTextEntrance(levelSelectText)),
        //    new TaskTree(new LevelSelectButtonEntranceTask(tutorialLevelButtons)),
        //    new TaskTree(new LevelSelectTextEntrance(backButton, true)),
        //    new TaskTree(new LevelSelectTextEntrance(optionButton)));
        //_tm.Do(tutorialEntrance);
    }

    internal override void OnExit()
    {
        Services.GameManager.SetHandicapValues(HandicapSystem.handicapValues);

    }

    internal override void ExitTransition()
    {
        //TaskTree tutorialExit = new TaskTree(new EmptyTask(),
        //    new TaskTree(new LevelSelectTextEntrance(tutorialLevelButtonParent, false, true)),
        //    new TaskTree(new LevelSelectButtonEntranceTask(tutorialLevelButtons, null, true)),
        //    new TaskTree(new LevelSelectTextEntrance(backButton, true, true)),
        //    new TaskTree(new LevelSelectTextEntrance(optionButton, false, true)));
        //_tm.Do(tutorialExit);
    }

    // Update is called once per frame
    void Update()
    {
        _tm.Update();
    }
}


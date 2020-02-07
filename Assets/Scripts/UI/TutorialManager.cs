
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class TutorialManager : MonoBehaviour
{
    public static string progressFileName
    {
        get
        {
            return Application.persistentDataPath + Path.DirectorySeparatorChar +
              "progress.txt";
        }
    }

    public static bool tutorialComplete = false;
    public static readonly int TUTORIAL_COMPLETE_NUMBER = 5;
    private static bool[] viewedTutorial = new bool[] { false, false, false, false, false };
    public TutorialTooltip currentTooltip { get; private set; }
    public GameObject tutorialTooltipPrefab;
    public int currentIndex { get; private set; }
    public GameObject backDim;
    public GameObject secondWindow;
    private TaskManager tm;
    public float delayDur;
    public TooltipInfo[] tooltipInfos { get { return Services.MapManager.currentLevel.tooltips; } }
    [SerializeField]
    private Transform tooltipZone;
    [SerializeField]
    private Button skipTutorialButton;
    private Color dimTutorialButtonColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    private Color dimmerTutorialButtonColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    private int humanPlayerNum = 1;
    [SerializeField]
    private bool completedRotation;
    private const float rotationInputRadius = 8f;
    private const float rotationDeadZone = 50f;
    public string[] objectiveText { get; private set; }
    public bool[] objectiveComplete { get; private set; }
    [SerializeField]
    private GameObject objectivesPanel;
    [SerializeField]
    private GameObject[] objectiveUI;
    [SerializeField]
    private Sprite success;
    [SerializeField]
    private Color successColor;
    [SerializeField]
    private Sprite notDone;
    [SerializeField]
    private GameObject secondObjectiveLocation;
    public bool tooltipActive { get; private set; }
    public static bool celebratoryTextBoxDisplayed = false;
    public static bool celebratoryTextBoxMade = false;
    public TextMeshProUGUI tutorialProgresss;

    // hide skip button & move first animation up 75 units

    private void Awake()
    {
        if(Services.GameManager.mode != TitleSceneScript.GameMode.Tutorial)
        {
            ToggleObjectiveUI(false);
            gameObject.SetActive(false);
        }
        skipTutorialButton.gameObject.SetActive(false);

        tm = new TaskManager();
    }

    // Use this for initialization
    void Start()
    {

        int progress = 0;
        if (File.Exists(progressFileName))
        {
            string fileText = File.ReadAllText(progressFileName);
            int.TryParse(fileText, out progress);
        }

        if(progress == TUTORIAL_COMPLETE_NUMBER)
        {
            tutorialComplete = true;
        }

        for(int i = 0; i < progress; i++)
        {
            if(progress < viewedTutorial.Length - 1)
                viewedTutorial[i] = true;
        }

        tutorialProgresss.text = "tutorial " + Services.GameManager.levelSelected.campaignLevelNum  + "/5";

        if (Services.GameManager.levelSelected != null &&
            Services.GameManager.levelSelected.objectives.Length > 0)
        {
            TaskTree slideInObjectives = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(objectivesPanel, true)));

            objectiveText = new string[Services.GameManager.levelSelected.objectives.Length];
            objectiveComplete = new bool[Services.GameManager.levelSelected.objectives.Length];
            for (int i = 0; i < objectiveText.Length; i++)
            {
                objectiveText[i] = Services.GameManager.levelSelected.objectives[i];
                objectiveUI[i].GetComponentInChildren<TextMeshProUGUI>().text = objectiveText[i].ToLower();

                if (Services.GameManager.CurrentDevice == DEVICE.IPHONE ||
                    Services.GameManager.CurrentDevice == DEVICE.IPHONE_X)
                {
                    objectiveUI[i].GetComponentInChildren<TextMeshProUGUI>().fontSize = 40;
                }
                objectiveComplete[i] = false;
                UpdateObjectiveUI(objectiveUI[i], objectiveComplete[i]);
                DisplayObjective(i, false);
            }
            tm.Do(slideInObjectives);
        }
  
        Services.GameEventManager.Register<RotationEvent>(OnRotation);
        Services.GameEventManager.Register<PieceRemoved>(OnPieceRemoved);
        Services.GameEventManager.Register<ClaimedTechEvent>(OnClaimTech);
        Services.GameEventManager.Register<GameEndEvent>(OnGameEnd);
    }

    private void OnDestroy()
    {
        Services.GameEventManager.Unregister<RotationEvent>(OnRotation);
        Services.GameEventManager.Unregister<PieceRemoved>(OnPieceRemoved);
        Services.GameEventManager.Unregister<ClaimedTechEvent>(OnClaimTech);
        Services.GameEventManager.Unregister<GameEndEvent>(OnGameEnd);
    }

    public static void ViewedAllTutorial(bool status)
    {
        for(int i  = 0; i < viewedTutorial.Length; i++)
        {
            viewedTutorial[i] = status;
        }
    }

    public void DisplaySkipButton()
    {
        if (Services.GameManager.mode != TitleSceneScript.GameMode.Tutorial) return;
        if (viewedTutorial[Services.GameManager.levelSelected.campaignLevelNum - 1])
        {
            skipTutorialButton.gameObject.SetActive(true);
        }
        else
        {
            skipTutorialButton.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        tm.Update();
        skipTutorialButton.GetComponent<Image>().color = Color.Lerp(dimTutorialButtonColor, dimmerTutorialButtonColor, Mathf.PingPong(Time.time, 0.5f));
    }

    public void Init()
    {
        currentIndex = 0;
        CreateTooltip();
        if (Services.GameManager.Players[0] is AIPlayer)
        {
            tooltipZone.localRotation = Quaternion.Euler(0, 0, -90);
            humanPlayerNum = 2;
        }
    }


    public void OnDismiss(bool displayNext = true)
    {
        if (Services.MapManager.currentLevel.campaignLevelNum == 5 &&
            celebratoryTextBoxMade && Services.GameScene.gameOver && !tutorialComplete)
        {
            Task showCampaignMenu = new Wait(0.5f);
            showCampaignMenu.Then(new ParameterizedActionTask<Player>(
                Services.UIManager.ShowCampaignLevelCompleteMenu, Services.GameManager.Players[0]));
            Services.GameScene.tm.Do(showCampaignMenu);
        }
        if (Services.MapManager.currentLevel.campaignLevelNum == 5 && 
            currentIndex == tooltipInfos.Length - 3)
        {
            celebratoryTextBoxMade = true;
        }

        if (tooltipInfos[currentIndex].dismissable)
        {
            Services.GameScene.UnpauseGame(true);
            backDim.SetActive(false);
        }

        Services.UIManager.tooltipsDisabled = false;
        tooltipActive = false;
        if (currentIndex < tooltipInfos.Length - 1 && displayNext) MoveToNextStep();
    }

    private void MoveToNextStep()
    {
        currentIndex += 1;

        CreateTooltip();
        if (!tooltipInfos[currentIndex].dismissable)
            Services.GameEventManager.Register<PiecePlaced>(OnPiecePlaced);
    }

    private void MoveToStep(int index)
    {
        if (index > tooltipInfos.Length - 1 || index < 0)
        {
            Debug.Log("Out of Range");
        }
        else
        {
            currentIndex = index;
            currentTooltip.Dismiss();
            if (!tooltipInfos[currentIndex].dismissable)
                Services.GameEventManager.Register<PiecePlaced>(OnPiecePlaced);
        }
    }

    public void SkipTutorial()
    {
        backDim.SetActive(false);
        if (Services.MapManager.currentLevel.campaignLevelNum == 5)
        {
            MoveToStep(tooltipInfos.Length - 4);
        }
        else
        {
            MoveToStep(tooltipInfos.Length - 1);
        }
        Services.GameScene.UnpauseGame(true);
        skipTutorialButton.gameObject.SetActive(false);

        for(int i = 0; i < objectiveText.Length; i++)
        {
            DisplayObjective(i, true);
        }
    }

    public bool CompletionCheck()
    {
        for(int i = 0; i < objectiveComplete.Length; i++)
        {
            if (!objectiveComplete[i]) return false;
        }
        return true;
    }

    public void UpdateObjectiveUI(GameObject objective, bool done)
    {
        if(done)
        {
            objective.GetComponentInChildren<Image>().sprite = success;
            objective.GetComponentInChildren<Image>().color = successColor;
        }
        else
        {
            objective.GetComponentInChildren<Image>().sprite = notDone;
            objective.GetComponentInChildren<Image>().color = Color.white;
        }
    }

    private void OnPiecePlaced(PiecePlaced e)
    {

        Task dismissTask = new Wait(1.2f);

        switch (Services.MapManager.currentLevel.campaignLevelNum)
        {
            case 1:
                if (currentTooltip.label == "Rotate" && !completedRotation) return;
                break;
            case 2:
                break;
            case 3:
                int aiPlayerNumber = humanPlayerNum == 1 ? 2 : 1;
                Player humanPlayer = Services.GameManager.Players[humanPlayerNum - 1];
                if (e.piece is Blueprint &&
                    !(e.piece.owner is AIPlayer))
                {
                    objectiveComplete[1] = true;
                    UpdateObjectiveUI(objectiveUI[1], objectiveComplete[1]);

                    if (currentIndex == tooltipInfos.Length - 2)
                    {
                        Services.GameEventManager.Unregister<PiecePlaced>(OnPiecePlaced);

                        dismissTask.Then(new ParameterizedActionTask<bool>(currentTooltip.Dismiss, true));
                        tm.Do(dismissTask);
                    }
                    else
                    {                                              
                        MoveToStep(tooltipInfos.Length - 1);
                    }
                }

                if (!((e.piece.owner.playerNum == aiPlayerNumber &&
                    (e.piece.owner.resourceProdLevel > 1 ||
                    e.piece.owner.normProdLevel > 1 ||
                    e.piece.owner.destProdLevel > 1)) &&
                    (humanPlayer.resourceProdLevel == 1 &&
                    humanPlayer.normProdLevel == 1 &&
                    humanPlayer.destProdLevel == 1)))
                    return;
                break;
            case 4:
                if (e.piece is Blueprint &&
                    !(e.piece is Generator) &&
                     !(e.piece.owner is AIPlayer))
                {
                    objectiveComplete[1] = true;
                    UpdateObjectiveUI(objectiveUI[1], objectiveComplete[1]);
                }
                
                    break;
            case 5:
                if (currentTooltip.label == "Do Not Display" || currentTooltip.label == "Complete")
                {
                    return;
                }
                break;
            default:
                break;
        }
        dismissTask.Then(new ParameterizedActionTask<bool>(currentTooltip.Dismiss, true));
        tm.Do(dismissTask);
    }

    public void OnPieceRemoved(PieceRemoved e)
    {
        switch (Services.MapManager.currentLevel.campaignLevelNum)
        {
            case 1:
                break;
            case 2:
                if (e.piece.owner is AIPlayer)
                {
                    objectiveComplete[1] = true;
                    UpdateObjectiveUI(objectiveUI[1], objectiveComplete[1]);
                }
                break;
            case 3:
                break;
            case 4:
                break;
            default:
                break;
        }
    }

    private void CreateTooltip()
    {
        currentTooltip = Instantiate(Services.Prefabs.TutorialTooltip,
            tooltipZone).GetComponent<TutorialTooltip>();
        TooltipInfo nextTooltipInfo = tooltipInfos[currentIndex];

        secondWindow.SetActive(false);
        currentTooltip.Init(nextTooltipInfo);
        tooltipActive = true;
        if(nextTooltipInfo.displayObjective)
        {
            DisplayObjective(nextTooltipInfo.objectiveIndex, true);
        }

        if (nextTooltipInfo.label == "Do Not Display")
        {
            currentTooltip.textBox.rectTransform.sizeDelta = new Vector2(0, 0);
        }
        else if ( nextTooltipInfo.label == "Attack Piece" || nextTooltipInfo.label == "Make Building" ||
                  nextTooltipInfo.label == "Another Building")
        {
            currentTooltip.textBox.rectTransform.sizeDelta = new Vector2(650 , 650);
        }

        if(nextTooltipInfo.label == "Place Piece" || nextTooltipInfo.label == "Tap")
        {
            Services.GameManager.Players[0].PauseProduction();
            Services.GameManager.Players[0].LockAllPiecesExcept(0);
            secondWindow.SetActive(true);
            backDim.SetActive(true);
        }

        if(Services.GameManager.CurrentDevice == DEVICE.IPHONE)
        {
            skipTutorialButton.GetComponent<RectTransform>().anchoredPosition = nextTooltipInfo.skipButtonLocationiPhone;
        }
        else if(Services.GameManager.CurrentDevice == DEVICE.IPHONE_X)
        {
            skipTutorialButton.GetComponent<RectTransform>().anchoredPosition = nextTooltipInfo.skipButtonLocationiPhoneX;
        }
        else
        {
            skipTutorialButton.GetComponent<RectTransform>().anchoredPosition = nextTooltipInfo.skipButtonLocation;
        }


        if(Services.MapManager.currentLevel.campaignLevelNum == 2)
        {
            if(currentIndex < 2)
            {
                Services.GameManager.Players[0].ToggleHandLock(true);
            }
            else
            {
                Services.GameManager.Players[0].ToggleHandLock(false);
            }
        }

        if (nextTooltipInfo.dismissable)
        {

            Services.GameScene.PauseGame(true, false);
            backDim.SetActive(true);
        }
        else if (!nextTooltipInfo.dismissable && currentIndex == 0)
        {
            Services.GameEventManager.Register<PiecePlaced>(OnPiecePlaced);
        }

        Services.UIManager.tooltipsDisabled = !nextTooltipInfo.enableTooltips;

        if (currentIndex == tooltipInfos.Length - 1 || 
            (Services.GameManager.levelSelected.campaignLevelNum == 3 &&
            currentIndex == tooltipInfos.Length - 3)||
            (Services.GameManager.levelSelected.campaignLevelNum == 5 &&
            currentIndex == tooltipInfos.Length - 4))
        {
            skipTutorialButton.gameObject.SetActive(false);
            viewedTutorial[Services.GameManager.levelSelected.campaignLevelNum - 1] = true;
        }
    }

    protected void OnRotation(RotationEvent e)
    {
        completedRotation = true;      

        if(Services.GameManager.levelSelected.campaignLevelNum == 1)
        {
            objectiveComplete[1] = true;
            UpdateObjectiveUI(objectiveUI[1], objectiveComplete[1]);
        }
    }

    public void OnClaimTech(ClaimedTechEvent e)
    {
        if (Services.GameManager.levelSelected.campaignLevelNum == 5 && 
            !(e.techBuilding.owner is AIPlayer))
        {
            objectiveComplete[1] = true;
            UpdateObjectiveUI(objectiveUI[1], objectiveComplete[1]);
        }
    }

    protected void OnGameEnd(GameEndEvent e)
    {
        if (!(e.winner is AIPlayer))
        {
            objectiveComplete[0] = true;
            UpdateObjectiveUI(objectiveUI[0], objectiveComplete[0]);
        }
        if(Services.MapManager.currentLevel.campaignLevelNum == 5 &&
            !tutorialComplete && CompletionCheck())
        {
            
            MoveToStep(tooltipInfos.Length - 3);
        }
        else
        {
            MoveToStep(tooltipInfos.Length - 1);
        }

        TaskTree slideOutObjectives = new TaskTree(new EmptyTask(),
            new TaskTree(new LevelSelectTextEntrance(objectivesPanel, true, true)));

        tm.Do(slideOutObjectives);    
    }

    public void ToggleObjectiveUI(bool show)
    {
        objectivesPanel.SetActive(show);
    }

    public void DisplaySecondObjective()
    {
        objectiveUI[1].SetActive(true);
    }

    public void DisplayObjective(int index, bool display)
    {       
        if (index == 1 && display)
        {
            TaskTree slidedDownFirstObjective = new TaskTree(new EmptyTask(),
                new TaskTree(new LERP(objectiveUI[0], objectiveUI[0].transform.localPosition,
                secondObjectiveLocation.transform.localPosition, 0.5f)));
            slidedDownFirstObjective.Then(new ActionTask(DisplaySecondObjective));
            tm.Do(slidedDownFirstObjective);
        }
        else
        {
            objectiveUI[index].SetActive(display);

        }
    }
}

[System.Serializable]
public class TooltipInfo
{
    public string label;
    [TextArea]
    public string text;
    public bool displayObjective;
    public int objectiveIndex; 
    public Vector2 location;    
    public Vector2 arrowLocation;  
    public float arrowRotation; 
    public bool dismissable = true;
    public bool enableTooltips;
    public Vector2 windowLocation;
    public Vector2 secondWindowLocation; 
    public Vector2 windowSize;
    public Vector2 secondWindowSize; 
    public bool haveImage;
    public bool imageLerps;
    public Vector2 imageLocation; 
    public Vector2 secondaryImageLocation;
    public Vector2 imageScale;
    public float imageRotation;   
    public Color imageColor;
    public Vector2 skipButtonLocation;   

    public Vector2 iPhoneToolTipSize;
    public Vector2 iPhoneArrowScale;
    public Vector2 iPhoneLocation;
    public Vector2 iPhoneArrowLocation;
    public Vector2 iPhoneWindowLocation;
    public Vector2 iPhoneSecondWindowLocation;
    public Vector2 iPhoneWindowSize;
    public Vector2 iPhoneSecondWindowSize;
    public float iPhoneArrowRotation;
    public Vector2 iPhoneImageLocation;
    public Vector2 iPhoneSecondaryImageLocation;
    public Vector2 iPhoneImageScale;
    public float iPhoneImageRotation;
    public Vector2 skipButtonLocationiPhone;

    public Vector2 iPhoneXToolTipSize;
    public Vector2 iPhoneXArrowScale;
    public Vector2 iPhoneXLocation;
    public Vector2 iPhoneXArrowLocation;
    public Vector2 iPhoneXWindowLocation;
    public Vector2 iPhoneXSecondWindowLocation;
    public Vector2 iPhoneXWindowSize;
    public Vector2 iPhoneXSecondWindowSize;
    public float iPhoneXArrowRotation;
    public Vector2 iPhoneXImageLocation;
    public Vector2 iPhoneXSecondaryImageLocation;
    public Vector2 iPhoneXImageScale;
    public float iPhoneXImageRotation;
    public Vector2 skipButtonLocationiPhoneX;

    public Vector2 iPadProToolTipSize;
    public Vector2 iPadProArrowScale;
    public Vector2 iPadProLocation;
    public Vector2 iPadProArrowLocation;
    public Vector2 iPadProWindowLocation;
    public Vector2 iPadProSecondWindowLocation;
    public Vector2 iPadProWindowSize;
    public Vector2 iPadProSecondWindowSize;
    public float iPadProArrowRotation;
    public Vector2 iPadProImageLocation;
    public Vector2 iPadProSecondaryImageLocation;
    public Vector2 iPadProImageScale;
    public float iPadProImageRotation;
    public Vector2 skipButtonLocationiPadPro;
}

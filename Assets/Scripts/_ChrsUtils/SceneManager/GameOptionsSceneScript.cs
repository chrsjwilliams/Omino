using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOptionsSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;

    private const float SECONDS_TO_WAIT = 0.01f;

    [SerializeField]
    private int numPlayers;
    private const string PLAYERS = "Players: ";

    [SerializeField]
    private bool turnBasedVersion;
    [SerializeField]
    private bool useStructures;
    [SerializeField]
    private bool useMiniBases;
    [SerializeField]
    private bool useBlueprints;
    private int levelSelected;

    private Text numPlayerText;
    private Text turnBasedButton;
    private Text structureButton;
    private Text miniBaseButton;
    private Text blueprintButton;
    [SerializeField]
    private GameObject levelButtonParent;
    private Button[] levelButtons;
    [SerializeField]
    private Image levelSelectionIndicator;

    private Slider winWeightSlider;
    private Slider structureWeightSlider;

    private int touchID;
    private TaskManager _tm = new TaskManager();

    private ScrollRectSnap ruleScroller;

    private void Start()
    {
        numPlayers = 2;
        turnBasedVersion = false;
        useStructures = true;
        useMiniBases = true;
        useBlueprints = true;

        numPlayerText = GameObject.Find("PlayerNum").GetComponent<Text>();
        structureButton = GameObject.Find("ToggleStructures").GetComponent<Text>();
        miniBaseButton = GameObject.Find("ToggleMiniBases").GetComponent<Text>();
        blueprintButton = GameObject.Find("ToggleBlueprints").GetComponent<Text>();
        winWeightSlider = GameObject.Find("WinWeightSlider").GetComponent<Slider>();
        structureWeightSlider = GameObject.Find("StructWeightSlider").GetComponent<Slider>();
        numPlayerText.text = PLAYERS + numPlayers;

        //ruleScroller = GetComponent<ScrollRectSnap>();

        //ruleScroller.Init();
        Services.GameEventManager.Register<TouchUp>(OnTouchUp);
        Services.GameEventManager.Register<TouchMove>(OnTouchMove);
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);

        Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Register<MouseMove>(OnMouseMoveEvent);
    }

    internal override void OnEnter(TransitionData data)
    {
        turnBasedVersion = false;
        useStructures = true;
        useMiniBases = true;
        useBlueprints = true;
        ruleScroller = GetComponent<ScrollRectSnap>();

        //ruleScroller.Init();
        Services.GameEventManager.Register<TouchUp>(OnTouchUp);
        Services.GameEventManager.Register<TouchMove>(OnTouchMove);
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);

        Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Register<MouseMove>(OnMouseMoveEvent);
        levelButtons = levelButtonParent.GetComponentsInChildren<Button>();
        SelectLevel(0);
    }

    internal override void OnExit()
    {
        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
        Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
    }

    public void IncrementPlayers()
    {
        if (numPlayers < Services.GameManager.MAX_PLAYERS)
        {
            numPlayers++;
        }
        numPlayerText.text = PLAYERS + numPlayers;
    }

    public void DecrementPlayers()
    {
        if(numPlayers > Services.GameManager.MIN_PLAYERS)
        {
            numPlayers--;
        }
        numPlayerText.text = PLAYERS + numPlayers;
    }

    public void ToggleTurnBased()
    {
        turnBasedVersion = !turnBasedVersion;
        if(turnBasedVersion)
        {
            turnBasedButton.text = "TURN BASED VERSION\nON";
        }
        else
        {
            turnBasedButton.text = "TURN BASED VERSION\nOFF";
        }
    }

    public void ToggleUsingStructures()
    {
        useStructures = !useStructures;
        if (useStructures)
        {
            structureButton.text = "STRUCTURES\nON";
        }
        else
        {
            structureButton.text = "STRUCTURES\nOFF";
        }
    }

    public void ToggleUsingMiniBases()
    {
        useMiniBases = !useMiniBases;
        if (useMiniBases)
        {
            miniBaseButton.text = "MINI-BASES\nON";
        }
        else
        {
            miniBaseButton.text = "MINI-BASES\nOFF";
        }
    }

    public void ToggleUsingBlueprints()
    {
        useBlueprints = !useBlueprints;
        if (useBlueprints)
        {
            blueprintButton.text = "BLUEPRINTS\nON";
        }
        else
        {
            blueprintButton.text = "BLUEPRINTS\nOFF";
        }
    }

    public void SetWinWeight()
    {
        Services.GameManager.SetWinWeight(winWeightSlider.value);
    }

    public void SetStructureWeight()
    {
        Services.GameManager.SetStructureWeight(structureWeightSlider.value);
    }

    private void OnTouchDown(TouchDown e)
    {
        OnInputDown();
    }

    protected void OnTouchUp(TouchUp e)
    {
        OnInputUp();
    }

    protected void OnMouseDownEvent(MouseDown e)
    {
        Vector3 mouseWorldPos = Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
        OnInputDown();
    }

    protected void OnMouseUpEvent(MouseUp e)
    {
        Vector3 mouseWorldPos = Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
        OnInputUp();
    }

    public void OnInputDown()
    {
        ruleScroller.dragging = true;
    }

    public void OnInputUp()
    {
        ruleScroller.dragging = false;
    }

    protected void OnMouseMoveEvent(MouseMove e)
    {
        OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos));
    }

    protected void OnTouchMove(TouchMove e)
    {
        if (e.touch.fingerId == touchID)
        {
            OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position));
        }
    }

    public void OnInputDrag(Vector3 inputPos)
    {
    }


    public void StartGame()
    {
        Services.GameManager.SetUserPreferences(turnBasedVersion, useStructures, 
            useMiniBases, useBlueprints, levelSelected);
        _tm.Do
        (
                    new Wait(SECONDS_TO_WAIT))
              .Then(new ActionTask(ChangeScene)
        );
        
    }

    private void ChangeScene()
    {
        Services.GameManager.SetNumPlayers(numPlayers);
        Services.Scenes.Swap<GameSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
    }

    public void SelectLevel(int levelNum)
    {
        levelSelected = levelNum;
        MoveLevelSelector(levelNum);
    }

    void MoveLevelSelector(int levelNum)
    {
        levelSelectionIndicator.transform.position = 
            levelButtons[levelNum].transform.position;
    }
}

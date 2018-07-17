using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{

    private static bool[] viewedTutorial = new bool[] { false, false, false, false };
    private TutorialTooltip currentTooltip;
    public GameObject tutorialTooltipPrefab;
    public int currentIndex { get; private set; }
    public GameObject backDim;
    private TaskManager tm;
    public float delayDur;
    public TooltipInfo[] tooltipInfos { get { return Services.MapManager.currentLevel.tooltips; } }
    [SerializeField]
    private Transform tooltipZone;
    [SerializeField]
    private Button skipTutorialButton;
    private int humanPlayerNum = 1;
    private int placementToolTipIndex = 5;
    [SerializeField]
    private bool completedRotation;
    private int touchID = -1;
    private const float rotationInputRadius = 8f;
    private const float rotationDeadZone = 50f;

    private void Awake()
    {
        tm = new TaskManager();
    }

    // Use this for initialization
    void Start()
    {
        skipTutorialButton.gameObject.SetActive(false);
        touchID = -1;
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
    }

    public void DisplaySkipButton()
    {
        if (Services.GameManager.mode != TitleSceneScript.GameMode.Campaign) return;
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

        if(Input.GetMouseButtonDown(1) && Services.GameManager.Players[0].selectedPiece != null)
        {
            CheckTouchForRotateInput(Services.GameManager.MainCamera.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SkipTutorial();
        }

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


    public void OnDismiss()
    {
        if (tooltipInfos[currentIndex].dismissable)
        {
            Services.GameScene.UnpauseGame(true);
            backDim.SetActive(false);
        }
        Services.UIManager.tooltipsDisabled = false;
        if (currentIndex < tooltipInfos.Length - 1) MoveToNextStep();
    }

    private void MoveToNextStep()
    {
        currentIndex += 1;
        //Wait wait = new Wait(delayDur);
        //wait.Then(new ActionTask(CreateTooltip));
        //tm.Do(wait);
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
            CreateTooltip();
            if (!tooltipInfos[currentIndex].dismissable)
                Services.GameEventManager.Register<PiecePlaced>(OnPiecePlaced);
        }
    }

    public void SkipTutorial()
    {
        backDim.SetActive(false);
        MoveToStep(tooltipInfos.Length - 1);
        Services.GameScene.UnpauseGame(true);
        skipTutorialButton.gameObject.SetActive(false);
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
                if (e.piece.owner.playerNum == humanPlayerNum) return;
                break;
            case 3:
                int aiPlayerNumber = humanPlayerNum == 1 ? 2 : 1;
                Player humanPlayer = Services.GameManager.Players[humanPlayerNum - 1];
                Player aiPlayer = Services.GameManager.Players[aiPlayerNumber - 1];
                if (humanPlayer.resourceProdLevel > 1 ||
                    humanPlayer.normProdLevel > 1 ||
                    humanPlayer.destProdLevel > 1)
                {
                    if (currentIndex == tooltipInfos.Length - 2)
                    {
                        Services.GameEventManager.Unregister<PiecePlaced>(OnPiecePlaced);

                        dismissTask.Then(new ActionTask(currentTooltip.Dismiss));
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
            default:
                break;
        }

        Services.GameEventManager.Unregister<PiecePlaced>(OnPiecePlaced);

        dismissTask.Then(new ActionTask(currentTooltip.Dismiss));
        tm.Do(dismissTask);

    }

    private void CreateTooltip()
    {
        currentTooltip = Instantiate(Services.Prefabs.TutorialTooltip,
            tooltipZone).GetComponent<TutorialTooltip>();
        TooltipInfo nextTooltipInfo = tooltipInfos[currentIndex];

        currentTooltip.Init(nextTooltipInfo);


        if (nextTooltipInfo.label == "Do Not Display")
        {
            currentTooltip.textBox.rectTransform.sizeDelta = new Vector2(0, 0);
        }
        else if (nextTooltipInfo.label == "Attack Piece" || nextTooltipInfo.label == "Make Building")
        {
            currentTooltip.textBox.rectTransform.sizeDelta = new Vector2(575, 575);
        }

        if (nextTooltipInfo.dismissable)
        {
            Services.GameScene.PauseGame(true);
            backDim.SetActive(true);
        }
        else if (!nextTooltipInfo.dismissable && currentIndex == 0)
        {
            Services.GameEventManager.Register<PiecePlaced>(OnPiecePlaced);
        }

        Services.UIManager.tooltipsDisabled = !nextTooltipInfo.enableTooltips;

        if (currentIndex == tooltipInfos.Length - 1)
        {
            viewedTutorial[Services.GameManager.levelSelected.campaignLevelNum - 1] = true;
        }
    }

    protected void CheckTouchForRotateInput(TouchDown e)
    {
        if ((Vector2.Distance(
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position),
            Services.GameManager.MainCamera.ScreenToWorldPoint(Input.GetTouch(touchID).position))
            < rotationInputRadius) ||
            (e.touch.position.y < (Screen.height / 2 - rotationDeadZone)))
        {
            completedRotation = true;
        }
    }

    protected void CheckTouchForRotateInput(Vector3 e)
    {
        completedRotation = true;      
    }

    protected void OnTouchDown(TouchDown e)
    {
        Vector3 touchWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position);
        if (touchID == -1)
        {
            touchID = e.touch.fingerId;
            OnInputDown(touchWorldPos);
        }
    }

    protected void OnMouseDownEvent(MouseDown e)
    {
        Vector3 mouseWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);

        OnInputDown(mouseWorldPos);
    }

    protected void OnTouchUp(TouchUp e)
    {
        if (e.touch.fingerId == touchID)
        {
            OnInputUp();
            touchID = -1;
        }
    }

    protected void OnMouseUpEvent(MouseUp e)
    {
        OnInputUp();
    }

    public void OnInputDown(Vector3 touchPos)
    {
        Services.GameEventManager.Register<TouchUp>(OnTouchUp);
        Services.GameEventManager.Register<TouchDown>(CheckTouchForRotateInput);
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

        Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
    }

    public virtual void OnInputUp()
    {
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
        Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);

        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
    }
}

[System.Serializable]
public class TooltipInfo
{
    public string label;
    [TextArea]
    public string text;
    public Vector2 location;
    public Vector2 arrowLocation;
    public float arrowRotation;
    public bool dismissable = true;
    public bool enableTooltips;
    public Vector2 windowLocation;
    public Vector2 windowSize;
    public bool haveImage;
    public bool imageLerps;
    public Vector2 imageLocation;
    public Vector3 secondaryImageLocation = Vector3.back;
    public Vector2 imageScale;
    public float imageRotation;
    public Color imageColor;
    public Vector2 subImageLocation;
    public Vector3 secondarySubImageLocation = Vector3.back;
    public Vector2 subImageScale;
    public float subImageRotation;
    public Color subImageColor;
    public Vector2 subImage2Location;
    public Vector3 secondarySubImage2Location = Vector3.back;
    public Vector2 subImage2Scale;
    public float subImage2Rotation;
    public Color subImage2Color;
}

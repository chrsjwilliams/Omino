using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

    private TutorialTooltip currentTooltip;
    public GameObject tutorialTooltipPrefab;
    private int currentIndex;
    public GameObject backDim;
    private TaskManager tm;
    public float delayDur;
    private TooltipInfo[] tooltipInfos { get { return Services.MapManager.currentLevel.tooltips; } }
    [SerializeField]
    private Transform tooltipZone;
    private int humanPlayerNum = 1;
    private int rotateToolTipIndex = 5;

    private void Awake()
    {
        tm = new TaskManager();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        tm.Update();
	}

    public void Init()
    {
        currentIndex = 0;
        CreateTooltip();
        if (Services.GameManager.Players[0] is AIPlayer)
        {
            tooltipZone.localRotation = Quaternion.Euler(0, 0, 180);
            humanPlayerNum = 2;
        }
    }


    public void OnDismiss()
    {
        if (tooltipInfos[currentIndex].dismissable)
        {
            Services.GameScene.UnpauseGame();
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

    private void OnPiecePlaced(PiecePlaced e)
    {
        if (e.piece.owner.playerNum == humanPlayerNum)
        {
            Services.GameEventManager.Unregister<PiecePlaced>(OnPiecePlaced);
            Task dismissTask = new Wait(1.2f);
            dismissTask.Then(new ActionTask(currentTooltip.Dismiss));
            tm.Do(dismissTask);
        }
    }

    private void CreateTooltip()
    {
        currentTooltip = Instantiate(Services.Prefabs.TutorialTooltip,
            tooltipZone).GetComponent<TutorialTooltip>();
        TooltipInfo nextTooltipInfo = tooltipInfos[currentIndex];
        nextTooltipInfo.imageColor = Color.black;

        if (humanPlayerNum == 1) nextTooltipInfo.subImageColor = Services.GameManager.AdjustColorAlpha(Services.GameManager.Player1ColorScheme[0], 0.5f);
        else nextTooltipInfo.subImageColor = Services.GameManager.AdjustColorAlpha(Services.GameManager.Player2ColorScheme[0], 0.5f);

        currentTooltip.Init(nextTooltipInfo);

        if (Services.MapManager.currentLevel.campaignLevelNum == 1 && currentIndex >= rotateToolTipIndex)
        {
            currentTooltip.ToggleImageAnimation("RotateAnimation", true);
        }
        if (nextTooltipInfo.dismissable)
        {
            Services.GameScene.PauseGame();
            backDim.SetActive(true);
        }
        Services.UIManager.tooltipsDisabled = !nextTooltipInfo.enableTooltips;
    }
}

[System.Serializable]
public class TooltipInfo
{
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
    public Vector2 imageLocation;
    public float imageRotation;
    public Color imageColor;
    public Vector2 subImageLocation;
    public float subImageRotation;
    public Color subImageColor;
}

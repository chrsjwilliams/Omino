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
        currentTooltip.Init(nextTooltipInfo);
        if (nextTooltipInfo.dismissable)
        {
            Services.GameScene.PauseGame();
            backDim.SetActive(true);
        }
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
    public Vector2 windowLocation;
    public Vector2 windowSize;
}

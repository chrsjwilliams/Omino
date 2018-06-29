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
    private int placementToolTipIndex = 5;

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

    private void OnPiecePlaced(PiecePlaced e)
    {
        
        Task dismissTask = new Wait(1.2f);

        switch (Services.MapManager.currentLevel.campaignLevelNum)
        {
            case 1:
                if (e.piece.owner.playerNum != humanPlayerNum) return;
                    break;
            case 2:
                if (e.piece.owner.playerNum == humanPlayerNum) return;
                break;
            case 3:
                int aiPlayerNumber = humanPlayerNum == 1 ? 2 : 1;
                Player humanPlayer = Services.GameManager.Players[humanPlayerNum - 1];
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

        //  Camp 2: called when opponent places 1 piece

        //  Camp 3: called if AI places blueprint but player has not

        //  Camp 4:  Not in camp4

        dismissTask.Then(new ActionTask(currentTooltip.Dismiss));
        tm.Do(dismissTask);
        
    }

    private void CreateTooltip()
    {
        currentTooltip = Instantiate(Services.Prefabs.TutorialTooltip,
            tooltipZone).GetComponent<TutorialTooltip>();
        TooltipInfo nextTooltipInfo = tooltipInfos[currentIndex];
        //nextTooltipInfo.imageColor = Color.black;

        //if (humanPlayerNum == 1) nextTooltipInfo.subImageColor = Services.GameManager.AdjustColorAlpha(Services.GameManager.Player1ColorScheme[0], 0.5f);
        //else nextTooltipInfo.subImageColor = Services.GameManager.AdjustColorAlpha(Services.GameManager.Player2ColorScheme[0], 0.5f);


        currentTooltip.Init(nextTooltipInfo);

        if(nextTooltipInfo.tag == "Do Not Display")
        {
            currentTooltip.textBox.rectTransform.sizeDelta = new Vector2(0, 0);
        }
        else if (nextTooltipInfo.tag == "Attack Piece" || nextTooltipInfo.tag == "Make Building")
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
    }
}

[System.Serializable]
public class TooltipInfo
{
    public string tag;
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

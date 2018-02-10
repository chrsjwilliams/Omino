using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public RectTransform[] handZones;
    public Text[] resourceCounters;
    private Image[] resourceSymbols;
    public RectTransform[] blueprintUIZones;
    [SerializeField]
    private Image[] normalDrawMeters;
    [SerializeField]
    private Text[] normalPieceTimers;
    private Transform[] normalQueueMeters;
    [SerializeField]
    private Image[] destructorDrawMeters;
    [SerializeField]
    private Text[] destructorPieceTimers;
    private Transform[] destructorQueueMeters;
    [SerializeField]
    private Image[] greyOutBoxes;
    [SerializeField]
    private RectTransform[] victoryBanners;
    [SerializeField]
    private RectTransform[] defeatBanners;
    public Transform[] mineBlueprintLocations;
    public Transform[] factoryBlueprintLocations;
    public Transform[] bombFactoryBlueprintLocations;
    public Button[] readyBanners;
    [SerializeField]
    private GameObject pauseMenu;
    public Transform canvas;
    public Sprite destructorIcon;
    public Sprite bombFactoryIcon;
    public Sprite factoryIcon;
    public Sprite mineIcon;
    public Sprite baseIcon;
    public Sprite drillIcon;
    public Sprite gearIcon;
    public Sprite steelIcon;
    public Sprite bricksIcon;
    public Sprite bigBombIcon;
    public Sprite splashIcon;
    public Sprite shieldIcon;
    public Sprite[] factoryOverlays;
    public Sprite[] mineOverlays;
    public Sprite baseOverlay;
    public Sprite[] bombFactoryOverlays;
    public Sprite structureOverlay;
    private bool scrollingInBanners;
    private Player winner;
    private float bannerScrollTimeElapsed;
    [SerializeField]
    private float bannerScrollDuration;
    private List<int> touchIdsMakingTooltips;
    [SerializeField]
    private Vector3 queueMeterOffset;
    [SerializeField]
    private Color notReadyColor;
    [SerializeField]
    private Color readyColor;
    public float readyBannerScrollOffTime;
    public float resourceGainAnimationDist;
    public float resourceGainAnimationDur;
    public Vector3 resourceGainAnimationOffset;
    [SerializeField]
    private float radialMeterFillMin;
    [SerializeField]
    private float radialMeterFillMax;

	// Use this for initialization
	void Start () {
        for (int i = 0; i < victoryBanners.Length; i++)
        {
            victoryBanners[i].gameObject.SetActive(false);
            defeatBanners[i].gameObject.SetActive(false);
        }
        touchIdsMakingTooltips = new List<int>();
        //InitializeQueueMeters();
        resourceSymbols = new Image[2];
        for (int i = 0; i < 2; i++)
        {
            UpdateDrawMeters(i + 1, 0, 0, 0, 0);
            resourceSymbols[i] = resourceCounters[i].GetComponentInChildren<Image>();
            resourceSymbols[i].fillAmount = 0;
        }
        pauseMenu.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (scrollingInBanners) ScrollBanners();
	}

	//public void UpdateTouchCount(Touch[] touches){
	//	string newText = "";
	//	for (int i = 0; i < touches.Length; i++) {
	//		newText += "touch "+ touches[i].fingerId + " at :" + touches [i].position.x + ", " + touches [i].position.y + "\n";
	//	}
	//	touchCount.text = newText;
	//}



    void InitializeQueueMeters()
    {
        normalQueueMeters = new Transform[2];
        destructorQueueMeters = new Transform[2];
        for (int i = 0; i < 2; i++)
        {
            Quaternion rotation;
            Vector3 offset = queueMeterOffset;
            if(i == 0)
            {
                rotation = Quaternion.Euler(0, 0, -90);
            }
            else
            {
                rotation = Quaternion.Euler(0, 0, 90);
                offset = new Vector3(offset.x, -offset.y, 0);
            }
            Vector3 normalPos = Services.GameManager.MainCamera
                .ScreenToWorldPoint(normalDrawMeters[i].transform.position) + offset;
            Transform normalQueueBar = Instantiate(Services.Prefabs.QueueBar,
                new Vector3(normalPos.x, normalPos.y, 0),
                rotation).transform;
            normalQueueMeters[i] = normalQueueBar;
            Vector3 destructorPos = Services.GameManager.MainCamera
                 .ScreenToWorldPoint(destructorDrawMeters[i].transform.position) + offset;
            Transform destructorQueueBar = Instantiate(Services.Prefabs.QueueBar,
                 new Vector3(destructorPos.x, destructorPos.y, 0),
                 rotation).transform;
            destructorQueueMeters[i] = destructorQueueBar;
        }
    }

    public void UpdateDrawMeters(int playerNum, float normalFillProportion, 
        float destructorFillProportion, float normalTimeLeft, float destructorTimeLeft)
    {
        float meterBody = radialMeterFillMax - radialMeterFillMin;

        float adjustedNormalProportion = radialMeterFillMin + 
            (normalFillProportion * meterBody);
        float adjustedDestructorProportion = radialMeterFillMin +
            (destructorFillProportion * meterBody);

        normalDrawMeters[playerNum - 1].fillAmount = adjustedNormalProportion;
        destructorDrawMeters[playerNum - 1].fillAmount = adjustedDestructorProportion;

        normalPieceTimers[playerNum - 1].text = Mathf.CeilToInt(normalTimeLeft).ToString();
        destructorPieceTimers[playerNum - 1].text = Mathf.CeilToInt(destructorTimeLeft).ToString();

        //normalQueueMeters[playerNum - 1].localScale = 
        //    new Vector3(normalFillProportion, 1, 1);
        //destructorQueueMeters[playerNum - 1].localScale =
        //    new Vector3(destructorFillProportion, 1, 1);
    }

    public void StartBannerScroll(Player winner_)
    {
        scrollingInBanners = true;
        winner = winner_;
        for (int i = 0; i < victoryBanners.Length; i++)
        {
            if(i == winner.playerNum - 1)
            {
                victoryBanners[i].gameObject.SetActive(true);
                victoryBanners[i].anchoredPosition = new Vector2(0, 1536);
            }
            else
            {
                defeatBanners[i].gameObject.SetActive(true);
                defeatBanners[i].anchoredPosition = new Vector2(0, 1536);
            }
        }
        bannerScrollTimeElapsed = 0;
    }

    void ScrollBanners()
    {
        bannerScrollTimeElapsed += Time.deltaTime;
        for (int i = 0; i < victoryBanners.Length; i++)
        {
            if(i == winner.playerNum - 1)
            {
                victoryBanners[i].anchoredPosition =
                    Vector2.Lerp(new Vector2(0, 1536), Vector2.zero,
                    EasingEquations.Easing.QuadEaseOut(
                        bannerScrollTimeElapsed / bannerScrollDuration));
            }
            else
            {
                defeatBanners[i].anchoredPosition =
                    Vector2.Lerp(new Vector2(0, 1536), Vector2.zero,
                    EasingEquations.Easing.QuadEaseOut(
                        bannerScrollTimeElapsed / bannerScrollDuration));
            }
        }
        if (bannerScrollTimeElapsed >= bannerScrollDuration) scrollingInBanners = false;
    }

    public void SetGreyOutBox(int playerNum, bool status)
    {
        greyOutBoxes[playerNum - 1].enabled = status;
    }

    public void UpdateResourceMeter(int playerNum, float fillProportion)
    {
        resourceSymbols[playerNum - 1].fillAmount = fillProportion;
    }

    public void UpdateResourceCount(int resourceCount, int maxResources, Player player)
    {
        resourceCounters[player.playerNum - 1].text = resourceCount + "/" + maxResources;
    }

    public void OnTooltipCreated(int touchId)
    {
        if(!IsTouchMakingTooltipAlready(touchId)) touchIdsMakingTooltips.Add(touchId);
    }

    public void OnTooltipDestroyed(int touchId)
    {
        if (IsTouchMakingTooltipAlready(touchId)) touchIdsMakingTooltips.Remove(touchId);
    }

    public bool IsTouchMakingTooltipAlready(int touchId)
    {
        return touchIdsMakingTooltips.Contains(touchId);
    }

    public Vector3 GetBarPosition(int playerNum, bool destructor)
    {
        if (destructor)
        {
            return destructorDrawMeters[playerNum - 1].transform.position;
        }
        else
        {
            return normalDrawMeters[playerNum - 1].transform.position;
        }
    }

    public void ToggleReady(int playerNum)
    {
        Player player = Services.GameManager.Players[playerNum - 1];
        player.ToggleReady();
        if (player.ready)
        {
            readyBanners[playerNum - 1].GetComponentInChildren<Text>().text =
                "READY";
            readyBanners[playerNum - 1].GetComponent<Image>().color = readyColor;
        }
        else
        {
            readyBanners[playerNum - 1].GetComponentInChildren<Text>().text =
                "TAP WHEN READY";
            readyBanners[playerNum - 1].GetComponent<Image>().color = notReadyColor;
        }
        bool allReady = true;
        for (int i = 0; i < Services.GameManager.Players.Length; i++)
        {
            if (!Services.GameManager.Players[i].ready)
            {
                allReady = false;
                break;
            }
        }
        if (allReady)
        {
            Services.GameScene.StartGameSequence();
        }
    }

    public void TogglePauseMenu()
    {
        if (Services.GameScene.gamePaused)
        {
            pauseMenu.SetActive(false);
            Services.GameScene.UnpauseGame();
        }
        else
        {
            pauseMenu.SetActive(true);
            Services.GameScene.PauseGame();
        }
    }

}

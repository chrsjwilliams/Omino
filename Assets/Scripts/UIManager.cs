using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public RectTransform[] handZones;
    public Text[] resourceCounters;
    public GameObject[] resourceSlotZones;
    private Image[][] resourceSlots;
    private Image[][] resourceMissingIndicators;
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
    public Transform uiTileHolder;
    public Transform canvas;
    public Sprite blueprintTile;
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
    [SerializeField]
    private float resourceGainHighlightScale;
    [SerializeField]
    private float resourceGainHighlightDuration;
    private float[] resourceGainHighlightTimeElapsed;
    private int[] resourceGainHighlightIndices;
    private bool[] resourceGainHighlightActive;
    private bool[] resourceGainHighlightIncreasing;
    private int[] lastResourceCount;
    [SerializeField]
    private float resourceMissingAnimDuration;
    [SerializeField]
    private float resourceMissingAnimScale;
    private float[] resourceMissingTimeElapsed;
    private int[] numResourcesMissing;
    private bool[] resourceMissingAnimActive;
    private bool[] resourceMissingAnimIncreasing;


    private void Awake()
    {
        Image[] slotsP1 = resourceSlotZones[0].GetComponentsInChildren<Image>();
        Image[] slotsP2 = resourceSlotZones[1].GetComponentsInChildren<Image>();
        Image[] slotTopsP1 = new Image[slotsP1.Length / 3];
        Image[] slotTopsP2 = new Image[slotsP1.Length / 3];
        Image[] missingIndicatorsP1 = new Image[slotsP1.Length / 3];
        Image[] missingIndicatorsP2 = new Image[slotsP1.Length / 3];
        for (int i = 0; i < slotsP1.Length; i++)
        {
            if (i % 3 == 1)
            {
                slotTopsP1[i / 3] = slotsP1[i];
                slotTopsP2[i / 3] = slotsP2[i];
                slotTopsP1[i / 3].color = Services.GameManager.Player1ColorScheme[0];
                slotTopsP2[i / 3].color = Services.GameManager.Player2ColorScheme[0];
            }
            else if (i%3 == 2)
            {
                missingIndicatorsP1[i / 3] = slotsP1[i];
                missingIndicatorsP2[i / 3] = slotsP2[i];
                Color indicatorColor = missingIndicatorsP1[i / 3].color;
                missingIndicatorsP1[i / 3].color = new Color(indicatorColor.r,
                    indicatorColor.g, indicatorColor.b, 0);
                missingIndicatorsP2[i / 3].color = new Color(indicatorColor.r,
                    indicatorColor.g, indicatorColor.b, 0);
            }
        }

        resourceSlots = new Image[][] { slotTopsP1, slotTopsP2 };
        resourceMissingIndicators = new Image[][] { missingIndicatorsP1, missingIndicatorsP2 };
        resourceGainHighlightIndices = new int[2] { 0, 0 };
        resourceGainHighlightTimeElapsed = new float[2] { 0, 0 };
        resourceGainHighlightActive = new bool[2] { false, false };
        resourceGainHighlightIncreasing = new bool[2] { true, true };
        lastResourceCount = new int[2] { 0, 0 };
        resourceMissingAnimActive = new bool[2] { false, false };
        resourceMissingAnimIncreasing = new bool[2] { true, true };
        resourceMissingTimeElapsed = new float[2] { 0, 0 };
        numResourcesMissing = new int[2] { 0, 0 };
    }

    // Use this for initialization
    void Start () {
        for (int i = 0; i < victoryBanners.Length; i++)
        {
            victoryBanners[i].gameObject.SetActive(false);
            defeatBanners[i].gameObject.SetActive(false);
        }
        touchIdsMakingTooltips = new List<int>();
        //InitializeQueueMeters();
        for (int i = 0; i < 2; i++)
        {
            UpdateDrawMeters(i + 1, 0, 0, 0, 0);
            //resourceSymbols[i] = resourceCounters[i].GetComponentInChildren<Image>();
            //resourceSymbols[i].fillAmount = 0;
        }
        pauseMenu.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (scrollingInBanners) ScrollBanners();
        HighlightResourceGained();
        HighlightResourcesMissing();
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
        if (bannerScrollTimeElapsed >= bannerScrollDuration)
        {
            scrollingInBanners = false;
            //if (!Services.GameScene.gamePaused && Services.GameScene.normalPlayMode)
            //    TogglePauseMenu();
        }
    }

    public void SetGreyOutBox(int playerNum, bool status)
    {
        greyOutBoxes[playerNum - 1].enabled = status;
    }

    public void UpdateResourceMeter(int playerNum, float fillProportion)
    {
        for (int i = 0; i < resourceSlots[playerNum - 1].Length; i++)
        {
            Image slotImage = resourceSlots[playerNum - 1][i];
            if (slotImage.fillAmount < 1)
            {
                slotImage.color = new Color(slotImage.color.r, slotImage.color.g,
                    slotImage.color.b, fillProportion * (2f/3));
                slotImage.fillAmount = fillProportion;
                break;
            }
        }
    }

    public void UpdateResourceCount(int resourceCount, int maxResources, Player player)
    {
        //resourceCounters[player.playerNum - 1].text = resourceCount + "/" + maxResources;
        int playerIndex = player.playerNum - 1;
        for (int i = 0; i < resourceSlots[playerIndex].Length; i++)
        {
            Image slotImage = resourceSlots[playerIndex][i];
            if (i < resourceCount)
            {
                slotImage.fillAmount = 1;
                slotImage.color = new Color(slotImage.color.r, 
                    slotImage.color.g, slotImage.color.b, 1);
                if (resourceCount > lastResourceCount[playerIndex]
                    && i == resourceCount - 1)
                {
                    resourceGainHighlightIndices[playerIndex] = i;
                    resourceGainHighlightActive[playerIndex] = true;
                }
            }
            else
            {
                slotImage.fillAmount = 0;
            }
        }
        if(resourceCount != lastResourceCount[playerIndex] 
            && numResourcesMissing[playerIndex] != 0)
        {
            numResourcesMissing[playerIndex] = Mathf.Max(0,
                numResourcesMissing[playerIndex]
                - (resourceCount - lastResourceCount[playerIndex]));
        }
        lastResourceCount[playerIndex] = resourceCount;
    }

    private void HighlightResourceGained()
    {
        for (int i = 0; i < 2; i++)
        {
            if (resourceGainHighlightActive[i])
            {
                resourceGainHighlightTimeElapsed[i] += Time.deltaTime;
                if (resourceGainHighlightIncreasing[i])
                {
                    resourceSlots[i][resourceGainHighlightIndices[i]].transform.localScale =
                        Vector3.Lerp(Vector3.one, resourceGainHighlightScale * Vector3.one,
                        EasingEquations.Easing.QuadEaseOut(
                            resourceGainHighlightTimeElapsed[i] / resourceGainHighlightDuration));
                    if(resourceGainHighlightTimeElapsed[i] > resourceGainHighlightDuration)
                    {
                        resourceGainHighlightIncreasing[i] = false;
                        resourceGainHighlightTimeElapsed[i] = 0;
                    }
                }
                else
                {
                    resourceSlots[i][resourceGainHighlightIndices[i]].transform.localScale =
                        Vector3.Lerp(resourceGainHighlightScale * Vector3.one, Vector3.one,
                        EasingEquations.Easing.QuadEaseIn(
                            resourceGainHighlightTimeElapsed[i] / resourceGainHighlightDuration));
                    if(resourceGainHighlightTimeElapsed[i] > resourceGainHighlightDuration)
                    {
                        resourceGainHighlightActive[i] = false;
                        resourceGainHighlightIncreasing[i] = true;
                        resourceGainHighlightTimeElapsed[i] = 0;
                    }
                }
            }
        }

    }

    private void HighlightResourcesMissing()
    {
        for (int i = 0; i < 2; i++)
        {
            Color indicatorColor = resourceMissingIndicators[i][0].color;
            if (resourceMissingAnimActive[i])
            {
                int resourcesMissingLeftToHighlight = numResourcesMissing[i];
                if (resourcesMissingLeftToHighlight == 0)
                {
                    for (int j = 0; j < resourceSlots[i].Length; j++)
                    {
                        resourceMissingIndicators[i][j].color = new Color(indicatorColor.r,
                            indicatorColor.g, indicatorColor.b, 0);
                    }
                }
                else
                {
                    resourceMissingTimeElapsed[i] += Time.deltaTime;
                    for (int j = 0; j < resourceSlots[i].Length; j++)
                    {
                        if (resourceSlots[i][j].fillAmount != 1)
                        {
                            resourcesMissingLeftToHighlight -= 1;
                            if (resourceMissingAnimIncreasing[i])
                            {
                                resourceMissingIndicators[i][j].color = Color.Lerp(
                                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 0),
                                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 1),
                                    EasingEquations.Easing.QuadEaseOut(
                                        resourceMissingTimeElapsed[i] / resourceMissingAnimDuration));
                                resourceMissingIndicators[i][j].transform.localScale = Vector3.Lerp(
                                    Vector3.one, resourceMissingAnimScale * Vector3.one,
                                    EasingEquations.Easing.QuadEaseOut(
                                        resourceMissingTimeElapsed[i] / resourceMissingAnimDuration));
                                if (resourceMissingTimeElapsed[i] > resourceMissingAnimDuration)
                                {
                                    resourceMissingAnimIncreasing[i] = false;
                                    resourceMissingTimeElapsed[i] = 0;
                                }
                            }
                            else
                            {
                                resourceMissingIndicators[i][j].color = Color.Lerp(
                                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 1),
                                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 0),
                                    EasingEquations.Easing.QuadEaseIn(
                                        resourceMissingTimeElapsed[i] / resourceMissingAnimDuration));
                                resourceMissingIndicators[i][j].transform.localScale = Vector3.Lerp(
                                    resourceMissingAnimScale * Vector3.one, Vector3.one,
                                    EasingEquations.Easing.QuadEaseIn(
                                        resourceMissingTimeElapsed[i] / resourceMissingAnimDuration));
                                if (resourceMissingTimeElapsed[i] > resourceMissingAnimDuration)
                                {
                                    resourceMissingAnimIncreasing[i] = true;
                                    resourceMissingAnimActive[i] = false;
                                    resourceMissingTimeElapsed[i] = 0;
                                }
                            }
                            if (resourcesMissingLeftToHighlight == 0) break;
                        }
                    }
                }
            }
            for (int j = 0; j < resourceMissingIndicators[i].Length; j++)
            {
                if (resourceSlots[i][j].fillAmount == 1)
                {
                    resourceMissingIndicators[i][j].color = new Color(indicatorColor.r,
                        indicatorColor.g, indicatorColor.b, 0);
                }
            }
        }
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

    public void FailedPlayFromLackOfResources(Player player, int resourceDeficit)
    {
        numResourcesMissing[player.playerNum - 1] = resourceDeficit;
        resourceMissingAnimActive[player.playerNum - 1] = true;
    }

}

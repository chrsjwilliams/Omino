using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {
    
    [SerializeField]
    private GameObject[] interfaceToFlip;
    public Text[] resourceCounters;
    public GameObject[] resourceSlotZones;
    private Image[][] resourceSlots;
    private Image[][] resourceSlotBacks;
    private Image[][] resourceMissingIndicators;
    private Image[][] attackResourceMissingIndicators;
    public GameObject[] attackResourceSlotZones;
    private Image[][] attackResourceSlots;
    private Image[][] attackResourceSlotBacks;
    public RectTransform[] blueprintUIZones;
    public GameObject[] techPowerUpIconArray;
    public GameObject[][] meters;
    [SerializeField]
    private Image[] normalDrawMeters;
    [SerializeField]
    private TextMeshProUGUI[] normalPieceTimers;
    [SerializeField]
    private Image[] destructorDrawMeters;
    [SerializeField]
    private TextMeshProUGUI[] destructorPieceTimers;
    [SerializeField]
    private TextMeshProUGUI[] resourceLevelTexts;
    [SerializeField]
    private TextMeshProUGUI[] normLevelTexts;
    [SerializeField]
    private TextMeshProUGUI[] destLevelTexts;
    [SerializeField]
    private RectTransform[] victoryBanners;
    [SerializeField]
    private RectTransform[] defeatBanners;
    private Vector3[] gameEndBannerStartPositions;
    private Vector3[] gameEndBannerTargetPositions;
    public Transform[] mineBlueprintLocations;
    public Transform[] factoryBlueprintLocations;
    public Transform[] bombFactoryBlueprintLocations;
    public Button[] readyBanners;
    [SerializeField]
    private GameObject pauseMenu;
    [SerializeField]
    private GameObject pauseButton;
    [SerializeField]
    private TextMeshProUGUI replayButtonText;
    private float pauseTimer;
    private const float pauseTimeWindow = 2f;
    [SerializeField]
    private GameObject campaignLevelCompleteMenu;
    [SerializeField]
    private GameObject dungeonRunChallenegeCompleteMenu;
    [SerializeField]
    private GameObject optionsMenu;
    public Transform canvas;
    public Sprite destructorIcon;
    public Sprite splashIcon;
    public Sprite[] factoryBottoms;
    public Sprite[] factoryTops;
    public Sprite[] factoryIcons;
    public Sprite[] mineBottoms;
    public Sprite[] mineTops;
    public Sprite[] mineIcons;
    public Sprite baseBottom;
    public Sprite baseOverlay;
    public Sprite sideBaseOverlay;
    public Sprite[] bombFactoryBottoms;
    public Sprite[] bombFactoryTops;
    public Sprite[] bombFactoryIcons;
    public Sprite structureOverlay;
    private bool scrollingInBanners;
    private Player winner;
    private float bannerScrollTimeElapsed;
    [SerializeField]
    private float bannerScrollDuration;
    private List<int> touchIdsMakingTooltips;
    [SerializeField]
    private Color[] notReadyColors;
    [SerializeField]
    private Color readyColor;
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
    private float[] attackResourceGainHighlightTimeElapsed;
    private int[] attackResourceGainHighlightIndices;
    private bool[] attackResourceGainHighlightActive;
    private bool[] attackResourceGainHighlightIncreasing;
    private int[] lastResourceCount;
    private int[] lastAttackResourceCount;
    [SerializeField]
    private float resourceMissingAnimDuration;
    [SerializeField]
    private float resourceMissingAnimScale;
    private float[] resourceMissingTimeElapsed;
    private int[] numResourcesMissing;
    private bool[] resourceMissingAnimActive;
    private bool[] resourceMissingAnimIncreasing;
    private float[] attackResourceMissingTimeElapsed;
    private int[] numAttackResourcesMissing;
    private bool[] attackResourceMissingAnimActive;
    private bool[] attackResourceMissingAnimIncreasing;
    public bool tooltipsDisabled;
    public Color legalGlowColor;
    public Color notLegalGlowColor;
    private const float energyUIFillMin = 0.13f;
    private const float energyUIFillMax = 0.9f;
    private const float attackUIFillMin = 0.08f;
    private const float attackUIFillMax = 0.94f;

    public EloInGameUiManager eloUIManager;
    public DungeonRunInGameUIManager dungeonRunUIManager;

    private void Awake()
    {
        Image[] slotsP1 = resourceSlotZones[0].GetComponentsInChildren<Image>();
        Image[] slotsP2 = resourceSlotZones[1].GetComponentsInChildren<Image>();
        Image[] slotTopsP1 = new Image[slotsP1.Length / 3];
        Image[] slotTopsP2 = new Image[slotsP2.Length / 3];
        Image[] slotBacksP1 = new Image[slotsP1.Length / 3];
        Image[] slotBacksP2 = new Image[slotsP2.Length / 3];
        Image[] attackSlotsP1 = attackResourceSlotZones[0].GetComponentsInChildren<Image>();
        Image[] attackSlotsP2 = attackResourceSlotZones[1].GetComponentsInChildren<Image>();
        Image[] attackSlotTopsP1 = new Image[attackSlotsP1.Length / 3];
        Image[] attackSlotTopsP2 = new Image[attackSlotsP2.Length / 3];
        Image[] attackSlotBacksP1 = new Image[attackSlotsP1.Length / 3];
        Image[] attackSlotBacksP2 = new Image[attackSlotsP2.Length / 3];
        Image[] missingIndicatorsP1 = new Image[slotsP1.Length / 3];
        Image[] missingIndicatorsP2 = new Image[slotsP1.Length / 3];
        Image[] attackMissingIndicatorsP1 = new Image[attackSlotsP1.Length / 3];
        Image[] attackMissingIndicatorsP2 = new Image[attackSlotsP2.Length / 3];
        for (int attackTrue = 0; attackTrue < 2; attackTrue++)
        {
            Image[] slotArrayP1 = attackTrue == 0 ? attackSlotsP1 : slotsP1;
            Image[] slotArrayP2 = attackTrue == 0 ? attackSlotsP2 : slotsP2;
            Image[] slotTopArrayP1 = attackTrue == 0 ? attackSlotTopsP1 : slotTopsP1;
            Image[] slotTopArrayP2 = attackTrue == 0 ? attackSlotTopsP2 : slotTopsP2;
            Image[] slotBackArrayP1 = attackTrue == 0 ? attackSlotBacksP1 : slotBacksP1;
            Image[] slotBackArrayP2 = attackTrue == 0 ? attackSlotBacksP2 : slotBacksP2;
            Image[] missingIndicatorArrayP1 = attackTrue == 0 ? 
                attackMissingIndicatorsP1 : missingIndicatorsP1;
            Image[] missingIndicatorArrayP2 = attackTrue == 0 ? 
                attackMissingIndicatorsP2 : missingIndicatorsP2;

            for (int i = 0; i < slotArrayP1.Length; i++)
            {
                if (i % 3 == 1)
                {
                    slotTopArrayP1[i / 3] = slotArrayP1[i];
                    slotTopArrayP2[i / 3] = slotArrayP2[i];
                    slotTopArrayP1[i / 3].color = Services.GameManager.Player1ColorScheme[0];
                    slotTopArrayP2[i / 3].color = Services.GameManager.Player2ColorScheme[0];
                }
                else if (i % 3 == 0)
                {
                    slotBackArrayP1[i / 3] = slotArrayP1[i];
                    slotBackArrayP2[i / 3] = slotArrayP2[i];
                    slotArrayP1[i].color = Services.GameManager.Player1ColorScheme[0];
                    slotArrayP2[i].color = Services.GameManager.Player2ColorScheme[0];
                }
                else if (i % 3 == 2)
                {
                    missingIndicatorArrayP1[i / 3] = slotArrayP1[i];
                    missingIndicatorArrayP2[i / 3] = slotArrayP2[i];
                    Color indicatorColor = missingIndicatorArrayP1[i / 3].color;
                    missingIndicatorArrayP1[i / 3].color = new Color(indicatorColor.r,
                        indicatorColor.g, indicatorColor.b, 0);
                    missingIndicatorArrayP2[i / 3].color = new Color(indicatorColor.r,
                        indicatorColor.g, indicatorColor.b, 0);
                }
            }

        }


        resourceSlots = new Image[][] { slotTopsP1, slotTopsP2 };
        resourceSlotBacks = new Image[][] { slotBacksP1, slotBacksP2 };
        attackResourceSlots = new Image[][] { attackSlotTopsP1, attackSlotTopsP2 };
        attackResourceSlotBacks = new Image[][] { attackSlotBacksP1, attackSlotBacksP2 };
        resourceMissingIndicators = new Image[][] { missingIndicatorsP1, missingIndicatorsP2 };
        attackResourceMissingIndicators = new Image[][] { attackMissingIndicatorsP1, attackMissingIndicatorsP2 };
        resourceGainHighlightIndices = new int[2] { 0, 0 };
        attackResourceGainHighlightIndices = new int[2] { 0, 0 };
        resourceGainHighlightTimeElapsed = new float[2] { 0, 0 };
        attackResourceGainHighlightTimeElapsed = new float[2] { 0, 0 };
        resourceGainHighlightActive = new bool[2] { false, false };
        attackResourceGainHighlightActive = new bool[2] { false, false };
        resourceGainHighlightIncreasing = new bool[2] { true, true };
        attackResourceGainHighlightIncreasing = new bool[2] { true, true };
        lastResourceCount = new int[2] { 0, 0 };
        lastAttackResourceCount = new int[2] { 0, 0 };
        resourceMissingAnimActive = new bool[2] { false, false };
        attackResourceMissingAnimActive = new bool[2] { false, false };
        resourceMissingAnimIncreasing = new bool[2] { true, true };
        attackResourceMissingAnimIncreasing = new bool[2] { true, true };
        resourceMissingTimeElapsed = new float[2] { 0, 0 };
        attackResourceMissingTimeElapsed = new float[2] { 0, 0 };
        numResourcesMissing = new int[2] { 0, 0 };
        numAttackResourcesMissing = new int[2] { 0, 0 };

        meters = new GameObject[2][]
        {
            new GameObject[4]
            {
                resourceCounters[0].gameObject,
                normalDrawMeters[0].transform.parent.gameObject,
                destructorDrawMeters[0].transform.parent.gameObject,
                techPowerUpIconArray[0].transform.parent.gameObject
            },
            new GameObject[3]
            {
                resourceCounters[1].gameObject,
                normalDrawMeters[1].transform.parent.gameObject,
                destructorDrawMeters[1].transform.parent.gameObject
            }
        };
        foreach(GameObject[] meterArray in meters)
        {
            foreach(GameObject obj in meterArray)
            {
                obj.SetActive(false);
            }
        }
    }

    // Use this for initialization
    void Start() {

        for (int i = 0; i < victoryBanners.Length; i++)
        {
            victoryBanners[i].gameObject.SetActive(false);
            defeatBanners[i].gameObject.SetActive(false);
            readyBanners[i].gameObject.SetActive(false);
        }
        touchIdsMakingTooltips = new List<int>();
        for (int i = 0; i < 2; i++)
        {
            UpdateDrawMeters(i + 1, 0, 0, 0, 0);
            if (!Services.GameManager.destructorsEnabled)
            {
                destructorDrawMeters[i].gameObject.SetActive(false);
                destructorPieceTimers[i].gameObject.SetActive(false);
                destLevelTexts[i].gameObject.SetActive(false);
            }
        }

        optionsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        campaignLevelCompleteMenu.SetActive(false);

        for (int i = 0; i < readyBanners.Length; i++)
        {
            if (Services.GameManager.Players[i] is AIPlayer)
                readyBanners[i].enabled = false;
            readyBanners[i].GetComponent<Image>().color = Services.GameManager.colorSchemes[i][0]; //notReadyColors[playerNum-1];

        }
        if (Services.GameManager.disableUI)
        {
            for (int i = 0; i < 2; i++)
            {
                normalDrawMeters[i].enabled = false;
                normalPieceTimers[i].enabled = false;
                destructorDrawMeters[i].enabled = false;
                destructorPieceTimers[i].enabled = false;
                destLevelTexts[i].enabled = false;
                normLevelTexts[i].enabled = false;
                resourceLevelTexts[i].enabled = false;
                resourceSlotZones[i].gameObject.SetActive(false);
            }
            pauseButton.SetActive(false);
        }

        if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
        {
            replayButtonText.text = "FORFEIT";
        }
    }
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKeyDown(KeyCode.Y)) StartBannerScroll(Services.GameManager.Players[0]);
        if (scrollingInBanners) ScrollBanners();
        HighlightResourceGained();
        HighlightResourcesMissing();
        HighlightResourceGained(true);
        HighlightResourcesMissing(true);
        if (Input.GetKeyDown(KeyCode.P)) Debug.Break();
        //if (Input.GetKeyDown(KeyCode.M)) ShowCampaignLevelCompleteMenu(Services.GameManager.Players[0]);
        if(pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
        }
	}

    public void UIForSinglePlayer(bool singlePlayer)
    {
        float bannerRotation = singlePlayer ? 0 : 180;
        float uiElementRotation = singlePlayer ? 180 : 0;

        foreach (GameObject uiElemt in interfaceToFlip)
        {

            if (uiElemt.name.Contains("Banner"))
            {
                uiElemt.transform.localRotation = Quaternion.Euler(0, 0, bannerRotation);
            }
            else
            {
                uiElemt.transform.localRotation = Quaternion.Euler(0, 0, uiElementRotation);
            }       
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
    }

    public void StartBannerScroll(Player winner_)
    {
        scrollingInBanners = true;
        winner = winner_;
        gameEndBannerStartPositions = new Vector3[2];
        gameEndBannerTargetPositions = new Vector3[2];
        for (int i = 0; i < victoryBanners.Length; i++)
        {
            RectTransform banner;
            if (i == winner.playerNum - 1)
            {
                banner = victoryBanners[i];
            }
            else
            {
                banner = defeatBanners[i];
            }
            gameEndBannerTargetPositions[i] = banner.localPosition;
            banner.gameObject.SetActive(true);
            Vector3 offset = banner.sizeDelta.y * Vector3.down;
            if (i == 1) offset *= -1;
            banner.localPosition += offset;
            gameEndBannerStartPositions[i] = banner.localPosition;
        }
        bannerScrollTimeElapsed = 0;
    }

    void ScrollBanners()
    {
        bannerScrollTimeElapsed += Time.deltaTime;
        for (int i = 0; i < victoryBanners.Length; i++)
        {
            RectTransform banner;
            if (i == winner.playerNum - 1)
            {
                banner = victoryBanners[i];
            }
            else
            {
                banner = defeatBanners[i];
            }
            float progress = bannerScrollTimeElapsed / bannerScrollDuration;
            banner.localPosition = Vector3.Lerp(
                gameEndBannerStartPositions[i], 
                gameEndBannerTargetPositions[i],
                    EasingEquations.Easing.QuadEaseOut(progress));
        }
        if (bannerScrollTimeElapsed >= bannerScrollDuration)
        {
            scrollingInBanners = false;
        }
    }

    public void UpdateResourceMeter(int playerNum, float fillProportion, bool attack = false)
    {
        Image[] slotFronts = attack ? 
            attackResourceSlots[playerNum - 1] : resourceSlots[playerNum - 1];
        Image[] slotBacks = attack ? 
            attackResourceSlotBacks[playerNum - 1] : resourceSlotBacks[playerNum - 1];
        float fillMin = attack ? attackUIFillMin : energyUIFillMin;
        float fillMax = attack ? attackUIFillMax : energyUIFillMax;

        for (int i = 0; i < slotFronts.Length; i++)
        {
            Image slotImage = slotFronts[i];
            Color slotColor = slotImage.color;
            if (slotImage.fillAmount < 1)
            {
                slotImage.color = new Color(slotColor.r, slotColor.g,
                    slotColor.b, 0.5f);
                slotImage.fillAmount = fillMin + ((fillMax - fillMin)*
                    EasingEquations.Easing.QuadEaseIn(fillProportion));
                slotBacks[i].color = new Color(slotColor.r, slotColor.g, slotColor.b, 1);
                break;
            }
        }
    }

    public void UpdateResourceCount(int resourceCount, int maxResources, Player player, bool attack = false)
    {
        int playerIndex = player.playerNum - 1;
        Image[] slotFronts = attack ? attackResourceSlots[playerIndex] : resourceSlots[playerIndex];
        Image[] slotBacks = attack ? attackResourceSlotBacks[playerIndex] : resourceSlotBacks[playerIndex];
        int lastCount = attack ? lastAttackResourceCount[playerIndex] : lastResourceCount[playerIndex];
        int[] highlightIndices = attack ? attackResourceGainHighlightIndices : resourceGainHighlightIndices;
        bool[] highlightsActive = attack ? attackResourceGainHighlightActive : resourceGainHighlightActive;
        int[] resourcesMissing = attack ? numAttackResourcesMissing : numResourcesMissing;
        for (int i = 0; i < slotFronts.Length; i++)
        {
            Image slotImage = slotFronts[i];
            Image slotBack = slotBacks[i];
            Color slotColor = new Color(slotImage.color.r,
                    slotImage.color.g, slotImage.color.b, 1);
            if (i < resourceCount)
            {
                slotImage.fillAmount = 1;
                slotImage.color = slotColor;
                //slotBack.color = slotColor;
                if (resourceCount > lastCount && i == resourceCount - 1)
                {
                    highlightIndices[playerIndex] = i;
                    highlightsActive[playerIndex] = true;
                }
            }
            else
            {
                slotImage.fillAmount = 0;
                //slotBack.color = new Color(slotColor.r, slotColor.g, slotColor.b, 0);
            }
        }
        if(resourceCount != lastCount
            && resourcesMissing[playerIndex] != 0)
        {
            resourcesMissing[playerIndex] = Mathf.Max(0,
                resourcesMissing[playerIndex]
                - (resourceCount - lastCount));
        }
        int[] lastResourceCountArray = attack ? lastAttackResourceCount : lastResourceCount;
        lastResourceCountArray[playerIndex] = resourceCount;
    }

    private void HighlightResourceGained(bool attack = false)
    {
        bool[] highlightsActive = attack ? 
            attackResourceGainHighlightActive : resourceGainHighlightActive;
        float[] highlightsTimeElapsed = attack ? 
            attackResourceGainHighlightTimeElapsed : resourceGainHighlightTimeElapsed;
        bool[] highlightsIncreasing = attack ?
            attackResourceGainHighlightIncreasing : resourceGainHighlightIncreasing;
        Image[][] slotBacks = attack ?
            attackResourceSlotBacks : resourceSlotBacks;
        int[] highlightIndices = attack ?
            attackResourceGainHighlightIndices : resourceGainHighlightIndices;

        for (int i = 0; i < 2; i++)
        {
            if (highlightsActive[i])
            {
                highlightsTimeElapsed[i] += Time.deltaTime;
                if (highlightsIncreasing[i])
                {
                    Vector3 scale = Vector3.Lerp(Vector3.one, resourceGainHighlightScale * Vector3.one,
                        EasingEquations.Easing.QuadEaseOut(
                            highlightsTimeElapsed[i] / resourceGainHighlightDuration));
                    slotBacks[i][highlightIndices[i]].transform.localScale = scale;

                    if(highlightsTimeElapsed[i] > resourceGainHighlightDuration)
                    {
                        highlightsIncreasing[i] = false;
                        highlightsTimeElapsed[i] = 0;
                    }
                }
                else
                {
                    slotBacks[i][highlightIndices[i]].transform.localScale =
                        Vector3.Lerp(resourceGainHighlightScale * Vector3.one, Vector3.one,
                        EasingEquations.Easing.QuadEaseIn(
                            highlightsTimeElapsed[i] / resourceGainHighlightDuration));
                    if(highlightsTimeElapsed[i] > resourceGainHighlightDuration)
                    {
                        highlightsActive[i] = false;
                        highlightsIncreasing[i] = true;
                        highlightsTimeElapsed[i] = 0;
                    }
                }
            }
        }

    }

    private void HighlightResourcesMissing(bool attack = false)
    {
        bool[] missingResourceAnimActive = attack ? attackResourceMissingAnimActive :
            resourceMissingAnimActive;
        int[] resourcesMissing = attack ? numAttackResourcesMissing : numResourcesMissing;
        Image[][] indicators = attack ? attackResourceMissingIndicators : resourceMissingIndicators;
        Image[][] slotFronts = attack ? attackResourceSlots : resourceSlots;
        float[] timesElapsed = attack ? attackResourceMissingTimeElapsed : resourceMissingTimeElapsed;
        bool[] increasing = attack ? attackResourceMissingAnimIncreasing : resourceMissingAnimIncreasing;
        for (int i = 0; i < 2; i++)
        {
            Color indicatorColor = indicators[i][0].color;
            if (missingResourceAnimActive[i])
            {
                int resourcesMissingLeftToHighlight = resourcesMissing[i];
                if (resourcesMissingLeftToHighlight == 0)
                {
                    for (int j = 0; j < slotFronts[i].Length; j++)
                    {
                        indicators[i][j].color = new Color(indicatorColor.r,
                            indicatorColor.g, indicatorColor.b, 0);
                    }
                }
                else
                {
                    timesElapsed[i] += Time.deltaTime;
                    for (int j = 0; j < slotFronts[i].Length; j++)
                    {
                        if (slotFronts[i][j].fillAmount != 1)
                        {
                            resourcesMissingLeftToHighlight -= 1;
                            if (increasing[i])
                            {
                                indicators[i][j].color = Color.Lerp(
                                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 0),
                                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 1),
                                    EasingEquations.Easing.QuadEaseOut(
                                        timesElapsed[i] / resourceMissingAnimDuration));
                                indicators[i][j].transform.localScale = Vector3.Lerp(
                                    Vector3.one, resourceMissingAnimScale * Vector3.one,
                                    EasingEquations.Easing.QuadEaseOut(
                                        timesElapsed[i] / resourceMissingAnimDuration));
                                if (timesElapsed[i] > resourceMissingAnimDuration)
                                {
                                    increasing[i] = false;
                                    timesElapsed[i] = 0;
                                }
                            }
                            else
                            {
                                indicators[i][j].color = Color.Lerp(
                                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 1),
                                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 0),
                                    EasingEquations.Easing.QuadEaseIn(
                                        timesElapsed[i] / resourceMissingAnimDuration));
                                indicators[i][j].transform.localScale = Vector3.Lerp(
                                    resourceMissingAnimScale * Vector3.one, Vector3.one,
                                    EasingEquations.Easing.QuadEaseIn(
                                        timesElapsed[i] / resourceMissingAnimDuration));
                                if (timesElapsed[i] > resourceMissingAnimDuration)
                                {
                                    increasing[i] = true;
                                    missingResourceAnimActive[i] = false;
                                    timesElapsed[i] = 0;
                                }
                            }
                            if (resourcesMissingLeftToHighlight == 0) break;
                        }
                    }
                }
            }
            for (int j = 0; j < indicators[i].Length; j++)
            {
                if (slotFronts[i][j].fillAmount == 1)
                {
                    indicators[i][j].color = new Color(indicatorColor.r,
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
            readyBanners[playerNum - 1].GetComponentInChildren<TextMeshProUGUI>().text =
                "READY";
            readyBanners[playerNum - 1].GetComponent<Image>().color = readyColor;
            Services.AudioManager.PlaySoundEffect(Services.Clips.UIReadyOn, 1.0f);
        }
        else
        {
            readyBanners[playerNum - 1].GetComponentInChildren<TextMeshProUGUI>().text =
                "READY?";
            readyBanners[playerNum - 1].GetComponent<Image>().color = Services.GameManager.colorSchemes[playerNum - 1][0]; //notReadyColors[playerNum-1];
            Services.AudioManager.PlaySoundEffect(Services.Clips.UIReadyOff, 1.0f);
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
            for (int i = 0; i < readyBanners.Length; i++)
            {
                readyBanners[i].enabled = false;
            }
            Services.GameScene.StartGameSequence();
        }
    }



    public void TogglePauseMenu()
    {
        if (Services.GameScene.gamePaused)
        {
            AudioListener.volume = 1.0f;
            pauseMenu.SetActive(false);
            Services.GameScene.UnpauseGame();
        }
        else //if (pauseTimer > 0)
        {
            AudioListener.volume = 0.55f;
            pauseMenu.SetActive(true);
            Services.GameScene.PauseGame();
            //pauseTimer = 0;
        }
        //else
        //{
        //    pauseTimer = pauseTimeWindow;
        //}

    }

    public void ToggleOptionsMenu(bool state)
    {
        if (state)
        {
            optionsMenu.SetActive(true);
            pauseMenu.SetActive(false);

            _SFXButtonAppearanceToggle();

            if (!Services.GameManager.MusicEnabled)
            {
                GameObject.Find("ToggleMusic").GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            }
        }
        else
        {
            optionsMenu.SetActive(false);
            pauseMenu.SetActive(true);
        }
    }

    public void ToggleSoundFX()
    {
        Services.AudioManager.ToggleSoundEffects();
        _SFXButtonAppearanceToggle();
    }
    
    public void ToggleMusic()
    {
        Services.AudioManager.ToggleMusic();
        _MusicButtonAppearanceToggle();
    }

    private void _SFXButtonAppearanceToggle()
    {
        GameObject button = GameObject.Find("ToggleSound");

        if (Services.GameManager.SoundEffectsEnabled)
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[0];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Sound FX";

        }
        else
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>Sound FX</s>";
        }
    }

    private void _MusicButtonAppearanceToggle()
    {
        GameObject button = GameObject.Find("ToggleMusic");

        if (Services.GameManager.MusicEnabled)
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[0];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "Music";

        }
        else
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>Music</s>";
        }
    }
    

    public void FailedPlayFromLackOfResources(Player player, int resourceDeficit)
    {
        numResourcesMissing[player.playerNum - 1] = resourceDeficit;
        resourceMissingAnimActive[player.playerNum - 1] = true;
    }

    public void UpdateResourceLevel(int level, int playerNum)
    {
        UpdateLevelText(resourceLevelTexts[playerNum - 1], level);
    }

    public void UpdateNormLevel(int level, int playerNum)
    {
        UpdateLevelText(normLevelTexts[playerNum - 1], level);
    }

    public void UpdateDestLevel(int level, int playerNum)
    {
        UpdateLevelText(destLevelTexts[playerNum - 1], level);
    }

    private void UpdateLevelText(TextMeshProUGUI uiText, int level)
    {
        int prevLevel = 1;
        int.TryParse(uiText.text.Split(' ')[1], out prevLevel);
        if(level > prevLevel)
            uiText.GetComponent<ProdLevelText>().StartHighlight();
        uiText.text = "LV " + level;
    }

    private void HighlightProdLevelIncrease(TextMeshProUGUI uiText)
    {

    }

    public void ShowCampaignLevelCompleteMenu(Player winner)
    {
        campaignLevelCompleteMenu.GetComponent<CampaignMenuManager>().Show(winner);
    }

    public void OnGameEndBannerTouch()
    {
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo ||
            Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            Services.GameScene.ReturnToLevelSelect();
        }
        else
        {
            TogglePauseMenu();
        }
    }
}

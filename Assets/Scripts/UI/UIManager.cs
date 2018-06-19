﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {

    public Text[] resourceCounters;
    public GameObject[] resourceSlotZones;
    private Image[][] resourceSlots;
    private Image[][] resourceSlotBacks;
    private Image[][] resourceMissingIndicators;
    public RectTransform[] blueprintUIZones;
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
    private float pauseTimer;
    private const float pauseTimeWindow = 2f;
    [SerializeField]
    private GameObject campaignLevelCompleteMenu;
    [SerializeField]
    private GameObject optionsMenu;
    public Transform canvas;
    public Sprite blueprintTile;
    public Sprite destructorIcon;
    public Sprite drillIcon;
    public Sprite gearIcon;
    public Sprite bricksIcon;
    public Sprite bigBombIcon;
    public Sprite splashStructIcon;
    public Sprite splashIcon;
    public Sprite shieldIcon;
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
    private int[] lastResourceCount;
    [SerializeField]
    private float resourceMissingAnimDuration;
    [SerializeField]
    private float resourceMissingAnimScale;
    private float[] resourceMissingTimeElapsed;
    private int[] numResourcesMissing;
    private bool[] resourceMissingAnimActive;
    private bool[] resourceMissingAnimIncreasing;
    public bool tooltipsDisabled;
    public Color legalGlowColor;
    public Color notLegalGlowColor;

    private void Awake()
    {
        Image[] slotsP1 = resourceSlotZones[0].GetComponentsInChildren<Image>();
        Image[] slotsP2 = resourceSlotZones[1].GetComponentsInChildren<Image>();
        Image[] slotTopsP1 = new Image[slotsP1.Length / 3];
        Image[] slotTopsP2 = new Image[slotsP1.Length / 3];
        Image[] slotBacksP1 = new Image[slotsP1.Length / 3];
        Image[] slotBacksP2 = new Image[slotsP1.Length / 3];
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
            else if(i % 3 == 0)
            {
                slotBacksP1[i / 3] = slotsP1[i];
                slotBacksP2[i / 3] = slotsP2[i];
                slotsP1[i].color = Services.GameManager.Player1ColorScheme[0];
                slotsP2[i].color = Services.GameManager.Player2ColorScheme[0];
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
        resourceSlotBacks = new Image[][] { slotBacksP1, slotBacksP2 };
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

        meters = new GameObject[2][]
        {
            new GameObject[3]
            {
                resourceCounters[0].gameObject,
                normalDrawMeters[0].transform.parent.gameObject,
                destructorDrawMeters[0].transform.parent.gameObject
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
    void Start () {

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
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKeyDown(KeyCode.Y)) StartBannerScroll(Services.GameManager.Players[0]);
        if (scrollingInBanners) ScrollBanners();
        HighlightResourceGained();
        HighlightResourcesMissing();
        if (Input.GetKeyDown(KeyCode.P)) Debug.Break();
        //if (Input.GetKeyDown(KeyCode.M)) ShowCampaignLevelCompleteMenu(Services.GameManager.Players[0]);
        if(pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
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

    public void UpdateResourceMeter(int playerNum, float fillProportion)
    {
        for (int i = 0; i < resourceSlots[playerNum - 1].Length; i++)
        {
            Image slotImage = resourceSlots[playerNum - 1][i];
            Color slotColor = slotImage.color;
            if (slotImage.fillAmount < 1)
            {
                slotImage.color = new Color(slotColor.r, slotColor.g,
                    slotColor.b, 0.5f);
                slotImage.fillAmount =
                    EasingEquations.Easing.QuadEaseIn(fillProportion);
                resourceSlotBacks[playerNum - 1][i].color = new Color(slotColor.r, slotColor.g, slotColor.b, 1);
                break;
            }
        }
    }

    public void UpdateResourceCount(int resourceCount, int maxResources, Player player)
    {
        int playerIndex = player.playerNum - 1;
        for (int i = 0; i < resourceSlots[playerIndex].Length; i++)
        {
            Image slotImage = resourceSlots[playerIndex][i];
            Image slotBack = resourceSlotBacks[playerIndex][i];
            Color slotColor = new Color(slotImage.color.r,
                    slotImage.color.g, slotImage.color.b, 1);
            if (i < resourceCount)
            {
                slotImage.fillAmount = 1;
                slotImage.color = slotColor;
                //slotBack.color = slotColor;
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
                //slotBack.color = new Color(slotColor.r, slotColor.g, slotColor.b, 0);
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
            readyBanners[playerNum - 1].GetComponentInChildren<TextMeshProUGUI>().text =
                "READY";
            readyBanners[playerNum - 1].GetComponent<Image>().color = readyColor;
            Services.AudioManager.RegisterSoundEffect(Services.Clips.UIReadyOn, 1.0f);
        }
        else
        {
            readyBanners[playerNum - 1].GetComponentInChildren<TextMeshProUGUI>().text =
                "READY?";
            readyBanners[playerNum - 1].GetComponent<Image>().color = Services.GameManager.colorSchemes[playerNum - 1][0]; //notReadyColors[playerNum-1];
            Services.AudioManager.RegisterSoundEffect(Services.Clips.UIReadyOff, 1.0f);
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
            pauseMenu.SetActive(false);
            Services.GameScene.UnpauseGame();
        }
        else //if (pauseTimer > 0)
        {
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
        resourceLevelTexts[playerNum - 1].text = "LV " + level;
    }

    public void UpdateNormLevel(int level, int playerNum)
    {
        normLevelTexts[playerNum - 1].text = "LV " + level;
    }

    public void UpdateDestLevel(int level, int playerNum)
    {
        destLevelTexts[playerNum - 1].text = "LV " + level;
    }

    public void ShowCampaignLevelCompleteMenu(Player winner)
    {
        campaignLevelCompleteMenu.GetComponent<CampaignMenuManager>().Show(winner);
    }
}

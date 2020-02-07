
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMeters : MonoBehaviour
{
    public int playerNum;
    public Text resourceCounter;
    public GameObject resourceSlotZone;
    //public Image[] resourceSlot;
    //public Image[] resourceSlotBack;
    //public Image[] resourceMissingIndicator;
    //public Image[] attackResourceMissingIndicator;
    public GameObject attackResourceSlotZone;
    //public Image[] attackResourceSlot;
    //public Image[] attackResourceSlotBack;

    //public Image[][] resourceSlots;
    //public Image[][] resourceSlotBacks;
    //public Image[][] resourceMissingIndicators;
    //public Image[][] attackResourceMissingIndicators;
    //public Image[][] attackResourceSlots;
    //public Image[][] attackResourceSlotBacks;

    public Transform blueprintUIZone;
    public GameObject techPowerUpIconArray;
    public GameObject[] meters { get; private set; }
    [SerializeField]
    private Image normalDrawMeter;
    [SerializeField]
    private TextMeshProUGUI normalPieceTimer;
    [SerializeField]
    private Image destructorDrawMeter;
    [SerializeField]
    private TextMeshProUGUI destructorPieceTimer;
    [SerializeField]
    private TextMeshProUGUI resourceLevelText;
    [SerializeField]
    private TextMeshProUGUI normLevelText;
    [SerializeField]
    private TextMeshProUGUI destLevelText;


    public RectTransform mineBlueprintLocation;
    public RectTransform factoryBlueprintLocation;
    public RectTransform bombFactoryBlueprintLocation;

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
    private int lastResourceCount;
    private int lastAttackResourceCount;
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
    private const float energyUIFillMin = 0.13f;
    private const float energyUIFillMax = 0.9f;
    private const float attackUIFillMin = 0.08f;
    private const float attackUIFillMax = 0.94f;

    [SerializeField]
    private float radialMeterFillMin;
    [SerializeField]
    private float radialMeterFillMax;

    private float[] timesElapsed = new float[] { 0, 0 };
    private float[] highlightsTimeElapsed = new float[] { 0, 0};
    [SerializeField]
    private float resourceGainHighlightScale;

    private ResourceIcon[] energyIcons;
    private ResourceIcon[] attackIcons;

    // Use this for initialization
    void Awake ()
    {
        energyIcons = resourceSlotZone.GetComponentsInChildren<ResourceIcon>();
        attackIcons = attackResourceSlotZone.GetComponentsInChildren<ResourceIcon>();
        foreach (ResourceIcon icon in energyIcons) icon.Init(playerNum);
        foreach (ResourceIcon icon in attackIcons) icon.Init(playerNum);

        //Image[] slots = resourceSlotZone.GetComponentsInChildren<Image>();
        //Image[] slotTops = new Image[slots.Length / 3];
        //Image[] slotBacks = new Image[slots.Length / 3];
        //Image[] attackSlots = attackResourceSlotZone.GetComponentsInChildren<Image>();
        //Image[] attackSlotTops = new Image[attackSlots.Length / 3];
        //Image[] attackSlotBacks = new Image[attackSlots.Length / 3];
        //Image[] missingIndicators = new Image[slots.Length / 3];
        //Image[] attackMissingIndicators = new Image[attackSlots.Length / 3];

        //Color[][] colorScheme = Services.GameManager.GetColorScheme();

        //for (int attackTrue = 0; attackTrue < 2; attackTrue++)
        //{
        //    Image[] slotArray = attackTrue == 0 ? attackSlots : slots;
        //    Image[] slotTopArray = attackTrue == 0 ? attackSlotTops : slotTops;
        //    Image[] slotBackArray = attackTrue == 0 ? attackSlotBacks : slotBacks;
        //    Image[] missingIndicatorArray = attackTrue == 0 ?
        //        attackMissingIndicators : missingIndicators;

        //    for (int i = 0; i < slotArray.Length; i++)
        //    {
        //        if (i % 3 == 0)
        //        {
        //            slotTopArray[i / 3] = slotArray[i];
        //            slotTopArray[i / 3].color = colorScheme[playerNum - 1][0];
        //        }
        //        else if (i % 3 == 1)
        //        {
        //            slotBackArray[i / 3] = slotArray[i];
        //            slotArray[i].color = colorScheme[playerNum - 1][0];
        //        }
        //        else if (i % 3 == 2)
        //        {
        //            missingIndicatorArray[i / 3] = slotArray[i];
        //            Color indicatorColor = missingIndicatorArray[i / 3].color;
        //            missingIndicatorArray[i / 3].color = new Color(indicatorColor.r,
        //                indicatorColor.g, indicatorColor.b, 0);
        //        }
        //    }

        //}

        //resourceSlots = new Image[][] { slotTops };
        //resourceSlotBacks = new Image[][] { slotBacks};
        //attackResourceSlots = new Image[][] { attackSlotTops};
        //attackResourceSlotBacks = new Image[][] { attackSlotBacks};
        //resourceMissingIndicators = new Image[][] { missingIndicators };
        //attackResourceMissingIndicators = new Image[][] { attackMissingIndicators};
        //resourceGainHighlightIndices = new int[] { 0 };
        //attackResourceGainHighlightIndices = new int[] { 0 };
        //resourceGainHighlightTimeElapsed = new float[] {  0 };
        //attackResourceGainHighlightTimeElapsed = new float[] {0 };
        //resourceGainHighlightActive = new bool[] {false };
        //attackResourceGainHighlightActive = new bool[] {false };
        //resourceGainHighlightIncreasing = new bool[] {true };
        //attackResourceGainHighlightIncreasing = new bool[] { true };
        lastResourceCount = 0;
        lastAttackResourceCount = 0;
        //resourceMissingAnimActive = new bool[] { false };
        //attackResourceMissingAnimActive = new bool[] {false };
        //resourceMissingAnimIncreasing = new bool[] { true };
        //attackResourceMissingAnimIncreasing = new bool[] { true };
        //resourceMissingTimeElapsed = new float[] { 0 };
        //attackResourceMissingTimeElapsed = new float[] { 0 };
        //numResourcesMissing = new int[] { 0 };
        //numAttackResourcesMissing = new int[] { 1 };


        meters = new GameObject[]
        {
                resourceCounter.gameObject,
                normalDrawMeter.transform.parent.gameObject,
                techPowerUpIconArray.transform.parent.gameObject,
                destructorDrawMeter.transform.parent.gameObject
        };

        foreach (GameObject obj in meters)
        {
            obj.SetActive(false);
        }
    }

    private void Start()
    {
        Color textColor = Color.white;
        if (Services.GameManager.mode == TitleSceneScript.GameMode.HyperSOLO ||
            Services.GameManager.mode == TitleSceneScript.GameMode.HyperVS)
        {
            textColor = Color.white;
        }


        UpdateDrawMeters(0, 0, 0, 0);
        if (!Services.GameManager.destructorsEnabled)
        {
            destructorDrawMeter.gameObject.SetActive(false);
            destructorPieceTimer.gameObject.SetActive(false);
            destLevelText.gameObject.SetActive(false);
        }
        

        for (int i = 0; i < 2; i++)
        {
            normLevelText.color = textColor;
            normalPieceTimer.color = textColor;
            resourceLevelText.color = textColor;
            destLevelText.color = textColor;

        }

        if (Services.GameManager.disableUI)
        {

            normalDrawMeter.enabled = false;
            normalPieceTimer.enabled = false;
            destructorDrawMeter.enabled = false;
            destructorPieceTimer.enabled = false;
            destLevelText.enabled = false;
            normLevelText.enabled = false;
            resourceLevelText.enabled = false;
            resourceSlotZone.gameObject.SetActive(false);

        }

        // pigeon:  to fix the dragging issue on iPadPRos i flipped the position of buildings and
        //          their meters
        if (Services.GameManager.CurrentDevice == DEVICE.IPAD_PRO)
        {
            resourceSlotZone.GetComponent<RectTransform>().localScale = new Vector2(0.5f, 0.5f);
            attackResourceSlotZone.GetComponent<RectTransform>().localScale = new Vector2(0.5f, 0.5f);

            normLevelText.GetComponent<RectTransform>().localScale = new Vector2(0.75f, 0.75f);
            resourceLevelText.GetComponent<RectTransform>().localScale = new Vector2(0.75f, 0.75f);
            destLevelText.GetComponent<RectTransform>().localScale = new Vector2(0.75f, 0.75f);

        }
    }

    //private void HighlightResourceGained(bool attack = false)
    //{
    //    bool[] highlightsActive = attack ?
    //        attackResourceGainHighlightActive : resourceGainHighlightActive;
    //    float[] highlightsTimeElapsed = attack ?
    //        attackResourceGainHighlightTimeElapsed : resourceGainHighlightTimeElapsed;
    //    bool[] highlightsIncreasing = attack ?
    //        attackResourceGainHighlightIncreasing : resourceGainHighlightIncreasing;
    //    Image[] slotBacks = attack ?
    //        attackResourceSlotBack : resourceSlotBack;
    //    int[] highlightIndices = attack ?
    //        attackResourceGainHighlightIndices : resourceGainHighlightIndices;

    //    for (int i = 0; i < highlightIndices.Length; i++)
    //    {
    //        if (highlightsActive[i])
    //        {
    //            highlightsTimeElapsed[i] += Time.deltaTime;
    //            if (highlightsIncreasing[i])
    //            {
    //                Vector3 scale = Vector3.Lerp(Vector3.one, resourceGainHighlightScale * Vector3.one,
    //                    EasingEquations.Easing.QuadEaseOut(
    //                        highlightsTimeElapsed[i] / resourceGainHighlightDuration));
    //                slotBacks[highlightIndices[i]].transform.localScale = scale;
    //                if (highlightsTimeElapsed[i] > resourceGainHighlightDuration)
    //                {
    //                    highlightsIncreasing[i] = false;
    //                    highlightsTimeElapsed[i] = 0;
    //                }
    //            }
    //            else
    //            {
    //                slotBacks[highlightIndices[i]].transform.localScale =
    //                    Vector3.Lerp(resourceGainHighlightScale * Vector3.one, Vector3.one,
    //                    EasingEquations.Easing.QuadEaseIn(
    //                        highlightsTimeElapsed[i] / resourceGainHighlightDuration));
    //                if (highlightsTimeElapsed[i] > resourceGainHighlightDuration)
    //                {
    //                    highlightsActive[i] = false;
    //                    highlightsIncreasing[i] = true;
    //                    highlightsTimeElapsed[i] = 0;
    //                }
    //            }
    //        }

    //        if(!highlightsActive[i] && slotBacks[highlightIndices[i]].transform.localScale.x > 1)
    //        {
    //            slotBacks[highlightIndices[i]].transform.localScale =
    //                    Vector3.Lerp(resourceGainHighlightScale * Vector3.one, Vector3.one,
    //                    EasingEquations.Easing.QuadEaseIn(
    //                        highlightsTimeElapsed[i] / resourceGainHighlightDuration));
    //        }
    //    }
    //}

    //private void HighlightResourcesMissing(bool attack = false)
    //{
    //    bool[] missingResourceAnimActive = attack ? attackResourceMissingAnimActive :
    //        resourceMissingAnimActive;
    //    int[] resourcesMissing = attack ? numAttackResourcesMissing : numResourcesMissing;
    //    Image[] indicators = attack ? attackResourceMissingIndicator : resourceMissingIndicator;
    //    Image[] slotFronts = attack ? attackResourceSlot : resourceSlot;
    //    float[] timesElapsed = attack ? attackResourceMissingTimeElapsed : resourceMissingTimeElapsed;
    //    bool[] increasing = attack ? attackResourceMissingAnimIncreasing : resourceMissingAnimIncreasing;

    //    Color indicatorColor = indicators[0].color;
    //    for (int i = 0; i < timesElapsed.Length; i++)
    //    {
    //        if (missingResourceAnimActive[i])
    //        {

    //            int resourcesMissingLeftToHighlight = resourcesMissing[i];
    //            if (resourcesMissingLeftToHighlight == 0)
    //            {
    //                for (int j = 0; j < slotFronts.Length; j++)
    //                {
    //                    indicators[j].color = new Color(indicatorColor.r,
    //                        indicatorColor.g, indicatorColor.b, 0);
    //                }
    //            }
    //            else
    //            {

    //                timesElapsed[i] += Time.deltaTime;
    //                for (int j = 0; j < slotFronts.Length; j++)
    //                {
    //                    if (slotFronts[j].fillAmount != 1)
    //                    {
    //                        resourcesMissingLeftToHighlight -= 1;
    //                        if (increasing[i])
    //                        {

    //                            indicators[j].color = Color.Lerp(
    //                                new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 0),
    //                                new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 1),
    //                                EasingEquations.Easing.QuadEaseOut(
    //                                    timesElapsed[i] / resourceMissingAnimDuration));
    //                            indicators[j].transform.localScale = Vector3.Lerp(
    //                                Vector3.one, resourceMissingAnimScale * Vector3.one,
    //                                EasingEquations.Easing.QuadEaseOut(
    //                                    timesElapsed[i] / resourceMissingAnimDuration));

    //                            if (timesElapsed[i] > resourceMissingAnimDuration)
    //                            {
    //                                increasing[i] = false;
    //                                timesElapsed[i] = 0;
    //                            }

    //                        }
    //                        else
    //                        {


    //                            indicators[j].color = Color.Lerp(
    //                            new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 1),
    //                            new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 0),
    //                            EasingEquations.Easing.QuadEaseIn(
    //                                timesElapsed[i] / resourceMissingAnimDuration));
    //                            indicators[j].transform.localScale = Vector3.Lerp(
    //                                resourceMissingAnimScale * Vector3.one, Vector3.one,
    //                                EasingEquations.Easing.QuadEaseIn(
    //                                    timesElapsed[i] / resourceMissingAnimDuration));


    //                            if (timesElapsed[i] > resourceMissingAnimDuration)
    //                            {
    //                                increasing[i] = true;
    //                                missingResourceAnimActive[i] = false;
    //                                timesElapsed[i] = 0;
    //                            }
    //                        }
    //                    }
    //                    if (resourcesMissingLeftToHighlight == 0) break;
    //                }
    //            }

    //        }
    //    }
    //    for (int j = 0; j < indicators.Length; j++)
    //    {
    //        if (slotFronts[j].fillAmount == 1)
    //        {
    //            indicators[j].color = new Color(indicatorColor.r,
    //                indicatorColor.g, indicatorColor.b, 0);
    //        }
    //    }
        
    //}


    public Vector3 GetBarPosition(bool destructor)
    {
        if (destructor)
        {
            return destructorDrawMeter.transform.TransformPoint(Vector3.zero);
        }
        else
        {
            return normalDrawMeter.transform.TransformPoint(Vector3.zero);
        }
    }

    //public void FailedPlayFromLackOfResources(int resourceDeficit, bool attack = false)
    //{
    //    if (attack)
    //    {
    //        numAttackResourcesMissing[0] = resourceDeficit;
    //        attackResourceMissingAnimActive[0] = true;
    //    }
    //    else
    //    {
    //        numResourcesMissing[0] = resourceDeficit;
    //        resourceMissingAnimActive[0] = true;
    //    }
    //}

    public void FailedPlayFromLackOfResources(bool attack)
    {
        ResourceIcon icon = attack ? attackIcons[0] : energyIcons[0];
        icon.StartMissingHighlight();
    }

    public void UpdateResourceMeter(float fillProportion, bool attack, 
        int resourceCount)
    {
        ResourceIcon[] icons = attack ? attackIcons : energyIcons;
        for (int i = 0; i < icons.Length; i++)
        {
            ResourceIcon icon = icons[i];
            if (i == resourceCount)
            {
                icon.SetFill(fillProportion);
            }
            else if (i < resourceCount) icon.SetFill(1);
            else icon.SetFill(0);
        }

    }

    //public void UpdateResourceMeter(float fillProportion, bool attack = false)
    //{
    //    Image[] slotFronts = attack ?
    //        attackResourceSlot : resourceSlot;
    //    Image[] slotBacks = attack ?
    //        attackResourceSlotBack : resourceSlotBack;
    //    float fillMin = attack ? attackUIFillMin : energyUIFillMin;
    //    float fillMax = attack ? attackUIFillMax : energyUIFillMax;

    //    for (int i = 0; i < slotFronts.Length; i++)
    //    {
    //        Image slotImage = slotFronts[i];
    //        Color slotColor = slotImage.color;
    //        if (slotImage.fillAmount < 1)
    //        {
    //            slotImage.color = new Color(slotColor.r, slotColor.g,
    //                slotColor.b, 0.5f);
    //            slotImage.fillAmount = fillMin + ((fillMax - fillMin) *
    //                EasingEquations.Easing.QuadEaseIn(fillProportion));
    //            slotBacks[i].color = new Color(slotColor.r, slotColor.g, slotColor.b, 1);
    //            break;
    //        }
    //    }
    //}

    public void UpdateResourceCount(int resourceCount, bool attack)
    {
        ResourceIcon[] icons = attack ? attackIcons : energyIcons;
        int lastCount = attack ? lastAttackResourceCount : lastResourceCount;
        for (int i = 0; i < icons.Length; i++)
        {
            ResourceIcon icon = icons[i];
            if (i == resourceCount - 1 && lastCount < resourceCount)
                icon.StartHighlight();
            if (i < resourceCount)
                icon.SetFill(1);
            else
                icon.SetFill(0);
        }
        if (attack) lastAttackResourceCount = resourceCount;
        else lastResourceCount = resourceCount;
    }

    //public void UpdateResourceCount(int resourceCount, int maxResources, bool attack = false)
    //{
    //    Image[] slotFronts = attack ? attackResourceSlot : resourceSlot;
    //    Image[] slotBacks = attack ? attackResourceSlotBack : resourceSlotBack;
    //    int lastCount = attack ? lastAttackResourceCount : lastResourceCount;
    //    int[] highlightIndices = attack ? attackResourceGainHighlightIndices : resourceGainHighlightIndices;
    //    bool[] highlightsActive = attack ? attackResourceGainHighlightActive : resourceGainHighlightActive;
    //    int[] resourcesMissing = attack ? numAttackResourcesMissing : numResourcesMissing;



    //    for (int i = 0; i < slotFronts.Length; i++)
    //    {
    //        Image slotImage = slotFronts[i];
    //        Image slotBack = slotBacks[i];
    //        Color slotColor = new Color(slotImage.color.r,
    //                slotImage.color.g, slotImage.color.b, 1);
    //        if (i < resourceCount)
    //        {
    //            slotImage.fillAmount = 1;
    //            slotImage.color = slotColor;
    //            slotBack.color = slotColor;
    //            slotBack.transform.localScale = Vector3.one;
    //            if (resourceCount > lastCount && i == resourceCount - 1)
    //            {
    //                highlightIndices[0] = i;
    //                highlightsActive[0] = true;
    //            }
    //        }
    //        else
    //        {
    //            slotImage.fillAmount = 0;
    //            slotBack.transform.localScale = Vector3.one;
    //            //slotBack.color = new Color(slotColor.r, slotColor.g, slotColor.b, 0);
    //        }
    //    }
    //    if (resourceCount != lastCount
    //        && resourcesMissing[0] != 0)
    //    {
    //        resourcesMissing[0] = Mathf.Max(0,
    //            resourcesMissing[0]
    //            - (resourceCount - lastCount));
    //    }
    //    int lastResourceCountArray = attack ? lastAttackResourceCount : lastResourceCount;
    //    lastResourceCountArray = resourceCount;
    //}

    public void UpdateDrawMeters(float normalFillProportion,
       float destructorFillProportion, float normalTimeLeft, float destructorTimeLeft)
    {
        float meterBody = radialMeterFillMax - radialMeterFillMin;

        int playerIndex = playerNum - 1;
        float adjustedNormalProportion = radialMeterFillMin +
            (normalFillProportion * meterBody);
        float adjustedDestructorProportion = radialMeterFillMin +
            (destructorFillProportion * meterBody);

        normalDrawMeter.fillAmount = adjustedNormalProportion;
        destructorDrawMeter.fillAmount = adjustedDestructorProportion;

        if (float.IsInfinity(normalTimeLeft)) normalTimeLeft = 0;

        normalPieceTimer.text = Mathf.CeilToInt(normalTimeLeft).ToString();
        destructorPieceTimer.text = Mathf.CeilToInt(destructorTimeLeft).ToString();
    }


    public void UpdateResourceLevel(int level)
    {
        UpdateLevelText(resourceLevelText, level);
    }

    public void UpdateNormLevel(int level)
    {
        UpdateLevelText(normLevelText, level);
    }

    public void UpdateDestLevel(int level)
    {
        UpdateLevelText(destLevelText, level);
    }

    public void UpdateLevelText(TextMeshProUGUI uiText, int level)
    {
        int prevLevel = 1;
        int.TryParse(uiText.text.Split(' ')[1], out prevLevel);
        if (level > prevLevel)
            uiText.GetComponent<ProdLevelText>().StartHighlight();
        uiText.text = "lvl " + level;
    }

    // Update is called once per frame
    void Update ()
    {
        //HighlightResourceGained();
        //HighlightResourcesMissing();
        //HighlightResourceGained(true);
        //HighlightResourcesMissing(true);
    }
}

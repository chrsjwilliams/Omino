using System.Collections;
using System.Collections.Generic;
using BeatManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour {
    
    [SerializeField]
    private GameObject[] interfaceToFlip;
    public Transform canvas;

    private List<int> touchIdsMakingTooltips;

    public bool tooltipsDisabled;


    private bool showCompletionMenu;
    public UIMeters[] UIMeters;
    #region UI Meters
    //public Text[] resourceCounters;
    //public GameObject[] resourceSlotZones;
    //private Image[][] resourceSlots;
    //private Image[][] resourceSlotBacks;
    //private Image[][] resourceMissingIndicators;
    //private Image[][] attackResourceMissingIndicators;
    //public GameObject[] attackResourceSlotZones;
    //private Image[][] attackResourceSlots;
    //private Image[][] attackResourceSlotBacks;
    //public RectTransform[] blueprintUIZones;
    //public GameObject[] techPowerUpIconArray;
    //public GameObject[][] meters;
    //[SerializeField]
    //private Image[] normalDrawMeters;
    //[SerializeField]
    //private TextMeshProUGUI[] normalPieceTimers;
    //[SerializeField]
    //private Image[] destructorDrawMeters;
    //[SerializeField]
    //private TextMeshProUGUI[] destructorPieceTimers;
    //[SerializeField]
    //private TextMeshProUGUI[] resourceLevelTexts;
    //[SerializeField]
    //private TextMeshProUGUI[] normLevelTexts;
    //[SerializeField]
    //private TextMeshProUGUI[] destLevelTexts;
    //[SerializeField]
    //private float radialMeterFillMin;
    //[SerializeField]
    //private float radialMeterFillMax;
    //[SerializeField]
    //private float resourceGainHighlightScale;

    //public Transform[] mineBlueprintLocations;
    //public Transform[] factoryBlueprintLocations;
    //public Transform[] bombFactoryBlueprintLocations;

    //[SerializeField]
    //private float resourceGainHighlightDuration;
    //private float[] resourceGainHighlightTimeElapsed;
    //private int[] resourceGainHighlightIndices;
    //private bool[] resourceGainHighlightActive;
    //private bool[] resourceGainHighlightIncreasing;
    //private float[] attackResourceGainHighlightTimeElapsed;
    //private int[] attackResourceGainHighlightIndices;
    //private bool[] attackResourceGainHighlightActive;
    //private bool[] attackResourceGainHighlightIncreasing;
    //private int[] lastResourceCount;
    //private int[] lastAttackResourceCount;
    //[SerializeField]
    //private float resourceMissingAnimDuration;
    //[SerializeField]
    //private float resourceMissingAnimScale;
    //private float[] resourceMissingTimeElapsed;
    //private int[] numResourcesMissing;
    //private bool[] resourceMissingAnimActive;
    //private bool[] resourceMissingAnimIncreasing;
    //private float[] attackResourceMissingTimeElapsed;
    //private int[] numAttackResourcesMissing;
    //private bool[] attackResourceMissingAnimActive;
    //private bool[] attackResourceMissingAnimIncreasing;
    //private const float energyUIFillMin = 0.13f;
    //private const float energyUIFillMax = 0.9f;
    //private const float attackUIFillMin = 0.08f;
    //private const float attackUIFillMax = 0.94f;
    #endregion

    [SerializeField]
    private float radialMeterFillMin;
    [SerializeField]
    private float radialMeterFillMax;
    
    private const float energyUIFillMin = 0.13f;
    private const float energyUIFillMax = 0.9f;
    private const float attackUIFillMin = 0.08f;
    private const float attackUIFillMax = 0.94f;

    public UIBannerManager UIBannerManager;
    public UIMenus UIMenu;
    public PauseButton pauseButton;

    [SerializeField]
    private Button replayButton;
    [SerializeField]
    private Button levelSelectButton;

    #region TileSkin Items

    [SerializeField]
    private Sprite[] _factoryBottoms;
    public Sprite[] factoryBottoms
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].factoryBottoms;
            }
            else
                return _factoryBottoms;
        }
    }

    [SerializeField]
    private Sprite[] _factoryTops;
    public Sprite[] factoryTops
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].factoryTops;
            }
            else
                return _factoryTops;
        }
    }

    [SerializeField]
    private Sprite[] _factoryIcons;
    public Sprite[] factoryIcons
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].factoryIcons;
            }
            else
                return _factoryIcons;
        }
    }

    [SerializeField]
    private Sprite[] _mineBottoms;
    public Sprite[] mineBottoms
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].mineBottoms;
            }
            else
                return _mineBottoms;
        }
    }

    [SerializeField]
    private Sprite[] _mineTops;
    public Sprite[] mineTops
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].mineTops;
            }
            else
                return _mineTops;
        }
    }

    [SerializeField]
    private Sprite[] _mineIcons;
    public Sprite[] mineIcons
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].mineIcons;
            }
            else
                return _mineIcons;
        }
    }

    [SerializeField]
    private Sprite[] _homeBaseSprites;
    public Sprite[] homeBaseSprites
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].homeBaseSprites;
            }
            else
                return _homeBaseSprites;
        }
    }
    [SerializeField]
    private Sprite _baseOverlay;
    public Sprite baseOverlay
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].baseOverlay;
            }
            else
                return _baseOverlay;
        }
    }

    [SerializeField]
    private Sprite _sideBaseOverlay;
    public Sprite sideBaseOverlay
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].sideBaseOverlay;
            }
            else
                return _sideBaseOverlay;
        }
    }

    [SerializeField]
    private Sprite[] _bombFactoryBottoms;
    public Sprite[] bombFactoryBottoms
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].bombFactoryBottoms;
            }
            else
                return _bombFactoryBottoms;
        }
    }

    [SerializeField]
    private Sprite[] _bombFactoryTops;
    public Sprite[] bombFactoryTops
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].bombFactoryTops;
            }
            else
                return _bombFactoryTops;
        }
    }

    [SerializeField]
    private Sprite[] _bombFactoryIcons;
    public Sprite[] bombFactoryIcons
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].bombFactoryIcons;
            }
            else
                return _bombFactoryIcons;
        }
    }

    [SerializeField]
    private Sprite _structureOverlay;
    public Sprite structureOverlay
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].structureOverlay;
            }
            else
                return _structureOverlay;
        }
    }
    #endregion

    public TextMeshProUGUI[] aiStatuses;

    private void Awake()
    {
    }

    // Use this for initialization
    void Start() {
        showCompletionMenu = true;
        
        touchIdsMakingTooltips = new List<int>();

        Color textColor = Color.black;
        if( Services.GameManager.mode == TitleSceneScript.GameMode.HyperSOLO ||
            Services.GameManager.mode == TitleSceneScript.GameMode.HyperVS)
        {
            textColor = Color.white;
        }

        if(Services.GameManager.mode != TitleSceneScript.GameMode.Edit)
            UIMenu.tutorialLevelCompleteMenu.gameObject.SetActive(false);

        if (Services.GameManager.disableUI)
        {
            //for (int i = 0; i < 2; i++)
            //{
            //    normalDrawMeters[i].enabled = false;
            //    normalPieceTimers[i].enabled = false;
            //    destructorDrawMeters[i].enabled = false;
            //    destructorPieceTimers[i].enabled = false;
            //    destLevelTexts[i].enabled = false;
            //    normLevelTexts[i].enabled = false;
            //    resourceLevelTexts[i].enabled = false;
            //    resourceSlotZones[i].gameObject.SetActive(false);
            //}
            pauseButton.gameObject.SetActive(false);
        }

        if (Services.GameManager.mode == TitleSceneScript.GameMode.Challenge || 
            Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            
            replayButton.gameObject.GetComponentsInChildren<Image>(true)[1].gameObject.SetActive(false);
            replayButton.GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(true);
            replayButton.GetComponentInChildren<TextMeshProUGUI>(true).text = "forfeit";
            levelSelectButton.gameObject.SetActive(false);

        }
        else if(Services.GameManager.mode != TitleSceneScript.GameMode.Edit)
        {
            levelSelectButton.gameObject.SetActive(true);
            replayButton.gameObject.GetComponentsInChildren<Image>(true)[1].gameObject.SetActive(true);
            replayButton.GetComponentInChildren<TextMeshProUGUI>(true).gameObject.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.P)) Debug.Break();
        //if (Input.GetKeyDown(KeyCode.M)) ShowCampaignLevelCompleteMenu(Services.GameManager.Players[0]);
	}

    public void UIForSinglePlayer(bool singlePlayer)
    {
        float bannerRotation = singlePlayer ? 0 : 180;
        float uiElementRotation = singlePlayer ? 180 : 0;

        foreach (GameObject uiElemt in interfaceToFlip)
        {

            if (uiElemt.name.Contains("Banner"))
            {
                //uiElemt.transform.localRotation = Quaternion.Euler(0, 0, bannerRotation);
            }
            else
            {
                uiElemt.transform.localRotation = Quaternion.Euler(0, 0, uiElementRotation);
            }       
        }
    }

    public void UpdateResourceLevel(int level, int playerNum)
    {
       UIMeters[playerNum - 1].UpdateResourceLevel(level);
    }

    public void UpdateNormLevel(int level, int playerNum)
    {
        UIMeters[playerNum - 1].UpdateNormLevel(level);
    }

    public void UpdateDestLevel(int level, int playerNum)
    {
        UIMeters[playerNum - 1].UpdateDestLevel(level);
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

    public void ToggleNeon()
    {
        Services.GameManager.ToggleNeon();
        _NeonButtonAppearanceToggle();
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

    private void _NeonButtonAppearanceToggle()
    {
        GameObject button = GameObject.Find("ToggleNeon");

        if (Services.GameManager.NeonEnabled)
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[0];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "neon";

        }
        else
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>neon</s>";
        }
    }

    private void _SFXButtonAppearanceToggle()
    {
        GameObject button = GameObject.Find("ToggleSound");

        if (Services.GameManager.SoundEffectsEnabled)
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[0];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "sound fx";

        }
        else
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>sound fx</s>";
        }
    }

    private void _MusicButtonAppearanceToggle()
    {
        GameObject button = GameObject.Find("ToggleMusic");

        if (Services.GameManager.MusicEnabled)
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[0];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "music";

        }
        else
        {
            button.GetComponent<Image>().color = Services.GameManager.Player2ColorScheme[1];
            button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "<s>music</s>";
        }
    }

    private void HighlightProdLevelIncrease(TextMeshProUGUI uiText)
    {

    }

    public void ShowCampaignLevelCompleteMenu(Player winner)
    {
        UIMenu.tutorialLevelCompleteMenu.Show(winner);
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class ReadyBanner : MonoBehaviour
{
    [SerializeField]
    private int playerNum;
    public Button button { get; private set; }
    private Player player;
    [SerializeField]
    private Color readyColor;
    private Image image;
    private TextMeshProUGUI uiText;
    private bool initialized;
    private ObjectPulser pulser;
    private float pulsePeriod = 0.4f;
    private readonly Vector3 minScale = 0.9f * Vector3.one;
    private readonly Vector3 maxScale = 1.1f * Vector3.one;

    // Use this for initialization
    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        uiText = GetComponentInChildren<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init()
    {
        pulsePeriod = Services.Clock.QuarterLength();
        
        player = Services.GameManager.Players[playerNum - 1];
        pulser = uiText.gameObject.AddComponent<ObjectPulser>();
        if (player is AIPlayer)
        {
            button.enabled = false;
            image.color = readyColor;
            ToggleReady();
        }
        else
        {
            image.color = player.ColorScheme[0];
            pulser.StartPulse(pulsePeriod, minScale, maxScale);
        }
    }

    public void ToggleReady()
    {
        if(!(player is AIPlayer)) Handheld.Vibrate();
        
        player.ToggleReady();
        if (player.ready)
        {
            uiText.text = "READY";
            image.color = readyColor;
            Services.AudioManager.PlaySoundEffect(Services.Clips.UIReadyOn, 1.0f);
            pulser.StopPulse();
        }
        else
        {
            uiText.text = "READY?";
            image.color = player.ColorScheme[0]; //notReadyColors[playerNum-1];
            Services.AudioManager.PlaySoundEffect(Services.Clips.UIReadyOff, 1.0f);
            pulser.StartPulse(pulsePeriod,minScale,maxScale);
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
            for (int i = 0; i < Services.UIManager.UIBannerManager.readyBanners.Length; i++)
            {
                Services.UIManager.UIBannerManager.readyBanners[i].button.enabled = false;
            }
            Services.GameScene.StartGameSequence();
        }
    }
}

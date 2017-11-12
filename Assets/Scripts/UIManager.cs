using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public RectTransform[] handZones;
    [SerializeField]
    private Text[] resourceCounters;
    [SerializeField]
    private Image[] drawMeters;
    [SerializeField]
    private Image[] playMeters;
    [SerializeField]
    private Image[] greyOutBoxes;
    [SerializeField]
    private RectTransform[] victoryBanners;
    [SerializeField]
    private RectTransform[] defeatBanners;
    public Transform canvas;
    public Sprite destructorIcon;
    public Sprite superDestructorIcon;
    public Sprite factoryIcon;
    public Sprite mineIcon;
    public Sprite baseIcon;
    private bool scrollingInBanners;
    private Player winner;
    private float bannerScrollTimeElapsed;
    [SerializeField]
    private float bannerScrollDuration;

	// Use this for initialization
	void Start () {
        for (int i = 0; i < victoryBanners.Length; i++)
        {
            victoryBanners[i].gameObject.SetActive(false);
            defeatBanners[i].gameObject.SetActive(false);
        }
        foreach(Image box in greyOutBoxes)
        {
            box.enabled = false;
        }
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

    public void UpdateDrawMeter(int playerNum, float fillProportion)
    {
        drawMeters[playerNum - 1].fillAmount = fillProportion;
    }

    public void UpdatePlayMeter(int playerNum, float fillProportion, bool playAvailable)
    {
        playMeters[playerNum - 1].fillAmount = fillProportion;
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

    public void UpdateResourceCount(int resourceCount, int maxResources, Player player)
    {
        resourceCounters[player.playerNum - 1].text = resourceCount + "/" + maxResources;
    }
}

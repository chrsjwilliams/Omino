using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBannerManager : MonoBehaviour {

    [SerializeField]
    private RectTransform[] victoryBanners;
    [SerializeField]
    private RectTransform[] defeatBanners;
    private Vector3[] gameEndBannerStartPositions;
    private Vector3[] gameEndBannerTargetPositions;
    
    public ReadyBanner[] readyBanners;
    private bool scrollingInBanners;
    private Player winner;
    private float bannerScrollTimeElapsed;
    [SerializeField]
    private Color readyColor;
    [SerializeField]
    private float bannerScrollDuration;

    // Use this for initialization
    void Start ()
    {
        for (int i = 0; i < victoryBanners.Length; i++)
        {
            victoryBanners[i].gameObject.SetActive(false);
            defeatBanners[i].gameObject.SetActive(false);
        }
    }

    private void HighlightReadyBanners()
    {
        float pingPong = Mathf.PingPong(Time.time, Services.Clock.BeatLength());


        for (int i = 0; i < readyBanners.Length; i++)
        {
            if (!Services.GameManager.Players[i].ready)
            {
                readyBanners[i].GetComponent<Image>().color =
                    Color.Lerp(Services.GameManager.colorSchemes[i][0],
                                Services.GameManager.colorSchemes[i][1],
                                pingPong);
            }
        }
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


    public void OnGameEndBannerTouch()
    {
        Services.UIManager.pauseButton.ToggleCompletionMenu(Services.GameManager.mode);
    }

    // Update is called once per frame
    void Update ()
    {
        if (scrollingInBanners) ScrollBanners();
        //if (!Services.GameScene.gameStarted) HighlightReadyBanners();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndBanner : MonoBehaviour
{
    private bool scrollingInBanner;
    [SerializeField]
    private RectTransform banner;



    public void OnGameEndBannerTouch()
    {
        Services.UIManager.pauseButton.ToggleCompletionMenu(Services.GameManager.mode);
    }


    // Update is called once per frame
    void Update () {
		
	}
}

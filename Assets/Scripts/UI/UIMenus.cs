using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenus : MonoBehaviour
{
    public GameObject pauseMenu;
    public CampaignMenuManager tutorialLevelCompleteMenu;
    public DungeonRunInGameUIManager dungeonRunChallenegeCompleteMenu;
    public GameObject optionsMenu;
    public EloInGameUiManager eloUIManager;

    // Use this for initialization
    void Start ()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

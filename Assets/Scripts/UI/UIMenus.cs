
using UnityEngine;

public class UIMenus : MonoBehaviour
{
    public PauseMenu pauseMenu;
    public CampaignMenuManager tutorialLevelCompleteMenu;
    public DungeonRunInGameUIManager dungeonRunChallenegeCompleteMenu;
    public GameObject optionsMenu;
    public EloInGameUiManager eloUIManager;

    // Use this for initialization
    void Start ()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetPauseMenuStatus(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

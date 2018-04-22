using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignMenuManager : MonoBehaviour {

    [SerializeField]
    private Button nextLevelButton;
    [SerializeField]
    private Image nextLevelDisabled;
    [SerializeField]
    private Button[] buttons;
    [SerializeField]
    private Image victorySymbol;
    [SerializeField]
    private Image[] wreaths;
    [SerializeField]
    private Image defeatSymbol;
    [SerializeField]
    private GameObject wreathHolder;
    [SerializeField]
    private Color highlightedButtonColor;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Show(Player winner)
    {
        float rot;
        Level nextLevel = Services.MapManager.GetNextLevel();
        Image resultImage;
        if (winner is AIPlayer)
        {
            rot = winner.playerNum == 2 ? -90 : 90;
            wreathHolder.SetActive(false);
            defeatSymbol.gameObject.SetActive(true);
            resultImage = defeatSymbol;
            buttons[1].GetComponent<Image>().color = highlightedButtonColor;
        }
        else
        {
            rot = winner.playerNum == 1 ? -90 : 90;
            wreathHolder.SetActive(true);
            defeatSymbol.gameObject.SetActive(false);
            resultImage = victorySymbol;
            for (int i = 0; i < wreaths.Length; i++)
            {
                wreaths[i].transform.localScale = Vector3.zero;
            }
        }
        transform.localRotation = Quaternion.Euler(0, 0, rot);
        gameObject.SetActive(true);
        if (nextLevel == null || winner is AIPlayer)
        {
            nextLevelButton.enabled = false;
            nextLevelDisabled.enabled = true;
        }
        else
        {
            nextLevelDisabled.enabled = false;
            buttons[0].GetComponent<Image>().color = highlightedButtonColor;
        }
        transform.localScale = Vector3.zero;
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }
        resultImage.transform.localScale = Vector3.zero;

        Services.GameScene.tm.Do(new CampaignLevelMenuEntranceTask(transform,
            resultImage, wreaths, buttons));
    }
}

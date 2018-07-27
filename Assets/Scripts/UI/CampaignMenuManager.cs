using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampaignMenuManager : MonoBehaviour {

    public bool inPosition { get; private set; }
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
    [SerializeField]
    private Sprite success;
    [SerializeField]
    private Sprite fail;
    [SerializeField]
    private TextMeshProUGUI[] objectiveText;
    [SerializeField]
    private Image[] objectiveCompletionSymbol;
    [SerializeField]
    private Sprite lockImage;
    [SerializeField]
    private Color lockColor;
    [SerializeField]
    private Image[] progressNodes;
    [SerializeField]
    private float progressBarFill;

    // Use this for initialization
    void Start () {
        inPosition = false;
	}
	
	// Update is called once per frame
	void Update () {
        //progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, progressBarFill, Time.deltaTime);
        //progressNodes[Services.GameManager.levelSelected.campaignLevelNum - 1].fillAmount = baseNodeProgressBarFill;

	}

    public void MenuInPosition()
    {
        inPosition = true;
    }

    private void SetFilledProgressNodes(int tutorialLevel)
    {
        if (tutorialLevel == 1) return;
        for(int i = 1; i < tutorialLevel; i++)
        {
            progressNodes[i - 1].fillAmount = 1;
        }
    }

    public void Show(Player winner)
    {
        SetFilledProgressNodes(Services.GameManager.levelSelected.campaignLevelNum);
        switch (Services.GameManager.levelSelected.campaignLevelNum)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = "COMPLETE";
                if(Services.TutorialManager.CompletionCheck())
                    buttons[0].GetComponent<Image>().color = highlightedButtonColor;
                break;
            default:
                break;
        }
        for (int i = 0; i < objectiveText.Length; i++)
        {
            objectiveText[i].text = Services.TutorialManager.objectiveText[i];

            if (Services.TutorialManager.objectiveComplete[i])
            {
                objectiveCompletionSymbol[i].sprite = success;
                objectiveCompletionSymbol[i].color = highlightedButtonColor;
                progressBarFill = (progressBarFill + 0.5f);
            }
            else
            {
                objectiveCompletionSymbol[i].sprite = fail;
            }
        }

        float rot;
        Level nextLevel = Services.MapManager.GetNextLevel();
        Image resultImage;
        if (winner is AIPlayer)
        {
            rot = winner.playerNum == 2 ? 0 : 180;
            wreathHolder.SetActive(false);
            defeatSymbol.gameObject.SetActive(true);
            resultImage = defeatSymbol;
            buttons[1].GetComponent<Image>().color = highlightedButtonColor;
        }
        else
        {
            rot = winner.playerNum == 1 ? 0 : 180;
            wreathHolder.SetActive(true);
            defeatSymbol.gameObject.SetActive(false);
            resultImage = victorySymbol;
            for (int i = 0; i < wreaths.Length; i++)
            {
                wreaths[i].transform.localScale = Vector3.zero;
            }
        }
        

        if(!Services.TutorialManager.CompletionCheck())
        {
            buttons[1].GetComponent<Image>().color = highlightedButtonColor;
        }

        transform.localRotation = Quaternion.Euler(0, 0, rot);
        gameObject.SetActive(true);
        if ((nextLevel == null || winner is AIPlayer) || 
            !Services.TutorialManager.CompletionCheck())
        {

            if(!Services.TutorialManager.CompletionCheck())
            {
                nextLevelButton.enabled = false;
                nextLevelDisabled.sprite = lockImage;
                nextLevelDisabled.color = lockColor;
                nextLevelDisabled.enabled = true;
            }
            else
            {
                nextLevelDisabled.enabled = false;
                
                nextLevelButton.enabled = true;
                nextLevelButton.onClick.RemoveListener(Services.GameScene.MoveToNextLevel);
                nextLevelButton.onClick.AddListener(Services.GameScene.Reset);
            }           
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

        TaskTree moveCampaignMenuIntoPosition = new TaskTree(new EmptyTask(),
                new TaskTree(new CampaignLevelMenuEntranceTask(transform,
            resultImage, wreaths, buttons)),
                new TaskTree(new LERPProgressBar(progressNodes[Services.GameManager.levelSelected.campaignLevelNum - 1], progressBarFill, 1.0f)));

        //moveCampaignMenuIntoPosition.Then(new LERPProgressBar(progressNodes[Services.GameManager.levelSelected.campaignLevelNum - 1], progressBarFill, 1.0f));
        moveCampaignMenuIntoPosition.Then(new ActionTask(MenuInPosition));
        


        Services.GameScene.tm.Do(moveCampaignMenuIntoPosition);
    }
}

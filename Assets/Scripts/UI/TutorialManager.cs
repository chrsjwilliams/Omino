using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

    private TutorialTooltip currentTooltip;
    public GameObject tutorialTooltipPrefab;
    private int currentIndex;
    public Vector2[] tooltipLocations;
    [TextArea]
    public string[] tooltipTexts;
    public bool[] tooltipsDismissable;
    public Vector2[] arrowLocations;
    public float[] arrowRotations;
    public GameObject backDim;
    private TaskManager tm;
    public float delayDur;

    private void Awake()
    {
        tm = new TaskManager();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        tm.Update();
	}

    public void Init()
    {
        currentIndex = 0;
        CreateTooltip();
    }


    public void OnDismiss()
    {
        if (tooltipsDismissable[currentIndex])
        {
            Services.GameScene.UnpauseGame();
            backDim.SetActive(false);
        }
        if (currentIndex < tooltipTexts.Length - 1) MoveToNextStep();
    }

    private void MoveToNextStep()
    {
        currentIndex += 1;
        //Wait wait = new Wait(delayDur);
        //wait.Then(new ActionTask(CreateTooltip));
        //tm.Do(wait);
        CreateTooltip();
        if (!tooltipsDismissable[currentIndex])
            Services.GameEventManager.Register<PiecePlaced>(OnPiecePlaced);
    }

    private void OnPiecePlaced(PiecePlaced e)
    {
        if (e.piece.owner.playerNum == 1)
        {
            Services.GameEventManager.Unregister<PiecePlaced>(OnPiecePlaced);
            currentTooltip.Dismiss();
        }
    }

    private void CreateTooltip()
    {
        currentTooltip = Instantiate(Services.Prefabs.TutorialTooltip,
            Services.UIManager.canvas).GetComponent<TutorialTooltip>();
        currentTooltip.Init(
            tooltipLocations[currentIndex],
            tooltipTexts[currentIndex],
            tooltipsDismissable[currentIndex],
            arrowLocations[currentIndex],
            arrowRotations[currentIndex]);
        if (tooltipsDismissable[currentIndex])
        {
            Services.GameScene.PauseGame();
            backDim.SetActive(true);
        }
    }
}

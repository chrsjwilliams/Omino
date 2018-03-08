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
        backDim.SetActive(false);
        if (currentIndex < tooltipTexts.Length - 1) MoveToNextStep();
        Services.GameScene.UnpauseGame();
    }

    private void MoveToNextStep()
    {
        currentIndex += 1;
        Wait wait = new Wait(delayDur);
        wait.Then(new ActionTask(CreateTooltip));
        tm.Do(wait);
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
        backDim.SetActive(true);
        Services.GameScene.PauseGame();
    }
}

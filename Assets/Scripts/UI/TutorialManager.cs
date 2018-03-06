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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init()
    {
        currentIndex = 0;
        CreateTooltip();
    }


    public void OnDismiss()
    {
        if (currentIndex < tooltipTexts.Length - 1) MoveToNextStep();
    }

    private void MoveToNextStep()
    {
        currentIndex += 1;
        CreateTooltip();
    }

    private void CreateTooltip()
    {
        currentTooltip = Instantiate(Services.Prefabs.TutorialTooltip,
            Services.UIManager.canvas).GetComponent<TutorialTooltip>();
        currentTooltip.Init(
            tooltipLocations[currentIndex],
            tooltipTexts[currentIndex],
            tooltipsDismissable[currentIndex]);
    }
}

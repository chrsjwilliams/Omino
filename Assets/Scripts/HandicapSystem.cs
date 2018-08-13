using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandicapSystem : MonoBehaviour
{

    private bool singlePlayerMode;

    public Slider p1HandicapSlider;
    public Slider p2HandicapSlider;

    public static PlayerHandicap[] tutorialAIHandicapLevels;
    public static PlayerHandicap[] tutorialPlayerHandicapLevels;
    public static PlayerHandicap[] handicapValues = new PlayerHandicap[]
        {
            new PlayerHandicap(1, 1, 1) ,
            new PlayerHandicap(1, 1, 1)
        };

    public static PlayerHandicap[] hyperModeValues = new PlayerHandicap[]
    {
        new PlayerHandicap(2, 2, 2),
        new PlayerHandicap(2, 2, 2)
    };
    public float p1HandicapValue;
    public float p2HandicapValue;

    [SerializeField]
    private TextMeshProUGUI p1Text;
    [SerializeField]
    private TextMeshProUGUI p2Text;

    [SerializeField]
    private TextMeshProUGUI p1PercentText;
    [SerializeField]
    private TextMeshProUGUI p2PercentText;

	// Use this for initialization
	public static void Init ()
    {
        handicapValues = new PlayerHandicap[]
        {
            new PlayerHandicap(1, 1, 1) ,
            new PlayerHandicap(1, 1, 1)
        };

        hyperModeValues = new PlayerHandicap[]
        {
        new PlayerHandicap(2, 2, 2),
        new PlayerHandicap(2, 2, 2)
        };

        tutorialAIHandicapLevels = new PlayerHandicap[]
        {
            new PlayerHandicap(float.MinValue, float.MinValue, float.MinValue),
            new PlayerHandicap(0.7f, 0.7f, 0.7f),
            new PlayerHandicap(0.7f, 0.75f, 0.75f),
            new PlayerHandicap(0.7f, 0.75f, 0.75f),
            new PlayerHandicap(0.8f, 0.8f, 0.8f)
        };

        tutorialPlayerHandicapLevels = new PlayerHandicap[]
        {
            new PlayerHandicap(1.5f, 1.5f, 1f),
            new PlayerHandicap(1.25f, 1.25f, 1.5f),
            new PlayerHandicap(1f, 1f, 1.25f),
            new PlayerHandicap(1f, 1f, 1f),
            new PlayerHandicap(1f, 1f, 1f)
        };
    }

    public void UpdateHandicapText()
    {
        singlePlayerMode = Services.GameManager.mode == TitleSceneScript.GameMode.TwoPlayers ? false : true;
        if (singlePlayerMode)
        {
            p1Text.text = "Player";
            p2Text.text = "AI";
        }
        else
        {
            p1Text.text = "Player 1";
            p2Text.text = "Player 2";
        }
    }

    public void UpdatePlayer1HandicapValueSlider()
    {

        handicapValues[0].SetEnergyHandicapLevel(1 + (p1HandicapSlider.value * 0.1f));
        handicapValues[0].SetHammerHandicapLevel(1 + (p1HandicapSlider.value * 0.1f));
        handicapValues[0].SetPieceHandicapLevel(1 + (p1HandicapSlider.value * 0.1f));

        p1PercentText.text = (handicapValues[0].enegryProduction * 100) + "%";
    }

    public void UpdatePlayer2HandicapValueSlider()
    {
        handicapValues[1].SetEnergyHandicapLevel(1 + (p2HandicapSlider.value * 0.1f));
        handicapValues[1].SetHammerHandicapLevel(1 + (p2HandicapSlider.value * 0.1f));
        handicapValues[1].SetPieceHandicapLevel(1 + (p2HandicapSlider.value * 0.1f));
        p2PercentText.text = (handicapValues[1].enegryProduction * 100) + "%";
    }

    // Update is called once per frame
    void Update ()
    {
        UpdatePlayer1HandicapValueSlider();
        UpdatePlayer2HandicapValueSlider();
    }
}

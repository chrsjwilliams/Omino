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

    public static float[] tutorialHandicapLevels = new float[]
    {
        float.MinValue,
        0.7f,
        0.75f,
        0.8f
    };

    public static float[] handicapValues = new float[]{ 1, 1 };
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
        handicapValues = new float[] { 1, 1 };
        tutorialHandicapLevels = new float[] { float.MinValue, 0.7f, 0.75f, 0.8f };
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
        handicapValues[0] = (1 + (p1HandicapSlider.value * 0.1f));
        p1PercentText.text = (handicapValues[0] * 100) + "%";
    }

    public void UpdatePlayer2HandicapValueSlider()
    {
        handicapValues[1] = (1 + (p2HandicapSlider.value * 0.1f));
        p2PercentText.text = (handicapValues[1] * 100) + "%";
    }

    // Update is called once per frame
    void Update ()
    {
        UpdatePlayer1HandicapValueSlider();
        UpdatePlayer2HandicapValueSlider();
    }
}

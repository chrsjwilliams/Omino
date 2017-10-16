﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public RectTransform[] handZones;
    [SerializeField]
    private Image[] drawMeters;
    [SerializeField]
    private Image[] playMeters;
    [SerializeField]
    private Image[] playAvailableIcons;
    [SerializeField]
    private Text[] playAvailableTexts;
    [SerializeField]
    private Color playAvailableColor;
    [SerializeField]
    private Color playUnavailableColor;
    [SerializeField]
    private string playAvailableText;
    [SerializeField]
    private string playUnavailableText;
    [SerializeField]
    private Text gameWinText;
    public Transform canvas;
	[SerializeField]
	private Text touchCount;

	// Use this for initialization
	void Start () {
        gameWinText = GameObject.Find("GameWinText").GetComponent<Text>();
        gameWinText.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateTouchCount(Touch[] touches){
		string newText = "";
		for (int i = 0; i < touches.Length; i++) {
			newText += "touch "+ touches[i].fingerId + " at :" + touches [i].position.x + ", " + touches [i].position.y + "\n";
		}
		touchCount.text = newText;
	}

    public void UpdateDrawMeter(int playerNum, float fillProportion)
    {
        drawMeters[playerNum - 1].fillAmount = fillProportion;
    }

    public void UpdatePlayMeter(int playerNum, float fillProportion, bool playAvailable)
    {
        playMeters[playerNum - 1].fillAmount = fillProportion;
        if (playAvailable)
        {
            playAvailableIcons[playerNum - 1].color = playAvailableColor;
            playAvailableTexts[playerNum - 1].text = playAvailableText;
        }
        else
        {
            playAvailableIcons[playerNum - 1].color = playUnavailableColor;
            playAvailableTexts[playerNum - 1].text = playUnavailableText;
        }
    }

    public void SetGameWinText(Player winner)
    {
        gameWinText.gameObject.SetActive(true);
        gameWinText.text = "PLAYER " + winner.playerNum + " WINS";
        gameWinText.color = winner.ActiveTilePrimaryColors[0];
    }
}

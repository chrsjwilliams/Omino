using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EloInGameUiManager : MonoBehaviour {

    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private GameObject defeatSymbol;
    [SerializeField]
    private GameObject victorySymbol;
    [SerializeField]
    private TextMeshProUGUI eloChangeText;

    private const float offset = 1800;
    private float timeElapsed;
    private const float dropDownDuration = 0.5f;
    private bool dropping;
    private Vector3 basePos;

	// Use this for initialization
	void Start () {
        menu.SetActive(false);
        basePos = menu.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
        if (dropping) Drop();
	}

    private void Drop()
    {
        timeElapsed += Time.deltaTime;
        menu.transform.localPosition = Vector3.Lerp(
            (Vector3.up * offset) + basePos,
            basePos, EasingEquations.Easing.QuadEaseOut(timeElapsed/dropDownDuration));
        if(timeElapsed >= dropDownDuration)
        {
            dropping = false;
            menu.transform.localPosition = basePos;
        }
    }

    public void OnGameEnd(bool victory, int prevElo, int newElo)
    {
        dropping = true;
        menu.transform.localPosition = basePos + (offset * Vector3.up);
        menu.SetActive(true);
        defeatSymbol.SetActive(!victory);
        victorySymbol.SetActive(victory);

        int diff = newElo - prevElo;
        string diffDirection = diff > 0 ? "+" : "";
        string colorTag = diff > 0 ? "green" : "red";

        eloChangeText.text = "    " + prevElo + " -> " + newElo +
            "<color=" + colorTag + "> (" + diff + ")</color>";
    }
}

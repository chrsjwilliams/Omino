using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EloInGameUiManager : MonoBehaviour {

    public GameObject menu;
    [SerializeField]
    private GameObject defeatSymbol;
    [SerializeField]
    private GameObject victorySymbol;
    [SerializeField]
    private Image rankImage;
    [SerializeField]
    private Image rankProgressBar;

    private const float offset = 1800;
    private float timeElapsed;
    private const float dropDownDuration = 0.5f;
    private bool dropping;
    private Vector3 basePos;

    private float progressBarFillDur = 1f;
    private bool progressFilling;
    private float progressTarget;
    private float progressStart;
    private float progressTimeElapsed;
    private Sprite rankImageTarget;
    private bool win;

	// Use this for initialization
	void Start () {
        menu.SetActive(false);
        basePos = menu.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
        if (dropping) Drop();
        else if (progressFilling) SetProgress();
	}

    private void Drop()
    {
        timeElapsed += Time.deltaTime;
        menu.transform.localPosition = Vector3.Lerp(
               (Vector3.up * offset) + basePos,
               basePos, EasingEquations.Easing.QuadEaseOut(timeElapsed / dropDownDuration));

        if (timeElapsed >= dropDownDuration)
        {
            dropping = false;
            menu.transform.localPosition = basePos;
            progressFilling = true;
        }
    }

    private void SetProgress()
    {
        progressTimeElapsed += Time.deltaTime;
        if (win && progressTarget > progressStart || 
            !win && progressStart > progressTarget)
        {
            rankProgressBar.fillAmount = Mathf.Lerp(
                progressStart, 
                progressTarget,
                EasingEquations.Easing.QuadEaseOut(
                    progressTimeElapsed / progressBarFillDur));
        }
        else
        {
            float interimTarget;
            if (progressTarget < progressStart) interimTarget = 1;
            else interimTarget = 0;
            float oppositeInterim = 1 - interimTarget;
            float interimTime = Mathf.Abs(progressStart - interimTarget) 
                / progressBarFillDur;
            if(timeElapsed <= interimTime)
            {
                rankProgressBar.fillAmount = Mathf.Lerp(
                    progressStart, interimTarget,
                    EasingEquations.Easing.QuadEaseOut(
                        progressTimeElapsed / interimTime));
            }
            else
            {
                rankImage.sprite = rankImageTarget;
                rankProgressBar.fillAmount = Mathf.Lerp(
                    oppositeInterim, progressTarget,
                    EasingEquations.Easing.QuadEaseOut(
                        (progressTimeElapsed-interimTime) 
                    / (progressBarFillDur - interimTime)));
            }

        }
       if(progressTimeElapsed >= progressBarFillDur)
        {
            progressFilling = false;
            rankImage.sprite = rankImageTarget;
        }
    }

    public void OnGameEnd(bool victory, EloData prevElo, EloData newElo)
    {
        dropping = true;
        menu.transform.localPosition = basePos + (offset * Vector3.up);
        menu.SetActive(true);
        defeatSymbol.SetActive(!victory);
        victorySymbol.SetActive(victory);
        win = victory;
        rankImage.sprite = prevElo.GetRankImage();
        rankProgressBar.fillAmount = prevElo.GetProgressToNextRank();
        rankImageTarget = newElo.GetRankImage();
        progressStart = prevElo.GetProgressToNextRank();
        progressTarget = newElo.GetProgressToNextRank();
        progressTimeElapsed = 0;
    }
}

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
    [SerializeField]
    private TextMeshProUGUI ratingNumberText;

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
    private bool rankChangeEffectPlayed;
    private EloData prevElo;
    private EloData newElo;
    private EloData.RankCategory displayedRank;

	// Use this for initialization
	void Start () {
        menu.SetActive(false);
        basePos = menu.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
        if (dropping) Drop();
        else if (progressFilling) SetProgress();
        if (Input.GetKeyDown(KeyCode.W)) OnGameEnd(true, 
            new EloData(0, 0.08f, 0, 0),
             new EloData(0, 0.12f, 0, 0));
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
        float progress;
        if (prevElo.GetRank() != newElo.GetRank())
        {
            if (progressTimeElapsed <= progressBarFillDur / 2)
            {
                progress = Easing.QuadEaseOut(
                    progressTimeElapsed /
                    (progressBarFillDur / 2));
                DisplayInterimProgress(progress, true);
            }
            else
            {
                if (!rankChangeEffectPlayed) RankChangeEffect();
                progress = Easing.QuadEaseOut(
                    (progressTimeElapsed - (progressTimeElapsed / 2)) /
                    (progressBarFillDur / 2));
                DisplayInterimProgress(progress, false);
            }
        }
        else
        {
            progress = progress = Easing.QuadEaseOut(
                    progressTimeElapsed / progressBarFillDur);
            if (displayedRank == EloData.RankCategory.Master)
                DisplayProgress(prevElo.GetRating(), newElo.GetRating(), progress);
            else
                DisplayProgress(prevElo.GetProgressToNextRank(),
                    newElo.GetProgressToNextRank(), progress);
        }
        
       if(progressTimeElapsed >= progressBarFillDur)
        {
            progressFilling = false;
        }
    }

    private void DisplayProgress(float start, float target, float progress)
    {
        if(displayedRank == EloData.RankCategory.Master)
        {
            int lerpedRating = Mathf.RoundToInt(Mathf.Lerp(
                start, target, progress));
            ratingNumberText.text = lerpedRating.ToString();
        }
        else
        {
            rankProgressBar.fillAmount = 
                Mathf.Lerp(start, target, progress);
        }
    }

    private void DisplayInterimProgress(float progress, bool firstHalf)
    {
        float start;
        float target;
        if (displayedRank == EloData.RankCategory.Master)
        {
            if (firstHalf)
            {
                start = prevElo.GetRating();
                target = EloData.FormatRating(EloData.GetRankMin(
                    EloData.RankCategory.Master));
            }
            else
            {
                start = EloData.FormatRating(EloData.GetRankMin(
                    EloData.RankCategory.Master));
                target = newElo.GetRating();
            }
        }
        else
        {
            if (firstHalf)
            {
                start = prevElo.GetProgressToNextRank();
                target = 1;
            }
            else
            {
                start = 0;
                target = newElo.GetProgressToNextRank();
            }
        }
        DisplayProgress(start, target, progress);
    }

    private void RankChangeEffect()
    {
        rankChangeEffectPlayed = true;
        rankImage.sprite = rankImageTarget;
        displayedRank = newElo.GetRank();
        rankProgressBar.transform.parent.gameObject.SetActive(
            newElo.GetRank() != EloData.RankCategory.Master);
        ratingNumberText.gameObject.SetActive(
            newElo.GetRank() == EloData.RankCategory.Master);
        GameObject particlePrefab;
        if (win) particlePrefab = Services.Prefabs.RankUp;
        else particlePrefab = Services.Prefabs.RankDown;
        OverlayParticles.ShowParticles(particlePrefab, rankImage.transform.position + (50 * Vector3.down));
    }

    public void OnGameEnd(bool victory, EloData prevElo, EloData newElo)
    {
        this.prevElo = prevElo;
        this.newElo = newElo;
        dropping = true;
        menu.transform.localPosition = basePos + (offset * Vector3.up);
        menu.SetActive(true);
        defeatSymbol.SetActive(!victory);
        victorySymbol.SetActive(victory);
        win = victory;
        rankImage.sprite = prevElo.GetRankImage();
        rankProgressBar.fillAmount = prevElo.GetProgressToNextRank();
        rankProgressBar.transform.parent.gameObject.SetActive(
            prevElo.GetRank() != EloData.RankCategory.Master);
        ratingNumberText.gameObject.SetActive(
            prevElo.GetRank() == EloData.RankCategory.Master);
        ratingNumberText.text = prevElo.GetRating().ToString();
        rankImageTarget = newElo.GetRankImage();
        progressStart = prevElo.GetProgressToNextRank();
        progressTarget = newElo.GetProgressToNextRank();
        progressTimeElapsed = 0;
        rankChangeEffectPlayed = false;
        displayedRank = prevElo.GetRank();
    }
}

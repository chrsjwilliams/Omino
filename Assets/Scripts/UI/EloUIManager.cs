using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EloUIManager : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI winCount;
    [SerializeField]
    private TextMeshProUGUI streakCount;
    [SerializeField]
    private TextMeshProUGUI rating;
    [SerializeField]
    private TextMeshProUGUI bestRating;
    [SerializeField]
    private TextMeshProUGUI rankText;
    [SerializeField]
    private TextMeshProUGUI bestRankText;
    [SerializeField]
    private Image rankImage;
    [SerializeField]
    private Image bestRankImage;
    [SerializeField]
    private Image progressBar;

    public void SetUI(EloData data)
    {
        EloData.RankCategory rank = data.GetRank();
        EloData.RankCategory highestRank = data.GetHighestRank();
        winCount.text = data.totalWins.ToString();
        streakCount.text = data.winStreakCount.ToString();
        rating.text = data.GetRating().ToString();
        rankText.text = rank.ToString();
        rankImage.sprite = data.GetRankImage();
        bestRating.text = data.GetHighestRating().ToString();
        bestRankText.text = highestRank.ToString();
        bestRankImage.sprite = data.GetHighestRankImage();
        rating.gameObject.SetActive(
            rank == EloData.RankCategory.Master);
        bestRating.gameObject.SetActive(
            highestRank == EloData.RankCategory.Master);
        progressBar.transform.parent.gameObject.SetActive(rank != EloData.RankCategory.Master);
        progressBar.fillAmount = data.GetProgressToNextRank();
    }

    public void ResetRank()
    {
        ELOManager.ResetRank();
        SetUI(ELOManager.eloData);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    EloData fakeElo = new EloData(0, 0.39f, 50, 0.41f);
        //    SetUI(fakeElo);
        //}
    }

}

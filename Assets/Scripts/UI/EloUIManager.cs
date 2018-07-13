using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EloUIManager : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI winCount;
    [SerializeField]
    private TextMeshProUGUI streakCount;
    [SerializeField]
    private TextMeshProUGUI rating;

    public void SetUI(EloData data)
    {
        winCount.text = data.totalWins.ToString();
        streakCount.text = data.winStreakCount.ToString();
        rating.text = data.GetRating().ToString();
    }

    public void ResetRank()
    {
        ELOManager.ResetRank();
        SetUI(ELOManager.eloData);
    }
    
}

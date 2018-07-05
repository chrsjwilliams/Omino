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
    private TextMeshProUGUI difficultyMultipler;

    public void SetUI(EloData data)
    {
        winCount.text = data.totalWins.ToString();
        streakCount.text = data.winStreakCount.ToString();
        difficultyMultipler.text = 
            Mathf.RoundToInt(100*(1 + data.handicapLevel)).ToString();
    }

    public void ResetRank()
    {
        ELOManager.ResetRank();
        SetUI(ELOManager.eloData);
    }
    
}

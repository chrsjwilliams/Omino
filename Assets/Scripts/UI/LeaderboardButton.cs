using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardButton : MonoBehaviour {

public void ShowLeaderboard()
    {
        Services.LeaderBoard.ShowLeaderBoardUI();
    }
}

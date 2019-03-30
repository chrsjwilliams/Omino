using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GameCenterLeaderBoard {


    UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform GCPlatform = new UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform();
    // Use this for initialization
    public GameCenterLeaderBoard() {
        Social.localUser.Authenticate(ProcessAuthentication);
    }
	
    void ProcessAuthentication(bool success)
    {
        if (success) Debug.Log("Authenticated, checking achievements.");
        else Debug.Log("Failed to authenticate");

    }

    public void ReportScore(long score, string leaderboardID)
    {
        Debug.Log("Reporting score " + score + " on leaderbaord " + leaderboardID);
        Social.ReportScore(score, leaderboardID,
            success => {
                Debug.Log(success ? "Reported score successfully" : "Failed to report score");
            }
        );
    }

    public void ShowLeaderBoardUI()
    {
        //Social.ShowLeaderboardUI();
        GCPlatform.ShowLeaderboardUI();
    }
}

using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GameCenterLeaderBoard : MonoBehaviour{


    UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform GCPlatform = new UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform();
    // Use this for initialization
    public void Init() {
        Social.localUser.Authenticate(ProcessAuthentication);
    }
	
    void ProcessAuthentication(bool success)
    {
        if (success) Debug.Log("Authenticated, checking achievements.");
        else Debug.Log("Failed to authenticate");

    }

    public void ReportScore(float score, string leaderboardID)
    {
        score = score * 1000;
        Debug.Log("Reporting score " + score + " on leaderbaord " + leaderboardID);
        Social.ReportScore((long)score, leaderboardID,
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

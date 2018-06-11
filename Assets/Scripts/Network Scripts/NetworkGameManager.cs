using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameManager : Photon.PunBehaviour {
	
	// Use this for initialization
	void Start () {
		Services.GameManager = GameObject.Find("Main").GetComponent<GameManager>();
		Services.GameManager.SetNumPlayers(new bool[] {true, true});
		
		Services.Scenes.Swap<NetworkGameSceneScript>();
	}
	
	// Update is called once per frame
	void Update () {
		if (PhotonNetwork.room.PlayerCount == 1)
		{
			Services.NetData.SetGameOverMessage("Opponent Disconnected");
			ReturnToTitle();
		}
	}

	void ReturnToTitle()
	{
		SceneManager.LoadScene(0);
	}
}

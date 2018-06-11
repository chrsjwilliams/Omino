using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameManager : Photon.PunBehaviour {

	// Use this for initialization
	void Start () {
		Services.Scenes.Swap<GameSceneScript>();

		if (!PhotonNetwork.isMasterClient)
		{
			Camera.main.transform.Rotate(0, 0, 180);
		}
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

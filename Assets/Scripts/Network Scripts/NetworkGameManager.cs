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
		
	}

	void ReturnToTitle()
	{
		
	}

	private void OnPlayerDisconnected(NetworkPlayer player)
	{
		Services.NetData.SetGameOverMessage("Opponent Disconnected");
		SceneManager.LoadScene(0);
	}
}

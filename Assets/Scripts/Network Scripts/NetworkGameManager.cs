using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameManager : Photon.PunBehaviour {

	void Awake()
	{
		Services.GameManager = new GameManager();
		Services.GameManager.Init();
	}
	
	// Use this for initialization
	void Start () {
		Services.Scenes.Swap<GameOptionsSceneScript>();
		
		if (!PhotonNetwork.player.isMasterClient)
		{
			Debug.Log("I'm not the master.");
			//Screen.orientation = ScreenOrientation.PortraitUpsideDown;
			Camera.main.gameObject.AddComponent<MirrorFlipCamera>();
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

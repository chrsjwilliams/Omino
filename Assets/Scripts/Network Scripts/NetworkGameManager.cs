using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Services.Scenes.Swap<GameSceneScript>();

		if (!PhotonNetwork.isMasterClient)
		{
			Camera.main.transform.Rotate(Vector3.back * 180);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void ReturnToTitle()
	{
		
	}
}

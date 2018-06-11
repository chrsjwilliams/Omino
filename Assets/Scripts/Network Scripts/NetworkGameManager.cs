using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Services.Scenes.PushScene<GameSceneScript>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void ReturnToTitle()
	{
		
	}
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{

	public TextMeshProUGUI loadingText;
	private bool not_loaded = true;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (not_loaded)
		{
			StartCoroutine(LoadGameAsync());
			not_loaded = false;
		}
		
		switch (Mathf.FloorToInt(Time.time) % 4)
		{
			case (0) :
				loadingText.text = "LOADING";
				break;
			case (1) :
				loadingText.text = "LOADING.";
				break;
			case (2) :
				loadingText.text = "LOADING..";
				break;
			case (3) :
				loadingText.text = "LOADING...";
				break;
		}	
	}

	IEnumerator LoadGameAsync()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

		// Wait until the asynchronous scene fully loads
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class LoadingScreenController : MonoBehaviour
{

	public TextMeshProUGUI loadingText;
	private bool load_started = false;

	void Awake() {
	}
	
	// Update is called once per frame
	void Update () {
		if ((!load_started) && (SplashScreen.isFinished))
		{
			StartCoroutine(LoadGameAsync());
			load_started = true;
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
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

		asyncLoad.allowSceneActivation = false;
		// Wait until the asynchronous scene fully loads
		while (asyncLoad.progress < 0.9f || Time.time < 3f)
		{
			yield return null;
		}
		asyncLoad.allowSceneActivation = true;
		while (!asyncLoad.isDone) {
			yield return null;
		}

		SceneManager.UnloadSceneAsync (0);
	}
}

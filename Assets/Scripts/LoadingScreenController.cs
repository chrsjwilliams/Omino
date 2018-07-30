using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class LoadingScreenController : MonoBehaviour
{
	IEnumerator Start() {
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
		while (!asyncLoad.isDone) {
			yield return asyncLoad;
		}
		SceneManager.UnloadSceneAsync(0);
	}
	
	/*
	private bool load_started = false;

	void Update () {
		if ((!load_started) && (SplashScreen.isFinished))
		{
			Application.backgroundLoadingPriority = ThreadPriority.Low;
			StartCoroutine(LoadGameAsync());
			load_started = true;
		}
	}

	IEnumerator LoadGameAsync()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

		asyncLoad.allowSceneActivation = false;
		// Wait until the asynchronous scene fully loads
		while (asyncLoad.progress < 0.9f || Time.time > 3.0f)
		{
			yield return null;
		}
		
		asyncLoad.allowSceneActivation = true;
		
		while (!asyncLoad.isDone) {
			yield return null;
		}
		
		SceneManager.UnloadSceneAsync (0);
	}
	*/
}

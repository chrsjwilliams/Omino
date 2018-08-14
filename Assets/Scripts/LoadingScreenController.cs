using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class LoadingScreenController : MonoBehaviour
{
	private bool load_started = false;
	
	#pragma strict
	
	#if UNITY_IOS

	void OnEnable() {
		DontDestroyOnLoad(this.gameObject);
		Handheld.SetActivityIndicatorStyle(ActivityIndicatorStyle.WhiteLarge);
		Handheld.StartActivityIndicator();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}
	
	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		Handheld.StopActivityIndicator();
		Handheld.Vibrate();
		SceneManager.sceneLoaded -= OnSceneLoaded;
		Destroy(gameObject);
	}
	
	#endif

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
		
		SceneManager.UnloadSceneAsync(0);
	}
}

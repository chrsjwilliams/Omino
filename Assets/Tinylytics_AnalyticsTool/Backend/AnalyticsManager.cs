using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tinylytics{
	[AddComponentMenu("")] 
	public class AnalyticsManager : MonoBehaviour
	{
		public float logInterval = 1.0f; // Logging interval in seconds
		
		private const string TOTAL_PLAYTIME = "TOTALPLAYTIME";
		private float _totalPlaytime = 0.0f;
		private float _sessionStartTime = 0.0f;
		private float _lastPlaytimeLog = 0.0f;
		private bool _pauseCallSent = false;
		private TaskManager _taskManager;

		private static AnalyticsManager _instance;
		public static AnalyticsManager Instance { get { return _instance; } }

		private void Awake() {
			if (_instance != null && _instance != this) {
				Destroy(this.gameObject);
			} else {
				_instance = this;
				DontDestroyOnLoad(this);
			}
		}
	
	
		void Start () {
			_sessionStartTime = Time.time;
			_lastPlaytimeLog = Time.time;
			
			if (PlayerPrefs.HasKey(TOTAL_PLAYTIME))
			{
				_totalPlaytime = PlayerPrefs.GetFloat(TOTAL_PLAYTIME);
			}

			_UpdateTotalPlaytime();
			_taskManager = new TaskManager();
			_LoadIntermittentLoggingTask();
			
		}

		void Update()
		{
			_UpdateTotalPlaytime();
			_taskManager.Update();
		}

		private void _LoadIntermittentLoggingTask()
		{
			ActionTask log = new ActionTask(_IntermittentLogs);
			Wait wait = new Wait(1.0f);
			ActionTask reset = new ActionTask(_LoadIntermittentLoggingTask);

			log.Then(wait);
			wait.Then(reset);
			
			_taskManager.Do(log);
		}

		void _IntermittentLogs()
		{
			_LogTotalPlaytime();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				if (!_pauseCallSent)
				{
					BackendManager.SendData("Session Playtime", (Time.time - Instance._sessionStartTime).ToString());
					_pauseCallSent = true;
				}
			}
			else
			{
				if (_pauseCallSent)
					_sessionStartTime = Time.time;
				_pauseCallSent = false;
			}
		}

		private void _UpdateTotalPlaytime()
		{
			_totalPlaytime += (Time.time - Instance._lastPlaytimeLog);
			_lastPlaytimeLog = Time.time;
			PlayerPrefs.SetFloat(TOTAL_PLAYTIME, _totalPlaytime);
			PlayerPrefs.Save();
		}

		private void _LogTotalPlaytime()
		{
			_UpdateTotalPlaytime();
			BackendManager.SendData("Total Playtime", PlayerPrefs.GetFloat(TOTAL_PLAYTIME).ToString());
		}
	
		public static void LogCustomMetric(string metricName, string dataToSend){
			BackendManager.SendData(metricName, dataToSend);
		}
	}
}
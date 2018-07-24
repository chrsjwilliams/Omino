using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tinylytics{
	[AddComponentMenu("")] 
	public class AnalyticsManager : MonoBehaviour
	{
		public float logInterval = 10.0f; // Logging interval in seconds
		public static AnalyticsData analyticsData { get; private set; }
		
		private const string fileName = "AnalyticsDataFile";

		private float _playModeStartTime = 0.0f;
		private int _playedBlueprint = 0;
		private float _totalPlaytime = 0.0f;
		private int _totalSessions = 0;
		private float _sessionStartTime = 0.0f;
		private float _lastPlaytimeLog = 0.0f;
		private bool _pauseCallSent = false;
		private TaskManager _taskManager;
		private bool _recievedGameOver = true;

		private TitleSceneScript.GameMode currentPlayMode;

		private static AnalyticsManager _instance;
		public static AnalyticsManager Instance { get { return _instance; } }
		
		private static void _LoadData()
		{
			string filePath = Path.Combine(
				Application.persistentDataPath,
				fileName);
			FileStream file;
			BinaryFormatter bf = new BinaryFormatter();

			if (File.Exists(filePath))
			{
				file = File.OpenRead(filePath);
				analyticsData = (AnalyticsData)bf.Deserialize(file);
			}
			else
			{
				file = File.Create(filePath);
				analyticsData = new AnalyticsData();
				bf.Serialize(file, analyticsData);
			}

			file.Close();
		}

		private static void _SaveData()
		{
			string filePath = Path.Combine(
				Application.persistentDataPath,
				fileName);
			FileStream file;
			BinaryFormatter bf = new BinaryFormatter();

			file = File.OpenWrite(filePath);
			bf.Serialize(file, analyticsData);
			file.Close();
		}

		private void Awake() {
			if (_instance != null && _instance != this) {
				Destroy(this.gameObject);
			} else {
				_instance = this;
				DontDestroyOnLoad(this);
			}
		}
	
		void Start ()
		{
			_playModeStartTime = Time.time;
			_sessionStartTime = Time.time;
			_lastPlaytimeLog = Time.time;
			
			_LoadData();
			
			if (analyticsData.VERSION_NUMBER != Application.version)
			{
				analyticsData.VERSION_NUMBER = Application.version;
				_LogVersionNumber();
			}
			
			_UpdateTotalPlaytime();
			_taskManager = new TaskManager();
			_LoadIntermittentLoggingTask();

		}

		private int _SecondsSinceEpoch()
		{
			System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
		}

		void Update()
		{
			_UpdateTotalPlaytime();
			_taskManager.Update();
		}

		private void _LoadIntermittentLoggingTask()
		{
			ActionTask log = new ActionTask(_LogTotalPlaytime);
			Wait wait = new Wait(logInterval);
			ActionTask reset = new ActionTask(_LoadIntermittentLoggingTask);

			log.Then(wait);
			wait.Then(reset);
			
			_taskManager.Do(log);
		}

		private void _LogVersionNumber()
		{
			BackendManager.SendData("Version Number", analyticsData.VERSION_NUMBER);
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				if (!_pauseCallSent)
				{
					BackendManager.SendData("Session Playtime", (Time.time - Instance._sessionStartTime).ToString());
					analyticsData.TOTAL_SESSIONS = _totalSessions + 1;
					_SaveData();
					_pauseCallSent = true;
				}
			}
			else if (_pauseCallSent)
			{
				_sessionStartTime = Time.time;
				_totalSessions = analyticsData.TOTAL_SESSIONS;
				_pauseCallSent = false;
			}
		}

		private void _UpdateTotalPlaytime()
		{
			_totalPlaytime += (Time.time - Instance._lastPlaytimeLog);
			_lastPlaytimeLog = Time.time;
			analyticsData.TOTAL_PLAYTIME = _totalPlaytime;
			_SaveData();
		}

		private void _LogTotalPlaytime()
		{
			_UpdateTotalPlaytime();
			_LogTimeSinceFirstPlay();
			BackendManager.SendData("Total Playtime", analyticsData.TOTAL_PLAYTIME.ToString());
		}

		private void _LogTimeSinceFirstPlay()
		{
			BackendManager.SendData("Time Since Initial Load", (_SecondsSinceEpoch() - analyticsData.FIRST_LOAD).ToString());
		}
	
		public static void LogMetric(string metricName, string dataToSend){
			BackendManager.SendData(metricName, dataToSend);
		}

		public void ELOWin(bool winState)
		{
			LogMetric("ELO Win", winState.ToString());
		}

		public void ELORating(int rating)
		{
			LogMetric("ELO Rating", rating.ToString());
		}

		public void ELOStreak(int streakCount)
		{
			LogMetric("ELO Streak", streakCount.ToString());
		}

		public void ELOTotalWins(int totalWins)
		{
			LogMetric("ELO Total Wins", totalWins.ToString());
		}

		public void PlayedTutorial()
		{
			if (!analyticsData.PLAYED_TUTORIAL)
			{
				_UpdateTotalPlaytime();
				LogMetric("Total Time Until Tutorial Played", _totalPlaytime.ToString());
				LogMetric("Number of Sessions Until Tutorial Played", _totalSessions.ToString());
				analyticsData.PLAYED_TUTORIAL = true;
				_SaveData();
			}
		}
		
		public void PlayedBlueprint()
		{
			if (!analyticsData.PLAYED_BLUEPRINT)
			{
				_UpdateTotalPlaytime();
				LogMetric("Total Time Until Blueprint Played", _totalPlaytime.ToString());
				LogMetric("Number of Sessions Until Blueprint Played", _totalSessions.ToString());
				analyticsData.PLAYED_BLUEPRINT = true;
				_SaveData();
			}
		}
		
		public void MatchStarted(TitleSceneScript.GameMode modeIn)
		{
			_playModeStartTime = Time.time;
			currentPlayMode = modeIn;
			_recievedGameOver = false;
		}
		
		public void MatchEnded()
		{
			if (!_recievedGameOver)
			{
				string matchTypeToIncreaseTime = "";
				float time_played = Time.time - _playModeStartTime;
				float total_time_played = 0;
				int total_matches = 0;

				switch (currentPlayMode)
				{
					case (TitleSceneScript.GameMode.Campaign):
						analyticsData.TUTORIAL_PLAYTIME += time_played;
						total_time_played = analyticsData.TUTORIAL_PLAYTIME;
						analyticsData.TUTORIAL_TOTAL_MATCHES++;
						total_matches = analyticsData.TUTORIAL_TOTAL_MATCHES;
						matchTypeToIncreaseTime = "TUTORIAL";
						break;
					case (TitleSceneScript.GameMode.Demo):
						analyticsData.DEMO_PLAYTIME += time_played;
						total_time_played = analyticsData.DEMO_PLAYTIME;
						analyticsData.DEMO_TOTAL_MATCHES++;
						total_matches = analyticsData.DEMO_TOTAL_MATCHES;
						matchTypeToIncreaseTime = "DEMO";
						break;
					case (TitleSceneScript.GameMode.DungeonRun):
						analyticsData.DUNGEON_RUN_PLAYTIME += time_played;
						total_time_played = analyticsData.DUNGEON_RUN_PLAYTIME;
						analyticsData.DUNGEON_RUN_TOTAL_MATCHES++;
						total_matches = analyticsData.DUNGEON_RUN_TOTAL_MATCHES;
						matchTypeToIncreaseTime = "DUNGEON RUN";
						break;
					case (TitleSceneScript.GameMode.Elo):
						analyticsData.ELO_PLAYTIME += time_played;
						total_time_played = analyticsData.ELO_PLAYTIME;
						analyticsData.ELO_TOTAL_MATCHES++;
						total_matches = analyticsData.ELO_TOTAL_MATCHES;
						matchTypeToIncreaseTime = "ELO";
						break;
					case (TitleSceneScript.GameMode.PlayerVsAI):
						analyticsData.PVAI_PLAYTIME += time_played;
						total_time_played = analyticsData.PVAI_PLAYTIME;
						analyticsData.PVP_TOTAL_MATCHES++;
						total_matches = analyticsData.PVP_TOTAL_MATCHES;
						matchTypeToIncreaseTime = "PVAI";
						break;
					case (TitleSceneScript.GameMode.TwoPlayers):
						analyticsData.PVP_PLAYTIME += time_played;
						total_time_played = analyticsData.PVP_PLAYTIME;
						analyticsData.PVP_TOTAL_MATCHES++;
						total_matches = analyticsData.PVP_TOTAL_MATCHES;
						matchTypeToIncreaseTime = "PVP";
						break;
				}

				LogMetric(matchTypeToIncreaseTime + " CURRENT MATCH", time_played.ToString());
				LogMetric(matchTypeToIncreaseTime + " TOTAL", total_time_played.ToString());
				LogMetric(matchTypeToIncreaseTime + " MATCHES", total_matches.ToString());
			}

			_SaveData();
			_recievedGameOver = true;
		}

		public void PlayerWin(bool playerIsWinner = true)
		{
			if (currentPlayMode == TitleSceneScript.GameMode.PlayerVsAI)
			{
				if (playerIsWinner)
				{
					analyticsData.PVAI_TOTAL_WINS++;
					LogMetric("PVAI TOTAL WINS", analyticsData.PVAI_TOTAL_WINS.ToString());
				}
				else
				{
					analyticsData.PVAI_TOTAL_LOSSES++;
					LogMetric("PVAI TOTAL LOSSES", analyticsData.PVAI_TOTAL_LOSSES.ToString());
				}
				
			}
			else if (currentPlayMode == TitleSceneScript.GameMode.Campaign)
			{
				if (playerIsWinner)
				{
					analyticsData.TUTORIAL_TOTAL_WINS++;
					LogMetric("TUTORIAL TOTAL WINS", analyticsData.TUTORIAL_TOTAL_WINS.ToString());
				}
				else
				{
					analyticsData.TUTORIAL_TOTAL_LOSSES++;
					LogMetric("TUTORIAL TOTAL LOSSES", analyticsData.TUTORIAL_TOTAL_LOSSES.ToString());
				}
			}
			_SaveData();
		}

		public void TechSelected(BuildingType tech, List<BuildingType> techChoices)
		{
			string toReturn = "";
			foreach (BuildingType techChoice in techChoices)
			{
				toReturn += techChoice.ToString() + " | ";
			}

			LogMetric("Dungeon Run Tech Selected", tech.ToString() + " from (" + toReturn + ")");
		}

		public void DungeonRunLoss(int challengeNumber)
		{
			LogMetric("Dungeon Run Lost - Number of Matches Won", (challengeNumber - 1).ToString());
		}

		public void DungeonRunWin(List<BuildingType> techList)
		{
			string toReturn = "";
			foreach (BuildingType tech in techList)
			{
				toReturn += tech.ToString() + " | ";
			}
			
			LogMetric("Dungeon Run Won With Tech", toReturn);
		}
	}

	[System.Serializable]
	public class AnalyticsData
	{
		public float TOTAL_PLAYTIME;
		public bool PLAYED_TUTORIAL;
		public int TOTAL_SESSIONS;
		public bool PLAYED_BLUEPRINT;
		public float ELO_PLAYTIME;
		public int ELO_TOTAL_MATCHES;
		public float PVP_PLAYTIME;
		public int PVP_TOTAL_MATCHES;
		public float PVAI_PLAYTIME;
		public int PVAI_TOTAL_MATCHES;
		public int PVAI_TOTAL_WINS;
		public int PVAI_TOTAL_LOSSES;
		public float DUNGEON_RUN_PLAYTIME;
		public int DUNGEON_RUN_TOTAL_MATCHES;
		public float DEMO_PLAYTIME;
		public int DEMO_TOTAL_MATCHES;
		public float TUTORIAL_PLAYTIME;
		public int TUTORIAL_TOTAL_MATCHES;
		public int TUTORIAL_TOTAL_WINS;
		public int TUTORIAL_TOTAL_LOSSES;
		public string VERSION_NUMBER;
		public int FIRST_LOAD;

		public AnalyticsData()
		{
			TOTAL_PLAYTIME = 0.0f;
			PLAYED_TUTORIAL = false;
			TOTAL_SESSIONS = 0;
			PLAYED_BLUEPRINT = false;
			ELO_PLAYTIME = 0.0f;
			ELO_TOTAL_MATCHES = 0;
			PVP_PLAYTIME = 0.0f;
			PVP_TOTAL_MATCHES = 0;
			PVAI_PLAYTIME = 0.0f;
			PVAI_TOTAL_MATCHES = 0;
			PVAI_TOTAL_WINS = 0;
			PVAI_TOTAL_LOSSES = 0;
			DUNGEON_RUN_PLAYTIME = 0.0f;
			DUNGEON_RUN_TOTAL_MATCHES = 0;
			DEMO_PLAYTIME = 0.0f;
			DEMO_TOTAL_MATCHES = 0;
			TUTORIAL_TOTAL_WINS = 0;
			TUTORIAL_TOTAL_LOSSES = 0;
			TUTORIAL_PLAYTIME = 0.0f;
			VERSION_NUMBER = Application.version;
			FIRST_LOAD = _SecondsSinceEpoch();
		}
		
		private int _SecondsSinceEpoch()
		{
			System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
		}
	}
}
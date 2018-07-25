using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tinylytics{
	[AddComponentMenu("")] 
	public class AnalyticsManager : MonoBehaviour
	{
		public float logInterval = 10.0f; // Logging interval in seconds
		public static AnalyticsData analyticsData { get; private set; }
		
		private const string _fileName = "AnalyticsData";

		private float _playModeStartTime = 0.0f;
		private float _sessionStartTime = 0.0f;
		private float _lastPlaytimeLog = 0.0f;
		private bool _pauseCallSent = false;
		private TaskManager _taskManager;
		private bool _recievedGameOver = true;
		private TitleSceneScript.GameMode _currentPlayMode;

		private static AnalyticsManager _instance;
		public static AnalyticsManager Instance { get { return _instance; } }
		
		private static void _LoadData()
		{
			string filePath = Path.Combine(
				Application.persistentDataPath,
				_fileName);
			FileStream file;
			BinaryFormatter bf = new BinaryFormatter();

			if (File.Exists(filePath))
			{
				file = File.OpenRead(filePath);
				try
				{
					analyticsData = (AnalyticsData) bf.Deserialize(file);
				}
				catch (SerializationException e)
				{
					Debug.Log("Failed to deserialize, reason : " + e.Message);
					file.Dispose();
					_ResetData();
					_SaveData();
					// throw;
				}
				finally
				{
					file.Close();
				}
			}
			else
			{
				file = File.Create(filePath);
				analyticsData = new AnalyticsData();
				bf.Serialize(file, analyticsData);
				file.Close();
			}
		}

		private static void _SaveData()
		{
			string filePath = Path.Combine(
				Application.persistentDataPath,
				_fileName);
			FileStream file;
			BinaryFormatter bf = new BinaryFormatter();

			file = File.OpenWrite(filePath);
			bf.Serialize(file, analyticsData);
			file.Close();
		}

		private static void _ResetData()
		{
			string filePath = Path.Combine(
				Application.persistentDataPath,
				_fileName);
			FileStream file;
			BinaryFormatter bf = new BinaryFormatter();
			
			file = File.Create(filePath);
			analyticsData = new AnalyticsData();
			bf.Serialize(file, analyticsData);
			
			file.Close();
		}

		void Awake() {
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
			
			if (analyticsData.versionNumber != Application.version)
			{
				analyticsData.versionNumber = Application.version;
				_LogVersionNumber();
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

		void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				if (!_pauseCallSent)
				{
					analyticsData.totalSessions++;
					BackendManager.SendData("Session Playtime", (Time.time - Instance._sessionStartTime).ToString());
					BackendManager.SendData("Total Sessions", analyticsData.totalSessions.ToString());
					_SaveData();
					_pauseCallSent = true;
				}
			}
			else if (_pauseCallSent)
			{
				_sessionStartTime = Time.time;
				_pauseCallSent = false;
			}
		}

		void OnApplicationQuit()
		{
			if (!_pauseCallSent)
			{
				analyticsData.totalSessions++;
				_SaveData();
			}
		}

		private int _SecondsSinceEpoch()
		{
			System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
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
			BackendManager.SendData("Version Number", analyticsData.versionNumber);
		}

		private void _UpdateTotalPlaytime()
		{
			analyticsData.totalPlaytime += (Time.time - Instance._lastPlaytimeLog);
			_lastPlaytimeLog = Time.time;
			_SaveData();
		}

		private void _LogTotalPlaytime()
		{
			_UpdateTotalPlaytime();
			_LogTimeSinceFirstPlay();
			BackendManager.SendData("Total Playtime", analyticsData.totalPlaytime.ToString());
		}

		private void _LogTimeSinceFirstPlay()
		{
			BackendManager.SendData("Time Since Initial Load", (_SecondsSinceEpoch() - analyticsData.firstLoadTime).ToString());
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
			if (!analyticsData.playedTutorial)
			{
				_UpdateTotalPlaytime();
				LogMetric("Total Time Until Tutorial Played", analyticsData.totalPlaytime.ToString());
				LogMetric("Number of Sessions Until Tutorial Played", analyticsData.totalSessions.ToString());
				analyticsData.playedTutorial = true;
				_SaveData();
			}
		}
		
		public void PlayedBlueprint()
		{
			if (!analyticsData.playedBlueprint)
			{
				_UpdateTotalPlaytime();
				LogMetric("Total Time Until Blueprint Played", analyticsData.totalPlaytime.ToString());
				LogMetric("Number of Sessions Until Blueprint Played", analyticsData.totalSessions.ToString());
				analyticsData.playedBlueprint = true;
				_SaveData();
			}
		}
		
		public void MatchStarted(TitleSceneScript.GameMode modeIn)
		{
			_playModeStartTime = Time.time;
			_currentPlayMode = modeIn;
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

				switch (_currentPlayMode)
				{
					case (TitleSceneScript.GameMode.Tutorial):
						analyticsData.tutorialPlaytime += time_played;
						total_time_played = analyticsData.tutorialPlaytime;
						analyticsData.tutorialTotalMatches++;
						total_matches = analyticsData.tutorialTotalMatches;
						matchTypeToIncreaseTime = "TUTORIAL";
						break;
					case (TitleSceneScript.GameMode.Demo):
						analyticsData.demoPlaytime += time_played;
						total_time_played = analyticsData.demoPlaytime;
						analyticsData.demoTotalMatches++;
						total_matches = analyticsData.demoTotalMatches;
						matchTypeToIncreaseTime = "DEMO";
						break;
					case (TitleSceneScript.GameMode.DungeonRun):
						analyticsData.dungeonRunPlaytime += time_played;
						total_time_played = analyticsData.dungeonRunPlaytime;
						analyticsData.dungeonRunTotalMatches++;
						total_matches = analyticsData.dungeonRunTotalMatches;
						matchTypeToIncreaseTime = "DUNGEON RUN";
						break;
					case (TitleSceneScript.GameMode.Elo):
						analyticsData.eloPlaytime += time_played;
						total_time_played = analyticsData.eloPlaytime;
						analyticsData.eloTotalMatches++;
						total_matches = analyticsData.eloTotalMatches;
						matchTypeToIncreaseTime = "ELO";
						break;
					case (TitleSceneScript.GameMode.PlayerVsAI):
						analyticsData.PvAIPlaytime += time_played;
						total_time_played = analyticsData.PvAIPlaytime;
						analyticsData.PvPTotalMatches++;
						total_matches = analyticsData.PvAITotalMatches;
						matchTypeToIncreaseTime = "PVAI";
						break;
					case (TitleSceneScript.GameMode.TwoPlayers):
						analyticsData.pvpPlaytime += time_played;
						total_time_played = analyticsData.pvpPlaytime;
						analyticsData.PvPTotalMatches++;
						total_matches = analyticsData.PvPTotalMatches;
						matchTypeToIncreaseTime = "PVP";
						break;
				}

				LogMetric(matchTypeToIncreaseTime + " CURRENT MATCH", time_played.ToString());
				LogMetric(matchTypeToIncreaseTime + " PLAYTIME TOTAL", total_time_played.ToString());
				LogMetric(matchTypeToIncreaseTime + " TOTAL MATCHES", total_matches.ToString());
			}

			_SaveData();
			_recievedGameOver = true;
		}

		public void PlayerWin(bool playerIsWinner = true)
		{
			if (_currentPlayMode == TitleSceneScript.GameMode.PlayerVsAI)
			{
				if (playerIsWinner)
				{
					analyticsData.PvAITotalWins++;
					LogMetric("PVAI TOTAL WINS", analyticsData.PvAITotalWins.ToString());
				}
				else
				{
					analyticsData.PvAITotalLosses++;
					LogMetric("PVAI TOTAL LOSSES", analyticsData.PvAITotalLosses.ToString());
				}
				
			}
			else if (_currentPlayMode == TitleSceneScript.GameMode.Tutorial)
			{
				if (playerIsWinner)
				{
					analyticsData.tutorialTotalWins++;
					LogMetric("TUTORIAL TOTAL WINS", analyticsData.tutorialTotalWins.ToString());
				}
				else
				{
					analyticsData.tutorialTotalLosses++;
					LogMetric("TUTORIAL TOTAL LOSSES", analyticsData.tutorialTotalLosses.ToString());
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
		public float totalPlaytime;
		public bool playedTutorial;
		public int totalSessions;
		public bool playedBlueprint;
		public float eloPlaytime;
		public int eloTotalMatches;
		public float pvpPlaytime;
		public int PvPTotalMatches;
		public float PvAIPlaytime;
		public int PvAITotalMatches;
		public int PvAITotalWins;
		public int PvAITotalLosses;
		public float dungeonRunPlaytime;
		public int dungeonRunTotalMatches;
		public float demoPlaytime;
		public int demoTotalMatches;
		public float tutorialPlaytime;
		public int tutorialTotalMatches;
		public int tutorialTotalWins;
		public int tutorialTotalLosses;
		public string versionNumber;
		public int firstLoadTime;

		public AnalyticsData()
		{
			totalPlaytime = 0.0f;
			playedTutorial = false;
			totalSessions = 0;
			playedBlueprint = false;
			eloPlaytime = 0.0f;
			eloTotalMatches = 0;
			pvpPlaytime = 0.0f;
			PvPTotalMatches = 0;
			PvAIPlaytime = 0.0f;
			PvAITotalMatches = 0;
			PvAITotalWins = 0;
			PvAITotalLosses = 0;
			dungeonRunPlaytime = 0.0f;
			dungeonRunTotalMatches = 0;
			demoPlaytime = 0.0f;
			demoTotalMatches = 0;
			tutorialTotalWins = 0;
			tutorialTotalLosses = 0;
			tutorialPlaytime = 0.0f;
			versionNumber = Application.version;
			firstLoadTime = _SecondsSinceEpoch();
		}
		
		private int _SecondsSinceEpoch()
		{
			System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
		}
	}
}
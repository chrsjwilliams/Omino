using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using UnityEngine;

namespace Tinylytics{
	[AddComponentMenu("")] 
	public class AnalyticsManager : MonoBehaviour
	{
		public float logInterval = 10.0f; // Logging interval in seconds
		
		private const string TOTAL_PLAYTIME = "TOTAL PLAYTIME";
		private const string PLAYED_TUTORIAL = "PLAYED TUTORIAL";
		private const string TOTAL_SESSIONS = "TOTAL SESSIONS";
		private const string PLAYED_BLUEPRINT = "PLAYED BLUEPRINT";
		private const string ELO_PLAYTIME = "ELO PLAYTIME";
		private const string ELO_TOTAL_MATCHES = "ELO TOTAL MATCHES";
		private const string PVP_PLAYTIME = "PVP PLAYTIME";
		private const string PVP_TOTAL_MATCHES = "PVP TOTAL MATCHES";
		private const string PVAI_PLAYTIME = "PVAI PLAYTIME";
		private const string PVAI_TOTAL_MATCHES = "PVAI TOTAL MATCHES";
		private const string PVAI_TOTAL_WINS = "PVAI TOTAL WINS";
		private const string PVAI_TOTAL_LOSSES = "PVAI TOTAL LOSSES";
		private const string DUNGEON_RUN_PLAYTIME = "DUNGEON RUN PLAYTIME";
		private const string DUNGEON_RUN_TOTAL_MATCHES = "DUNGEON RUN TOTAL MATCHES";
		private const string DEMO_PLAYTIME = "DEMO PLAYTIME";
		private const string DEMO_TOTAL_MATCHES = "DEMO TOTAL MATCHES";
		private const string TUTORIAL_PLAYTIME = "TUTORIAL PLAYTIME";
		private const string TUTORIAL_TOTAL_MATCHES = "TUTORIAL TOTAL MATCHES";
		private const string TUTORIAL_TOTAL_WINS = "TUTORIAL TOTAL WINS";
		private const string TUTORIAL_TOTAL_LOSES = "TUTORIAL TOTAL LOSES";

		private float _playModeStartTime = 0.0f;
		private int _playedBlueprint = 0;
		private float _totalPlaytime = 0.0f;
		private int _totalSessions = 0;
		private float _sessionStartTime = 0.0f;
		private float _lastPlaytimeLog = 0.0f;
		private bool _pauseCallSent = false;
		private int _playedTutorial = 0;
		private TaskManager _taskManager;
		private bool _recievedGameOver = true;

		private TitleSceneScript.GameMode currentPlayMode;

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
	
		void Start ()
		{
			_playModeStartTime = Time.time;
			_sessionStartTime = Time.time;
			_lastPlaytimeLog = Time.time;
			
			_InitializePlayerPrefs();
			
			_UpdateTotalPlaytime();
			_taskManager = new TaskManager();
			_LoadIntermittentLoggingTask();
			
		}

		private void _InitializePlayerPrefs()
		{
			if (PlayerPrefs.HasKey(TOTAL_PLAYTIME))
			{
				_totalPlaytime = PlayerPrefs.GetFloat(TOTAL_PLAYTIME);
			}
			if (PlayerPrefs.HasKey(PLAYED_TUTORIAL))
			{
				_playedTutorial = PlayerPrefs.GetInt(PLAYED_TUTORIAL);
			}
			if (PlayerPrefs.HasKey(TOTAL_SESSIONS))
			{
				_totalSessions = PlayerPrefs.GetInt(TOTAL_SESSIONS);
			}
			if (PlayerPrefs.HasKey(PLAYED_BLUEPRINT))
			{
				_playedBlueprint = PlayerPrefs.GetInt(PLAYED_BLUEPRINT);
			}
			if (!PlayerPrefs.HasKey(ELO_PLAYTIME))
			{
				PlayerPrefs.SetFloat(ELO_PLAYTIME, 0.0f);
			}
			if (!PlayerPrefs.HasKey(ELO_TOTAL_MATCHES))
			{
				PlayerPrefs.SetInt(ELO_TOTAL_MATCHES, 0);
			}
			if (!PlayerPrefs.HasKey(PVP_PLAYTIME))
			{
				PlayerPrefs.SetFloat(PVP_PLAYTIME, 0.0f);
			}
			if (!PlayerPrefs.HasKey(PVP_TOTAL_MATCHES))
			{
				PlayerPrefs.SetInt(PVP_TOTAL_MATCHES, 0);
			}
			if (!PlayerPrefs.HasKey(PVAI_PLAYTIME))
			{
				PlayerPrefs.SetFloat(PVAI_PLAYTIME, 0.0f);
			}
			if (!PlayerPrefs.HasKey(PVAI_TOTAL_MATCHES))
			{
				PlayerPrefs.SetInt(PVAI_TOTAL_MATCHES, 0);
			}
			if (!PlayerPrefs.HasKey(DUNGEON_RUN_PLAYTIME))
			{
				PlayerPrefs.SetFloat(DUNGEON_RUN_PLAYTIME, 0.0f);
			}
			if (!PlayerPrefs.HasKey(DUNGEON_RUN_TOTAL_MATCHES))
			{
				PlayerPrefs.SetInt(DUNGEON_RUN_TOTAL_MATCHES, 0);
			}
			if (!PlayerPrefs.HasKey(DEMO_PLAYTIME))
			{
				PlayerPrefs.SetFloat(DEMO_PLAYTIME, 0.0f);
			}
			if (!PlayerPrefs.HasKey(DEMO_TOTAL_MATCHES))
			{
				PlayerPrefs.SetInt(DEMO_TOTAL_MATCHES, 0);
			}
			if (!PlayerPrefs.HasKey(PVAI_TOTAL_WINS))
			{
				PlayerPrefs.SetInt(PVAI_TOTAL_WINS, 0);
			}
			if (!PlayerPrefs.HasKey(PVAI_TOTAL_LOSSES))
			{
				PlayerPrefs.SetInt(PVAI_TOTAL_LOSSES, 0);
			}
			if (!PlayerPrefs.HasKey(TUTORIAL_TOTAL_WINS))
			{
				PlayerPrefs.SetInt(TUTORIAL_TOTAL_WINS, 0);
			}
			if (!PlayerPrefs.HasKey(TUTORIAL_TOTAL_LOSES))
			{
				PlayerPrefs.SetInt(TUTORIAL_TOTAL_LOSES, 0);
			}

			PlayerPrefs.Save();
		}

		void Update()
		{
			_UpdateTotalPlaytime();
			_taskManager.Update();
		}

		private void _LoadIntermittentLoggingTask()
		{
			ActionTask log = new ActionTask(_IntermittentLogs);
			Wait wait = new Wait(logInterval);
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
					PlayerPrefs.SetInt(TOTAL_SESSIONS, _totalSessions + 1);
					PlayerPrefs.Save();
					_pauseCallSent = true;
				}
			}
			else if (_pauseCallSent)
			{
				_sessionStartTime = Time.time;
				_totalSessions = PlayerPrefs.GetInt(TOTAL_SESSIONS);
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
	
		public static void LogMetric(string metricName, string dataToSend){
			BackendManager.SendData(metricName, dataToSend);
		}

		public void ELOWin(bool winState)
		{
			LogMetric("ELO Win", winState.ToString());
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
			if (_playedTutorial == 0)
			{
				_UpdateTotalPlaytime();
				LogMetric("Total Time Until Tutorial Played", _totalPlaytime.ToString());
				LogMetric("Number of Sessions Until Tutorial Played", _totalSessions.ToString());
				_playedTutorial = 1;
				PlayerPrefs.SetInt(PLAYED_TUTORIAL, 1);
				PlayerPrefs.Save();
			}
		}
		
		public void PlayedBlueprint()
		{
			if (_playedBlueprint == 0)
			{
				_UpdateTotalPlaytime();
				LogMetric("Total Time Until Blueprint Played", _totalPlaytime.ToString());
				LogMetric("Number of Sessions Until Blueprint Played", _totalSessions.ToString());
				_playedBlueprint = 1;
				PlayerPrefs.SetInt(PLAYED_BLUEPRINT, 1);
				PlayerPrefs.Save();
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
				string matchTypeToIncreaseNumberOfMatches = "";

				switch (currentPlayMode)
				{
					case (TitleSceneScript.GameMode.Campaign):
						matchTypeToIncreaseTime = TUTORIAL_PLAYTIME;
						matchTypeToIncreaseNumberOfMatches = TUTORIAL_TOTAL_MATCHES;
						break;
					case (TitleSceneScript.GameMode.Demo):
						matchTypeToIncreaseTime = DEMO_PLAYTIME;
						matchTypeToIncreaseNumberOfMatches = DEMO_TOTAL_MATCHES;
						break;
					case (TitleSceneScript.GameMode.DungeonRun):
						matchTypeToIncreaseTime = DUNGEON_RUN_PLAYTIME;
						matchTypeToIncreaseNumberOfMatches = DUNGEON_RUN_TOTAL_MATCHES;
						break;
					case (TitleSceneScript.GameMode.Elo):
						matchTypeToIncreaseTime = ELO_PLAYTIME;
						matchTypeToIncreaseNumberOfMatches = ELO_TOTAL_MATCHES;
						break;
					case (TitleSceneScript.GameMode.PlayerVsAI):
						matchTypeToIncreaseTime = PVAI_PLAYTIME;
						matchTypeToIncreaseNumberOfMatches = PVAI_TOTAL_MATCHES;
						break;
					case (TitleSceneScript.GameMode.TwoPlayers):
						matchTypeToIncreaseTime = PVP_PLAYTIME;
						matchTypeToIncreaseNumberOfMatches = PVP_TOTAL_MATCHES;
						break;
				}

				float previousTotalTime = PlayerPrefs.GetFloat(matchTypeToIncreaseTime);
				int previousTotalMatches = PlayerPrefs.GetInt(matchTypeToIncreaseNumberOfMatches);

				PlayerPrefs.SetFloat(matchTypeToIncreaseTime, previousTotalTime + (Time.time - _playModeStartTime));
				PlayerPrefs.SetInt(matchTypeToIncreaseNumberOfMatches, previousTotalMatches + 1);
				PlayerPrefs.Save();
				LogMetric(matchTypeToIncreaseTime + " CURRENT MATCH", (Time.time - _playModeStartTime).ToString());
				LogMetric(matchTypeToIncreaseTime + " TOTAL", PlayerPrefs.GetFloat(matchTypeToIncreaseTime).ToString());
				LogMetric(matchTypeToIncreaseNumberOfMatches, PlayerPrefs.GetInt(matchTypeToIncreaseNumberOfMatches).ToString());
			}

			_recievedGameOver = true;
		}

		public void PlayerWin(bool playerIsWinner = true)
		{
			if (currentPlayMode == TitleSceneScript.GameMode.PlayerVsAI)
			{
				if (playerIsWinner)
				{
					int previousTotalWins = PlayerPrefs.GetInt(PVAI_TOTAL_WINS);
					PlayerPrefs.SetInt(PVAI_TOTAL_WINS, previousTotalWins + 1);
					PlayerPrefs.Save();
					LogMetric(PVAI_TOTAL_WINS, PlayerPrefs.GetInt(PVAI_TOTAL_WINS).ToString());
				}
				else
				{
					int previousTotalWins = PlayerPrefs.GetInt(PVAI_TOTAL_LOSSES);
					PlayerPrefs.SetInt(PVAI_TOTAL_LOSSES, previousTotalWins + 1);
					PlayerPrefs.Save();
					LogMetric(PVAI_TOTAL_LOSSES, PlayerPrefs.GetInt(PVAI_TOTAL_LOSSES).ToString());
				}
			}
			else if (currentPlayMode == TitleSceneScript.GameMode.Campaign)
			{
				if (playerIsWinner)
				{
					int previousTotalWins = PlayerPrefs.GetInt(TUTORIAL_TOTAL_WINS);
					PlayerPrefs.SetInt(TUTORIAL_TOTAL_WINS, previousTotalWins + 1);
					PlayerPrefs.Save();
					LogMetric(TUTORIAL_TOTAL_WINS, PlayerPrefs.GetInt(TUTORIAL_TOTAL_WINS).ToString());
				}
				else
				{
					int previousTotalWins = PlayerPrefs.GetInt(TUTORIAL_TOTAL_LOSES);
					PlayerPrefs.SetInt(TUTORIAL_TOTAL_LOSES, previousTotalWins + 1);
					PlayerPrefs.Save();
					LogMetric(TUTORIAL_TOTAL_LOSES, PlayerPrefs.GetInt(TUTORIAL_TOTAL_LOSES).ToString());
				}
			}
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
			LogMetric("Dungeon Run Lost - Number of Matches Won", challengeNumber.ToString());
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
}
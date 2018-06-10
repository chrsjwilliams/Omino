﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameSceneScript : Scene<TransitionData>
{
    public const string TILE_MAP_HOLDER = "TileMapHolder";
    public static Transform tileMapHolder;

    private float _colorChangeTime;
    public Transform backgroundImage;
    public TaskManager tm;

    [SerializeField]
    private bool demoMode;
    [SerializeField]
    private bool evolutionMode;

    [SerializeField]
    private Color _backgroundColor;

    private bool gameStarted;
    public bool gamePaused { get; private set; }
    public bool gameInProgress
    {
        get { return gameStarted && !gamePaused; }
    }

    internal override void OnEnter(TransitionData data)
    {
        tileMapHolder = GameObject.Find(TILE_MAP_HOLDER).transform;
        Time.timeScale = 1;
        Services.GameScene = this;
        tm = new TaskManager();
        Services.GameEventManager = new GameEventsManager();
        Services.UIManager = GetComponentInChildren<UIManager>();
        Services.TutorialManager = GetComponentInChildren<TutorialManager>();
        _colorChangeTime = 0f;
        Services.MapManager.GenerateMap();
        if (evolutionMode)
        {
            Services.GameManager.InitPlayersEvoMode();
        }
        else
        {
            Services.GameManager.InitPlayers();
        }
        Services.AudioManager.SetMainTrack(Services.Clips.MainTrackAudio, 0.3f);
        Services.CameraController.SetScreenEdges();
    }

    public void PlayerHand()
    {
        for (int i = 0; i < Services.GameManager.Players.Length; i++)
        {
            Services.GameManager.Players[i].OrganizeHand(Services.GameManager.Players[i].hand, true);
        }
    }

    internal override void OnExit()
    {
        Time.timeScale = 1;
        Services.GameEventManager.Clear();
    }

	// Update is called once per frame
	void Update ()
    {
        _colorChangeTime += Time.deltaTime;
        Services.GameManager.MainCamera.backgroundColor = Color.Lerp(Color.black, _backgroundColor, _colorChangeTime);
        tm.Update();
    }

    public void GameWin(Player winner)
    {
        Services.UIManager.StartBannerScroll(winner);
        if (winner is AIPlayer)
        {
            Services.AudioManager.CreateTempAudio(Services.Clips.Defeat, 0.6f);
        }
        else
        {
            Services.AudioManager.CreateTempAudio(Services.Clips.Victory, 0.3f);
        }
        foreach (Player player in Services.GameManager.Players)
        {
            player.OnGameOver();
        }
        if (demoMode)
        {
            Task restartTask = new Wait(5f);
            restartTask.Then(new ActionTask(Reload));
            Services.GeneralTaskManager.Do(restartTask);
        }
        else if (evolutionMode)
        {
            Services.GameManager.MutateAndSaveStrats(winner);
            Task restartTask = new Wait(1f);
            restartTask.Then(new ActionTask(Reload));
            Services.GeneralTaskManager.Do(restartTask);
        }
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Campaign)
        {
            Task showCampaignMenu = new Wait(0.5f);
            showCampaignMenu.Then(new ParameterizedActionTask<Player>(
                Services.UIManager.ShowCampaignLevelCompleteMenu, winner));
            Services.GameScene.tm.Do(showCampaignMenu);
            if (Services.GameManager.levelSelected != null && !(winner is AIPlayer))
            {
                int progress = 0;
                if (File.Exists(GameOptionsSceneScript.progressFileName))
                {
                    string fileText = File.ReadAllText(GameOptionsSceneScript.progressFileName);
                    int.TryParse(fileText, out progress);
                }
                int levelBeaten = Services.GameManager.levelSelected.campaignLevelNum;
                if (levelBeaten > progress)
                {
                    File.WriteAllText(GameOptionsSceneScript.progressFileName,
                        levelBeaten.ToString());
                }
            }
        }
    }

    public void Replay()
    {
        Task reload = new WaitUnscaled(0.01f);
        reload.Then(new ActionTask(Reload));
        Services.GeneralTaskManager.Do(reload);
    }

    void Reload()
    {
        Services.Scenes.Swap<GameSceneScript>();
    }

    public void ReturnToLevelSelect()
    {
        Task returnToLevelSelect = new WaitUnscaled(0.01f);
        returnToLevelSelect.Then(new ActionTask(LoadLevelSelect));
        Services.GeneralTaskManager.Do(returnToLevelSelect);
    }

    void LoadLevelSelect()
    {
        Services.Scenes.Swap<GameOptionsSceneScript>();
    }

    public void MoveToNextLevel()
    {
        Level nextLevel = Services.MapManager.GetNextLevel();
        if(nextLevel != null)
        {
            Services.GameManager.SetCurrentLevel(nextLevel);
            Replay();
        }
    }

    public void Reset()
    {
        Services.GameManager.Reset(new Reset());
    }

    public void StartGame()
    {
        gameStarted = true;
        TogglePlayerHandLock(false);
        if (Services.MapManager.currentLevel != null &&
            Services.MapManager.currentLevel.tooltips.Length > 0)
            Services.TutorialManager.Init();
    }

    void TogglePlayerHandLock(bool locked)
    {
        for (int i = 0; i < Services.GameManager.Players.Length; i++)
        {
            Services.GameManager.Players[i].ToggleHandLock(locked);
        }
    }

    public void StartGameSequence()
    {
        TaskTree startSequence =
            new TaskTree(new ScrollOffReadyBanners(Services.UIManager.readyBanners),
            new TaskTree(new HandPieceEntry(Services.GameManager.Players[0].hand)),
            new TaskTree(new HandPieceEntry(Services.GameManager.Players[1].hand)),
            new TaskTree(new ActionTask(PlayerHand)));
        startSequence
            .Then(new ActionTask(StartGame));
        Services.GameScene.tm.Do(startSequence);
    }

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0;
        TogglePlayerHandLock(true);
    }

    public void UnpauseGame()
    {
        gamePaused = false;
        Time.timeScale = 1;
        if (gameStarted)
        {
            TogglePlayerHandLock(false);
        }
    }
}

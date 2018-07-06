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
    public bool gameOver { get; private set; }
    public bool gameInProgress
    {
        get { return gameStarted && !gamePaused && !gameOver; }
    }

    private bool showDestructors;

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
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
        {
            float[] handicaps = new float[2];
            handicaps[0] = 1;
            handicaps[1] = 1 + ELOManager.eloData.handicapLevel;
            Debug.Log("handicap level: " + ELOManager.eloData.handicapLevel);
            Debug.Log("total wins: " + ELOManager.eloData.totalWins);
            Debug.Log("win streak: " + ELOManager.eloData.winStreakCount);

            Services.GameManager.SetHandicapValues(handicaps);
        }
        if (evolutionMode)
        {
            Services.GameManager.InitPlayersEvoMode();
        }
        else
        {
            //Services.GameManager.InitPlayers();
            Services.GameManager.InitPlayers(Services.GameManager.useBlueprintHandicapType, Services.GameManager.handicapValue);
        }

        if (Services.GameManager.mode == TitleSceneScript.GameMode.TwoPlayers)
        {
            Services.UIManager.UIForSinglePlayer(false);
        }
        else
        {
            Services.UIManager.UIForSinglePlayer(true);
        }

        if (Services.GameManager.mode == TitleSceneScript.GameMode.Campaign &&
            Services.GameManager.levelSelected.campaignLevelNum == 1)
        {
            showDestructors = false;
        }
        else
        {
            showDestructors = true;
        }

        Services.AudioManager.SetMainTrack(Services.Clips.MenuSong, 0.3f);
        Services.CameraController.SetScreenEdges();
        
        Services.AudioManager.RegisterStartLevelMusic();
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
        if (gameInProgress) Services.GameData.secondsSinceMatchStarted += Time.deltaTime;
    }

    public void GameWin(Player winner)
    {
        gameOver = true;
        Services.UIManager.StartBannerScroll(winner);
        if (winner is AIPlayer)
        {
            if(Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
            {
                ELOManager.OnGameLoss();
            }
            Services.AudioManager.RegisterSoundEffect(Services.Clips.Defeat, 0.6f);
        }
        else
        {
            if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
            {
                ELOManager.OnGameWin();
            }
            Services.AudioManager.RegisterSoundEffect(Services.Clips.Victory, 0.5f);
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
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
        {
            ELOManager.OnGameLoss();
            ReturnToLevelSelect();
        }
        else
        {
            Services.GameData.Reset();
            Services.AudioManager.ResetLevelMusic();
            Task reload = new WaitUnscaled(0.01f);
            reload.Then(new ActionTask(Reload));
            Services.GeneralTaskManager.Do(reload);
        }
    }

    void Reload()
    {
        Services.GameData.Reset();
        Services.AudioManager.ResetLevelMusic();
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
        Services.AudioManager.FadeOutLevelMusic();
        Services.Scenes.Swap<GameOptionsSceneScript>();
    }

    public void MoveToNextLevel()
    {
        Services.AudioManager.FadeOutLevelMusic();
        Level nextLevel = Services.MapManager.GetNextLevel();
        if(nextLevel != null)
        {
            Services.GameManager.SetCurrentLevel(nextLevel);
            Replay();
        }
    }

    public void Reset()
    {
        Services.AudioManager.FadeOutLevelMusicMainMenuCall();
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
            new TaskTree(new ScrollReadyBanners(Services.UIManager.readyBanners, false));
        TaskTree uiEntry =
            new TaskTree(new EmptyTask(),
                new TaskTree(
                    new UIEntryAnimation(Services.UIManager.meters[0], Services.GameManager.Players[0].blueprints, showDestructors)),
                new TaskTree(
                    new UIEntryAnimation(Services.UIManager.meters[1], Services.GameManager.Players[1].blueprints, showDestructors)));
        TaskTree handEntry = 
            new TaskTree(new EmptyTask(),
                new TaskTree(
                new HandPieceEntry(Services.GameManager.Players[0].hand)),
                new TaskTree(
                    new HandPieceEntry(Services.GameManager.Players[1].hand)));
        startSequence
            .Then(uiEntry)
            .Then(handEntry)
            .Then(new ActionTask(StartGame));

        
        Services.GameScene.tm.Do(startSequence);
    }

    public void PauseGame(bool tutorialToolTipActive)
    {
        gamePaused = true;
        TogglePlayerHandLock(true);
        if (tutorialToolTipActive)
        {
            foreach (Player player in Services.GameManager.Players)
            {
                player.PauseProdutcion();
            }
        }
        else
        {

            Time.timeScale = 0;

        }
    }

    public void PauseGame()
    {
        PauseGame(false);
    }

    public void UnpauseGame(bool tutorialToolTipActive)
    {
        if (tutorialToolTipActive)
        {
            foreach (Player player in Services.GameManager.Players)
            {
                player.ResumeProduction();
            }
        }
        else
        {
            Time.timeScale = 1;
        }
        gamePaused = false;
        if (gameStarted)
        {
            TogglePlayerHandLock(false);
        }
    }
    
    public void UnpauseGame()
    {
        UnpauseGame(false);
    }

    public void UIClick()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 0.55f);
    }
    
    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }

    
}

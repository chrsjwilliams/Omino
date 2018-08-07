using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
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

    public Color backgroundColor;

    public bool gameStarted { get; private set; }
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
        Services.UIManager.UIForSinglePlayer(true);

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.Challenge:

                PlayerHandicap[] handicaps = new PlayerHandicap[2];
                handicaps[0].SetEnergyHandicapLevel(1);
                handicaps[0].SetHammerHandicapLevel(1);
                handicaps[0].SetPieceHandicapLevel(1);

                handicaps[1].SetEnergyHandicapLevel(1 + ELOManager.eloData.handicapLevel);
                handicaps[1].SetHammerHandicapLevel(1 + ELOManager.eloData.handicapLevel);
                handicaps[1].SetPieceHandicapLevel( 1 + ELOManager.eloData.handicapLevel);

                Debug.Log("handicap level: " + ELOManager.eloData.handicapLevel);
                Debug.Log("total wins: " + ELOManager.eloData.totalWins);
                Debug.Log("win streak: " + ELOManager.eloData.winStreakCount);

                Services.GameManager.SetHandicapValues(handicaps);

                break;

            case TitleSceneScript.GameMode.DungeonRun:

                handicaps = new PlayerHandicap[2];
                handicaps[0].SetEnergyHandicapLevel(1);
                handicaps[0].SetHammerHandicapLevel(1);
                handicaps[0].SetPieceHandicapLevel(1);

                handicaps[1] = DungeonRunManager.SetHandicap();
                Services.GameManager.SetHandicapValues(handicaps);

                for (int i = 0; i < DungeonRunManager.dungeonRunData.currentTech.Count; i++)
                {
                    Image techBuildingIcon = Services.UIManager.techPowerUpIconArray[0].GetComponentsInChildren<Image>()[i];
                    BuildingType techBuilding = DungeonRunManager.dungeonRunData.currentTech[i];
                    techBuildingIcon.sprite = Services.TechDataLibrary.GetIcon(techBuilding);
                    techBuildingIcon.color = Services.GameManager.Player1ColorScheme[0];
                }

                if (Services.GameManager.onIPhone)
                {
                    Services.UIManager.techPowerUpIconArray[0].GetComponent<RectTransform>().localPosition = new Vector2(-600, 0);
                    Services.UIManager.techPowerUpIconArray[0].GetComponent<RectTransform>().localScale = new Vector2(0.75f, 0.75f);
                }

                break;

            case TitleSceneScript.GameMode.Tutorial:

                handicaps = new PlayerHandicap[2];
                handicaps[0].SetEnergyHandicapLevel(1);
                handicaps[0].SetHammerHandicapLevel(1);
                handicaps[0].SetPieceHandicapLevel(1);
                handicaps[1] = HandicapSystem.tutorialHandicapLevels[Services.MapManager.currentLevel.campaignLevelNum - 1];
                Services.GameManager.SetHandicapValues(handicaps);

                break;

            case TitleSceneScript.GameMode.TwoPlayers:
                Services.UIManager.UIForSinglePlayer(false);
                break;
            case TitleSceneScript.GameMode.HyperVS:
            case TitleSceneScript.GameMode.HyperSOLO:
                handicaps = new PlayerHandicap[2];
                handicaps[0].SetEnergyHandicapLevel(2.5f);
                handicaps[0].SetHammerHandicapLevel(2.5f);
                handicaps[0].SetPieceHandicapLevel(2.5f);
                handicaps[1].SetEnergyHandicapLevel(2.5f);
                handicaps[1].SetHammerHandicapLevel(2.5f);
                handicaps[1].SetPieceHandicapLevel(2.5f);
                
                Services.GameManager.SetHandicapValues(handicaps);
                
                HyperModeManager.StartGame();
                Services.UIManager.UIForSinglePlayer(false);
                break;
            default:

                break;
        }

        if (evolutionMode)
        {
            Services.GameManager.InitPlayersEvoMode();
        }
        else
        {
            Services.GameManager.InitPlayers(Services.GameManager.handicapValue);
        }

        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);

        Services.AudioManager.SetMainTrack(Services.Clips.MenuSong, 0.3f);
        Services.CameraController.SetScreenEdges();
        
        Services.AudioManager.RegisterStartLevelMusic();
    }

    internal override void OnExit()
    {
        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

        Services.GameManager.MainCamera.backgroundColor = backgroundColor;
        Time.timeScale = 1;
        Services.GameEventManager.Clear();
        
        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.HyperSOLO:
            case TitleSceneScript.GameMode.HyperVS:
                HyperModeManager.Exit();
                break;
            default:
                break;
        }
    }

    public void OnMouseDownEvent(MouseDown e)
    {

        Vector3 mouseWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
        OnInputDown(mouseWorldPos);
    }

    public void OnTouchDown(TouchDown e)
    {
        Vector3 touchWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position);
        OnInputDown(touchWorldPos);
    }


    public void OnInputDown(Vector3 position)
    {
        Vector3 effectPosition = new Vector3(position.x, position.y, 5);

        Instantiate(Resources.Load("Prefabs/TouchEffect"), effectPosition, Quaternion.identity);
    }
        // Update is called once per frame
        void Update ()
    {
        _colorChangeTime += Time.deltaTime;
        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.HyperSOLO:
            case TitleSceneScript.GameMode.HyperVS:
                HyperModeManager.Update();
                break;
            default:
                break;
        }

        tm.Update();
        if (gameInProgress) Services.GameData.secondsSinceMatchStarted += Time.deltaTime;
    }

    public void GameWin(Player winner)
    {
        Services.Analytics.MatchEnded();
        gameOver = true;
        Services.GameEventManager.Fire(new GameEndEvent(winner));
        Services.UIManager.StartBannerScroll(winner);

        if (winner is AIPlayer)
        {
            Services.Analytics.PlayerWin(false);
            Services.AudioManager.RegisterSoundEffect(Services.Clips.Defeat, 0.6f);
        }
        else
        {
            Services.Analytics.PlayerWin();
            Services.AudioManager.RegisterSoundEffect(Services.Clips.Victory, 0.5f);
        }

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.Challenge:

                if (winner is AIPlayer) ELOManager.OnGameLoss();
                else ELOManager.OnGameWin();

                break;
            case TitleSceneScript.GameMode.DungeonRun:

                if (winner is AIPlayer) DungeonRunManager.OnGameLoss();
                else DungeonRunManager.OnGameWin();

                break;
            case TitleSceneScript.GameMode.Tutorial:
                if (Services.MapManager.currentLevel.campaignLevelNum == 5 &&
                    Services.TutorialManager.CompletionCheck() &&
                    !TutorialManager.tutorialComplete)
                {
                }
                else
                { 
                    Task showCampaignMenu = new Wait(0.5f);
                    showCampaignMenu.Then(new ParameterizedActionTask<Player>(
                        Services.UIManager.ShowCampaignLevelCompleteMenu, winner));
                    Services.GameScene.tm.Do(showCampaignMenu);
                }
                if (Services.GameManager.levelSelected != null && !(winner is AIPlayer))
                {
                    int progress = 0;
                    if (File.Exists(GameOptionsSceneScript.progressFileName))
                    {
                        string fileText = File.ReadAllText(GameOptionsSceneScript.progressFileName);
                        int.TryParse(fileText, out progress);
                    }

                    

                    int levelBeaten = Services.GameManager.levelSelected.campaignLevelNum;
                    if(levelBeaten == TutorialManager.TUTORIAL_COMPLETE_NUMBER && Services.TutorialManager.CompletionCheck())
                    {
                        Services.GameManager.UnlockMode(TitleSceneScript.GameMode.Practice, true);
                        Services.GameManager.UnlockAllModes();
                    }
                    if (levelBeaten > progress && Services.TutorialManager.CompletionCheck())
                    {
                        File.WriteAllText(GameOptionsSceneScript.progressFileName,
                            levelBeaten.ToString());
                        
                        Services.Analytics.TutorialLevelBeaten(levelBeaten);
                    }
                }
                break;
            case TitleSceneScript.GameMode.TwoPlayers:
                break;
            default:
                break;
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
    }

    public void Replay()
    {
        Services.Analytics.MatchEnded();

        if (Services.GameManager.mode == TitleSceneScript.GameMode.Challenge &&
            !Services.GameScene.gameOver)
        {
            ELOManager.OnGameLoss();
            ReturnToLevelSelect();
        }
        else if(Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            DungeonRunManager.OnGameLoss();
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
        Services.Analytics.MatchEnded();
        Services.GameData.Reset();
        Services.AudioManager.ResetLevelMusic();
        Services.Scenes.Swap<GameSceneScript>();
    }

    public void ReturnToLevelSelect()
    {
        Services.Analytics.MatchEnded();
        Task returnToLevelSelect = new WaitUnscaled(0.01f);
        returnToLevelSelect.Then(new ActionTask(LoadLevelSelect));
        Services.GeneralTaskManager.Do(returnToLevelSelect);
    }

    void LoadLevelSelect()
    {
        Services.AudioManager.FadeOutLevelMusic();
        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.Tutorial:
                Services.Scenes.Swap<TutorialLevelSceneScript>();
                break;
            case TitleSceneScript.GameMode.Challenge:
                Services.Scenes.Swap<EloSceneScript>();
                break;
            case TitleSceneScript.GameMode.DungeonRun:
                if (DungeonRunManager.dungeonRunData.selectingNewTech)
                {
                    Services.Scenes.Swap<TechSelectSceneScript>();
                }
                else
                {
                    Services.Scenes.Swap<DungeonRunSceneScript>();
                }

                break;
            case TitleSceneScript.GameMode.Practice:
                Services.Scenes.Swap<AIDifficultySceneScript>();
                break;
            default:
                Services.Scenes.Swap<MapSelectSceneScript>();
                break;
        }
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
        Services.Analytics.MatchEnded();
        Services.AudioManager.FadeOutLevelMusicMainMenuCall();
        Services.GameManager.Reset(new Reset());
    }

    public void StartGame()
    {
        gameStarted = true;
        TogglePlayerHandLock(false);
        if (Services.MapManager.currentLevel != null &&
            Services.MapManager.currentLevel.tooltips.Length > 0)
        {
            Services.TutorialManager.Init();
            Services.Analytics.PlayedTutorial();
        }

        if (Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            foreach (BuildingType buildingType in DungeonRunManager.dungeonRunData.currentTech)
            {
                TechBuilding techBuilding = DungeonRunManager.GetBuildingFromType(buildingType);
                techBuilding.OnClaimEffect(Services.GameManager.Players[0]);
            }
        }
    }

    public void TogglePlayerHandLock(bool locked)
    {
        for (int i = 0; i < Services.GameManager.Players.Length; i++)
        {
            Services.GameManager.Players[i].ToggleHandLock(locked);
        }
    }

    public void StartGameSequence()
    {
        TaskTree startSequence = new TaskTree(new ScrollReadyBanners(Services.UIManager.readyBanners, false));
        TaskTree uiEntry;
        TaskTree handEntry;
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Tutorial &&
            Services.GameManager.levelSelected.campaignLevelNum == 1)
        {
            showDestructors = false;          
            uiEntry =
                new TaskTree(new EmptyTask(),
                    new TaskTree(
                        new UIEntryAnimation(Services.UIManager.meters[0], Services.GameManager.Players[0].blueprints, showDestructors)));

           handEntry =
           new TaskTree(new EmptyTask(),
               new TaskTree(
               new HandPieceEntry(Services.GameManager.Players[0].hand)));
        }
        else
        {
            showDestructors = true;
            uiEntry =
                new TaskTree(new EmptyTask(),
                    new TaskTree(
                        new UIEntryAnimation(Services.UIManager.meters[0], Services.GameManager.Players[0].blueprints, showDestructors)),
                    new TaskTree(
                        new UIEntryAnimation(Services.UIManager.meters[1], Services.GameManager.Players[1].blueprints, showDestructors)));

            handEntry =
            new TaskTree(new EmptyTask(),
                new TaskTree(
                new HandPieceEntry(Services.GameManager.Players[0].hand)),
                new TaskTree(
                    new HandPieceEntry(Services.GameManager.Players[1].hand)));
        }
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Tutorial)
        {
            startSequence
                .Then(uiEntry)
                .Then(handEntry)
                .Then(new ActionTask(StartGame))
                .Then(new Wait(1))
                .Then(new ActionTask(Services.TutorialManager.DisplaySkipButton));
        }
        else
        {
            startSequence
                .Then(uiEntry)
                .Then(handEntry)
                .Then(new ActionTask(StartGame));
        }
        
        Services.GameScene.tm.Do(startSequence);
    }

    public void PauseGame(bool tutorialToolTipActive, bool paused = true)
    {
        gamePaused = paused;
        TogglePlayerHandLock(true);
        if (tutorialToolTipActive)
        {
            foreach (Player player in Services.GameManager.Players)
            {
                player.PauseProduction();
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

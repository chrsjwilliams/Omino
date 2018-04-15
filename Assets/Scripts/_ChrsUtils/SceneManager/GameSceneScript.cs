using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneScript : Scene<TransitionData>
{
    public const string TILE_MAP_HOLDER = "TileMapHolder";
    public static Transform tileMapHolder;

    private float _colorChangeTime;
    public Transform backgroundImage;

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
    }

    internal override void OnExit()
    {
        Time.timeScale = 1;
    }

	// Update is called once per frame
	void Update ()
    {
        _colorChangeTime += Time.deltaTime;
        Services.GameManager.MainCamera.backgroundColor = Color.Lerp(Color.black, _backgroundColor, _colorChangeTime);
    }

    public void GameWin(Player winner)
    {
        Services.UIManager.StartBannerScroll(winner);
        Services.AudioManager.CreateTempAudio(Services.Clips.Victory, 0.3f);
        foreach(Player player in Services.GameManager.Players)
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

    void Reload()
    {
        Services.Scenes.Swap<GameSceneScript>();
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
        Task scrollBanners = new ScrollOffReadyBanners(Services.UIManager.readyBanners);
        scrollBanners.Then(new ActionTask(StartGame));
        Services.GeneralTaskManager.Do(scrollBanners);
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

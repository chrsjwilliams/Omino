using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneScript : Scene<TransitionData>
{
    public const string TILE_MAP_HOLDER = "TileMapHolder";
    public static Transform tileMapHolder;

    private float _colorChangeTime;

    [SerializeField]
    private bool demoMode;

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
        Services.GameScene = this;
        Services.UIManager = GetComponentInChildren<UIManager>();

        _colorChangeTime = 0f;
        Services.MapManager.GenerateMap();
        Services.GameManager.InitPlayers();
        Services.AudioManager.SetMainTrack(Services.Clips.MainTrackAudio, 0.3f);
        Time.timeScale = 1;
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
        Debug.Log("player " + winner.playerNum + " has won");
        Services.UIManager.StartBannerScroll(winner);
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

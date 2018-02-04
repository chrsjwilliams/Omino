using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneScript : Scene<TransitionData>
{
    public const string TILE_MAP_HOLDER = "TileMapHolder";
    public static Transform tileMapHolder;

    private float _colorChangeTime;

    [SerializeField]
    private Color _backgroundColor;

    public bool gameStarted { get; private set; }

    internal override void OnEnter(TransitionData data)
    {
        tileMapHolder = GameObject.Find(TILE_MAP_HOLDER).transform;
        Services.GameScene = this;
        Services.UIManager = GetComponentInChildren<UIManager>();

        _colorChangeTime = 0f;
        Services.MapManager.GenerateMap();
        Services.GameManager.InitPlayers();
        Services.AudioManager.SetMainTrack(Services.Clips.MainTrackAudio, 0.3f);
    }

    internal override void OnExit()
    {

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
    }

    public void Reset()
    {
        Services.GameManager.Reset(new Reset());
    }

    public void StartGame()
    {
        gameStarted = true;
        for (int i = 0; i < Services.GameManager.Players.Length; i++)
        {
            Services.GameManager.Players[i].ToggleHandLock(false);
        }
    }

    public void StartGameSequence()
    {
        Task scrollBanners = new ScrollOffReadyBanners(Services.UIManager.readyBanners);
        scrollBanners.Then(new ActionTask(StartGame));
        Services.GeneralTaskManager.Do(scrollBanners);
    }
}

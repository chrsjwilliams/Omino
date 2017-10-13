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

    internal override void OnEnter(TransitionData data)
    {
        tileMapHolder = GameObject.Find(TILE_MAP_HOLDER).transform;
        Services.GameScene = this;
        Services.UIManager = GetComponentInChildren<UIManager>();

        _colorChangeTime = 0f;
        Services.MapManager.GenerateMap();
        Services.GameManager.InitPlayers();
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneScript : Scene<TransitionData>
{
    public const string TILE_MAP_HOLDER = "TileMapHolder";
    public static Transform tileMapHolder;

    private float _colorChangeTime;

    private Color _backgroundColor;

    internal override void OnEnter(TransitionData data)
    {
        tileMapHolder = GameObject.Find(TILE_MAP_HOLDER).transform;

        _colorChangeTime = 0f;
        _backgroundColor = new Color(0.449f, 0.820f, 0.867f);
        Services.MapManager.GenerateMap(20, 20);
        //Services.GameManager.InitPlayers();
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

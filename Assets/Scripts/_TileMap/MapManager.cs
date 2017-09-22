using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{ 
	[SerializeField] private int _mapWidth;
    public int MapWidth
    {
        get { return _mapWidth; }
    }

  	[SerializeField] private int _mapLength;
    public int MapLength
    {
        get { return _mapLength; }
    }

	[SerializeField] private Tile[,] _map;
    public Tile[,] Map
    {
        get { return _map; }
    }

    [SerializeField] private static IntVector2 _center;
    public static IntVector2 Center
    {
        get { return _center; }
    }

    public void Init()
    {
        _center = Services.MapManager.CenterIndexOfGrid();
    }

    public void GenerateMap(int width, int length)
    {
        _mapWidth = width;
        _mapLength = length;

        _map = new Tile[_mapWidth, _mapLength];
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapLength; j++)
            {
                Tile tile = Instantiate(Services.Prefabs.Tile, Services.Main.transform)
                    .GetComponent<Tile>();
                tile.transform.parent = GameSceneScript.tileMapHolder;
                
                tile.Init(new Coord(i, j));
                _map[i, j] = tile;
                tile.name = "Tile [X: " + i + ", Y: " + j + "]";
            }
        }
        Map[0, 0].isActive = true;
    }

    public IntVector2 CenterIndexOfGrid()
    {
        return new IntVector2(_mapWidth / 2, _mapLength / 2);
    }

    public void ActivateTile(Tile tile)
    {
        if (ValidateTile(tile))
        {
            tile.isActive = !tile.isActive;
        }
    }

    public Tile GetRandomTile()
    {
		return _map[Random.Range(0, _mapWidth), Random.Range(0, _mapLength) ];
    }

    public Tile GetRandomEmptyTile()
    {
        Tile randomTile = GetRandomTile();

        return randomTile;
    }

    bool ValidateTile(Tile tile)
    {
        int north = tile.coord.y + 1;
        int south = tile.coord.y - 1;
        int west = tile.coord.x + 1;
        int east = tile.coord.x - 1;
        

        if (north > MapLength - 1)
        {
            north = MapLength - 1;
        }

        if (south < 0)
        {
            south = 0;
        }

        if (west > MapLength - 1)
        {
            west = MapLength - 1;
        }

        if (east < 0)
        {
            east = 0;
        }
        bool isValid =  Map[tile.coord.x, north       ].isActive ||
                        Map[tile.coord.x, south       ].isActive ||
                        Map[west        , tile.coord.y].isActive ||
                        Map[east        , tile.coord.y].isActive;

        return isValid;
    }
}

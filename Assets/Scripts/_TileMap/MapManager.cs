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

    public void GenerateMap()
    {
        _map = new Tile[MapWidth, MapWidth];
        for (int i = 0; i < MapWidth; i++)
        {
            for (int j = 0; j < MapLength; j++)
            {
                Tile tile = Instantiate(Services.Prefabs.Tile, Services.Main.transform)
                    .GetComponent<Tile>();
                tile.transform.parent = GameSceneScript.tileMapHolder;
                
                tile.Init(new Coord(i, j), 0);
                _map[i, j] = tile;
                tile.name = "Tile [X: " + i + ", Y: " + j + "]";
            }
        }
        
    }

    public IntVector2 CenterIndexOfGrid()
    {
        return new IntVector2(MapWidth / 2, MapLength / 2);
    }

    public void ActivateBase(Player player, IntVector2 offset)
    {
        Polyomino playerBase = new Polyomino(9, 0, player);
        playerBase.MakePhysicalPiece();
        playerBase.SetBasePosition(offset);

        foreach(Tile tile in playerBase.tiles)
        {
            Map[tile.coord.x + offset.x, tile.coord.y + offset.y].SetOccupyingPiece(playerBase);
        }
    }

    public void ForceActivateTile(Tile tile, Player player)
    {
        tile.ActivateTile(player);
    }

    public void ForceDeactivateTile(Tile tile)
    {
        tile.DeactivateTile();
    }

    public void ActivateTile(Tile tile, Player player)
    {
        if (ValidateTile(tile))
        {
            tile.ActivateTile(player);
        }
    }

    public void DeactivateTile(Tile tile)
    {
        if (ValidateTile(tile))
        {
            tile.DeactivateTile();
        }
    }

    public Tile GetRandomTile()
    {
		return _map[Random.Range(0, MapWidth), Random.Range(0, MapLength) ];
    }

    public Tile GetRandomEmptyTile()
    {
        Tile randomTile = GetRandomTile();

        return randomTile;
    }

    public bool ValidateTile(Tile tile)
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

        bool n = Map[tile.coord.x, north].IsOccupied();
        bool s = Map[tile.coord.x, south].IsOccupied();
        bool w = Map[west, tile.coord.y].IsOccupied();
        bool e = Map[east, tile.coord.y].IsOccupied();



        bool isValid =  n ||
                        s ||
                        w ||
                        w;

        return isValid;
    }
}

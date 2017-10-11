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
                
                tile.Init(new Coord(i, j));
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

    //public void ActivateTile(Tile tile, Player player)
    //{
    //    if (ValidateTile(tile))
    //    {
    //        tile.ActivateTile(player);
    //    }
    //}

    //public void DeactivateTile(Tile tile)
    //{
    //    if (ValidateTile(tile))
    //    {
    //        tile.DeactivateTile();
    //    }
    //}

    public Tile GetRandomTile()
    {
		return _map[Random.Range(0, MapWidth), Random.Range(0, MapLength) ];
    }

    public Tile GetRandomEmptyTile()
    {
        Tile randomTile = GetRandomTile();

        return randomTile;
    }

    public bool IsCoordContainedInMap(Coord coord)
    {
        return coord.x >= 0 && coord.x < MapWidth &&
                coord.y >= 0 && coord.y < MapLength;
    }

    public bool ValidateTile(Tile tile, Player owner)
    {
        foreach(Coord direction in Coord.Directions())
        {
            Coord adjacentCoord = tile.coord.Add(direction);
            if (IsCoordContainedInMap(adjacentCoord))
            {
                if (Map[adjacentCoord.x, adjacentCoord.y].IsOccupied() &&
                    Map[adjacentCoord.x, adjacentCoord.y].occupyingPiece.owner == owner)
                    return true;
            }
        }
        return false;
    }
}

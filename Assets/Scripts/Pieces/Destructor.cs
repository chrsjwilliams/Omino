using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Destructor : Polyomino
{
    private bool isSuper;
    private List<Polyomino> piecesInRange;
    public Destructor(int _units, int _index, Player _player, bool _isSuper) 
        : base(_units, _index, _player)
    {
        isSuper = _isSuper;
        piecesInRange = new List<Polyomino>();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        if (isSuper)
        {
            iconSr.sprite = Services.UIManager.bombFactoryIcon;
        }
        else
        {
            iconSr.sprite = Services.UIManager.destructorIcon;
        }
    }

    public override bool IsPlacementLegal()
    {
        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            if (!Services.MapManager.ConnectedToBase(this, new List<Polyomino>())) return false;
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.owner == owner) return false;
            if (mapTile.IsOccupied() && mapTile.occupyingPiece is Structure) return false;
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.shieldDurationRemaining > 0) return false;

            //if (CanDestroyPieceOn(mapTile)) return false;
        }
        
        return true;
    }

    public bool CanDestroyPieceOn(Tile mapTile)
    {
        if (isSuper) return false;
        else
        {
            if (mapTile.IsOccupied() && !mapTile.occupyingPiece.isFortified) return true;
            return false;
        }
    }

    //public override void CheckForFortification()
    //{
    //    List<Tile> emptyAdjacentTiles = new List<Tile>();

    //    foreach (Tile tile in tiles)
    //    {
    //        foreach (Coord direction in Coord.Directions())
    //        {
    //            Coord adjacentCoord = tile.coord.Add(direction);
    //            if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
    //            {
    //                Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
    //                if (!adjTile.IsOccupied() && !emptyAdjacentTiles.Contains(adjTile))
    //                {
    //                    emptyAdjacentTiles.Add(adjTile);
    //                }
    //            }
    //        }
    //    }

    //    Services.MapManager.CheckForFortification(this, emptyAdjacentTiles, isSuper);
    //}


    protected override void OnPlace()
    {
        base.OnPlace();
        iconSr.enabled = false;
        List<Polyomino> piecesToRemove = GetPiecesInRange();
        if (piecesToRemove.Count > 0)
            Services.AudioManager.CreateTempAudio(Services.Clips.PieceDestroyed, 1);
        for (int i = piecesToRemove.Count - 1; i >= 0; i--)
        {
            piecesToRemove[i].Remove();
        }
        foreach(Tile tile in tiles)
        {
            tile.StartSettlingToNormalPiece();
        }
        MakeFireBurst();
    }

    void MakeFireBurst()
    {
        foreach(Tile tile in tiles)
        {
            GameObject.Instantiate(Services.Prefabs.FireBurst, 
                tile.transform.position, Quaternion.identity);
        }
    }

    protected override void CleanUpUI()
    {
        base.CleanUpUI();
        UnhighlightTargetedPieces();
    }

    void UnhighlightTargetedPieces()
    {
        for (int i = 0; i < piecesInRange.Count; i++)
        {
            if (!piecesInRange[i].dead)
            {
                piecesInRange[i].TurnOffGlow();
            }
        }
    }

    public override void SetLegalityGlowStatus()
    {
        base.SetLegalityGlowStatus();
        UnhighlightTargetedPieces();
        if (IsPlacementLegal())
        {
            piecesInRange = GetPiecesInRange();
            for (int i = 0; i < piecesInRange.Count; i++)
            {
                piecesInRange[i].SetGlow(Color.yellow);
            }
        }
    }

    List<Polyomino> GetPiecesInRange()
    {
        List<Polyomino> enemyPiecesInRange = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.occupyingPiece != null && mapTile.occupyingPiece.owner != owner &&
                !enemyPiecesInRange.Contains(mapTile.occupyingPiece))
            {
                enemyPiecesInRange.Add(mapTile.occupyingPiece);
            }
        }
        if (owner.splashDamage)
        {
            List<Polyomino> adjacentEnemyPieces =
                GetAdjacentPolyominos(Services.GameManager.Players[owner.playerNum % 2]);
            for (int i = 0; i < adjacentEnemyPieces.Count; i++)
            {
                Polyomino enemyPiece = adjacentEnemyPieces[i];
                if (!enemyPiecesInRange.Contains(enemyPiece) && !(enemyPiece is Structure)
                    && enemyPiece.shieldDurationRemaining <= 0)
                {
                    enemyPiecesInRange.Add(enemyPiece);
                }
            }
        }
        return enemyPiecesInRange;
    }
}

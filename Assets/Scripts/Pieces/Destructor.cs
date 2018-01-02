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
        List<Polyomino> piecesToRemove = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.owner != owner)
            {
                if (!piecesToRemove.Contains(mapTile.occupyingPiece))
                    piecesToRemove.Add(mapTile.occupyingPiece);
            }
        }
        if (piecesToRemove.Count > 0)
            Services.AudioManager.CreateTempAudio(Services.Clips.PieceDestroyed, 1);
        for (int i = piecesToRemove.Count - 1; i >= 0; i--)
        {
            piecesToRemove[i].Remove();
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

    protected override void SetLegalityGlowStatus()
    {
        base.SetLegalityGlowStatus();
        UnhighlightTargetedPieces();
        if (IsPlacementLegal())
        {
            List<Polyomino> overlappingEnemyPieces = new List<Polyomino>();
            foreach (Tile tile in tiles)
            {
                Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                if(mapTile.occupyingPiece != null && mapTile.occupyingPiece.owner != owner &&
                    !overlappingEnemyPieces.Contains(mapTile.occupyingPiece))
                {
                    overlappingEnemyPieces.Add(mapTile.occupyingPiece);
                }
            }
            for (int i = 0; i < overlappingEnemyPieces.Count; i++)
            {
                overlappingEnemyPieces[i].SetGlow(Color.yellow);
            }
            piecesInRange = overlappingEnemyPieces;
        }
    }
}

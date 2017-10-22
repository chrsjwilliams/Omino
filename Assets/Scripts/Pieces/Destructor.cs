using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Destructor : Polyomino
{
    private bool isSuper;
    public Destructor(int _units, int _index, Player _player, bool _isSuper) 
        : base(_units, _index, _player)
    {
        isSuper = _isSuper;
    }

    public override bool IsPlacementLegal()
    {
        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            if (!Services.MapManager.ConnectedToBase(this, new List<Polyomino>(), 0)) return false;
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.owner == owner) return false;
        }

        return true;
    }

    protected override void OnPlace()
    {
        base.OnPlace();
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
        for (int i = piecesToRemove.Count - 1; i >= 0; i--)
        {
            piecesToRemove[i].Remove();
        }
    }
}

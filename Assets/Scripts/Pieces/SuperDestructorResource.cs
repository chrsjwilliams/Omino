using UnityEngine;
using System.Collections;

public class SuperDestructorResource : Polyomino
{
    public SuperDestructorResource(int _units, int _index, Player _player) 
        : base(_units, _index, _player)
    {
        
    }

    public SuperDestructorResource(int _units, int _index) : base(_units, _index, null)
    {

    }

    public override void MakePhysicalPiece(bool isViewable)
    {
        base.MakePhysicalPiece(isViewable);
        HideFromInput();
        holder.localScale = Vector3.one;
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.bombFactoryIcon;
    }

    public override void Remove()
    {
        foreach(Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            mapTile.SetOccupyingResource(null);
            tile.OnRemove();
        }

        GameObject.Destroy(holder.gameObject);
    }

    public override void PlaceAtCurrentLocation()
    {
        //place the piece on the board where it's being hovered now
        placed = true;
        OnPlace();
        foreach (Tile tile in tiles)
        {
            Coord tileCoord = tile.coord;
            Services.MapManager.Map[tileCoord.x, tileCoord.y].SetOccupyingResource(this);
        }
    }

    public override void PlaceAtCurrentLocation(bool replace)
    {
        PlaceAtCurrentLocation();
    }

    protected override void OnPlace()
    {
    }
}

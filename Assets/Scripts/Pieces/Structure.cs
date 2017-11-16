using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Structure : Polyomino
{
    protected bool isActivated;
    private Color neutralColor;
    protected static int[,,] structure = new int[2, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //   ###  
            //    # 
            //    #  
            //   
            {
                { 0,0,0,0,0 },
                { 0,1,1,1,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 },
                { 0,0,0,0,0 }
            },
            //  These hashes represent what the piece will look like
            //   #
            //  ###
            //   # 
            {
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,1,1,1,0 },
                { 0,0,1,0,0 },
                { 0,0,0,0,0 }
            }
    };

    public Structure(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
        index = _index;
        units = _units;
        isActivated = false;
        
        placed = true;

        switch (_units)
        {
            case 1:
                holderName = "StructureHolder";
                piece = structure;
                buildingType = BuildingType.STRUCTURE;
                break;
            default:
                break;
        }
    }

    public Structure(int _units, int _index) : base(_units, _index, null)
    {
        index = _index;
        units = _units;
        isActivated = false;
        isFortified = true;
        placed = true;

        switch (_units)
        {
            case 7:
                holderName = "StructureHolder";
                piece = structure;
                buildingType = BuildingType.STRUCTURE;
                break;
            default:
                break;
        }
    }

    public void SetToNeutralColor()
    {
        foreach(Tile tile in tiles)
        {
            tile.GetComponent<SpriteRenderer>().color = neutralColor;
        }
    }

    public virtual void ToggleStructureActivation(Player player)
    {
        if (owner == null) OnClaim(player);
        else
        {
            List<Polyomino> adjacentPieces = GetAdjacentPolyominos();

            //  Not connected to a piece anymore
            if (adjacentPieces.Count < 1)
            {
                OnClaimLost();
                return;
            }

            bool connectedToBase = false;
            foreach (Polyomino piece in adjacentPieces)
            {
                connectedToBase = Services.MapManager.ConnectedToBase(piece, new List<Polyomino>(), 0);
                //  Strcuture belongs to no one 2nd check? why?
                if (connectedToBase && owner == null)
                {
                    OnClaim(piece.owner);
                    return;
                }
                else if (connectedToBase && owner != piece.owner)
                {
                    //  Structure belongs to opponent
                    List<Polyomino> recheck = new List<Polyomino>();
                    foreach (Polyomino newPiece in adjacentPieces)
                    {
                        if (newPiece != piece && !recheck.Contains(newPiece) &&
                           newPiece.owner != piece.owner)
                        {
                            recheck.Add(newPiece);
                        }
                    }

                    foreach (Polyomino opponentPieces in recheck)
                    {
                        //  Is the opponent piece connectd to their base
                        if (Services.MapManager.ConnectedToBase(opponentPieces, new List<Polyomino>(), 0))
                            return;
                    }

                    OnClaim(piece.owner);
                    return;
                }
            }

            if (!connectedToBase)
            {
                OnClaimLost();
            }
        }
    }

    public override void MakePhysicalPiece(bool isViewable)
    {
        base.MakePhysicalPiece(isViewable);
        HideFromInput();
        holder.localScale = Vector3.one;
        neutralColor = tiles[0].GetComponent<SpriteRenderer>().color;
        
    }

    public override void PlaceAtCurrentLocation(bool replace)
    {
        base.PlaceAtCurrentLocation(false);
        foreach (Tile tile in tiles)
        {
            Services.MapManager.Map[tile.coord.x, tile.coord.y].SetOccupyingStructure(this);
        }
    }

    protected override void OnPlace()
    {
        //CreateTimerUI();
        ToggleCostUIStatus(false);
    }

    protected virtual void OnClaim(Player player)
    {
        owner = player;
        ToggleAltColor(true);
    }

    protected virtual void OnClaimLost()
    {
        SetToNeutralColor();
        owner = null;
    }
}
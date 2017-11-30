using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Structure : Polyomino
{
    protected bool isActivated;
    private Color neutralColor;
    protected static int[,,] structure = new int[3, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            // MINING DRILL
            //   ###  
            //    #  
            //   
            {
                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,1,1,0,0 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            },
            //  These hashes represent what the piece will look like
            // ASSEMBLY LINE
            //  #
            //  ###
            //    
            {
                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,1,1,0,0 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            },
            //  These hashes represent what the piece will look like
            // FORTIFIED STEEL
            //  ##
            //  ##
            //    
            {
                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,1,1,0,0 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            }
    };

    public Structure(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
        index = _index;
        units = _units;
        isActivated = false;
        placed = true;
        holderName = "StructureHolder";
    }

    public Structure(int _index) : base(4, _index, null)
    {
        index = _index;
        isActivated = false;
        isFortified = true;
        placed = true;

        holderName = "StructureHolder";
        piece = structure;

    }

    public void SetToNeutralColor()
    {
        foreach(Tile tile in tiles)
        {
            tile.ShiftColor(neutralColor);
        }
    }

    public virtual void ToggleStructureActivation(Player player)
    {
        //  If I am connected to my owner's base, ownership
        //  does not need to be transferred
        List<Polyomino> adjacentPieces = GetAdjacentPolyominos(owner);
        foreach (Polyomino piece in adjacentPieces)
        {
            if (Services.MapManager.ConnectedToBase(piece, new List<Polyomino>()) && owner != null) return;
            else
            {
                if (owner != null)
                    OnClaimLost();
                break;
            }
        }

        adjacentPieces = GetAdjacentPolyominos(player);
        foreach (Polyomino piece in adjacentPieces)
        {
            if (Services.MapManager.ConnectedToBase(piece, new List<Polyomino>()))
            {
                OnClaim(player);
                return;
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

    public virtual void OnClaim(Player player)
    {
        owner = player;
        if(owner == null)
        {
            OnClaimLost();
            return;
        }
        ShiftColor(owner.ColorScheme[0]);
        Services.AudioManager.CreateTempAudio(Services.Clips.StructureClaimed, 1);
        owner.GainOwnership(this);
    }

    public virtual void OnClaimLost()
    {
        SetToNeutralColor();
        owner.LoseOwnership(this);
        owner = null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Structure : Polyomino
{
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
                { 0,1,1,0,0 },
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
                { 0,1,1,0,0 },
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
                { 0,1,1,0,0 },
                { 0,0,0,0,0 }
            }
    };

    public Structure(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
        index = _index;
        units = _units;
        placed = true;

        holderName = "StructureHolder";
    }

    public Structure(int _index) : base(4, _index, null)
    {
        index = _index;
        placed = true;

        holderName = "StructureHolder";
        piece = structure;
    }

    public override void MakePhysicalPiece()
    {
        base.MakePhysicalPiece();
        HideFromInput();
        ScaleHolder(Vector3.one);
        holder.spriteBottom.color = Color.white;
        TurnOffGlow();
        ListenForInput(false);
        foreach(Tile tile in tiles)
        {
            tile.mainSr.enabled = false;
            tile.SetFilledUIFillAmount(0);
            tile.highlightSr.sortingLayerName = "Underlay";
        }
    }

    public override void PlaceAtCurrentLocation(bool replace)
    {
        placed = true;
        OnPlace();

        foreach (Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            mapTile.SetOccupyingPiece(this);
            tile.OnPlace();
        }
        adjacentPieces = new List<Polyomino>();
        SortOverlay();
        SetOverlaySprite();
    }

    public virtual void OnClaim(Player player)
    {
        owner = player;
        SetOverlaySprite();
        Services.AudioManager.PlaySoundEffect(Services.Clips.StructureClaimed, 1);
        owner.GainOwnership(this);
        StructClaimAura effect = GameObject.Instantiate(Services.Prefabs.StructClaimEffect, 
            holder.transform.position + GetCenterpoint(), Quaternion.identity).GetComponent<StructClaimAura>();
        effect.Init(owner);
        adjacentPieces = GetAdjacentPolyominos(owner);
        foreach(Polyomino piece in adjacentPieces)
        {
            if (!piece.adjacentPieces.Contains(this)) piece.adjacentPieces.Add(this);
        }
    }

    public virtual void OnClaimLost()
    {
        owner.LoseOwnership(this);
        owner = null;
        SetOverlaySprite();
        foreach (Polyomino piece in adjacentPieces)
        {
            piece.adjacentPieces.Remove(this);
        }
        adjacentPieces.Clear();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.enabled = true;
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        holder.spriteBottom.sprite = Services.UIManager.structureOverlay;
        if (owner == null)
        {
            holder.spriteBottom.color = new Color(0.6f, 0.6f, 0.6f);
        }
        else
        {
            holder.spriteBottom.color = owner.ColorScheme[0];
        }
    }

    public override void OnInputDown(bool fromPlayTask)
    {
        if (!Services.UIManager.IsTouchMakingTooltipAlready(touchID) &&
            !Services.UIManager.tooltipsDisabled)
        {
            Tooltip tooltipLeft = GameObject.Instantiate(Services.Prefabs.Tooltip,
                Services.UIManager.canvas).GetComponent<Tooltip>();
            tooltipLeft.Init(GetName(), GetDescription(), 0,
                Services.GameManager.MainCamera.WorldToScreenPoint(
                holder.transform.position + GetCenterpoint()));
            Tooltip tooltipRight = GameObject.Instantiate(Services.Prefabs.Tooltip,
                Services.UIManager.canvas).GetComponent<Tooltip>();
            tooltipRight.Init(GetName(), GetDescription(), 180,
                Services.GameManager.MainCamera.WorldToScreenPoint(
                holder.transform.position + GetCenterpoint()));
            tooltips.Add(tooltipLeft);
            tooltips.Add(tooltipRight);
            Services.GameEventManager.Register<TouchUp>(OnTouchUp);
            Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

            Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
            Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);

            Services.UIManager.OnTooltipCreated(touchID);
        }
    }

    public override void OnInputUp(bool forceCancel = false)
    {
        DestroyTooltips();
        touchID = -1;
    }
}
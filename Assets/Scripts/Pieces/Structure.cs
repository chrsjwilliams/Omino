using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Structure : Polyomino
{
    protected bool isActivated;
    private Color neutralColor;
    private ParticleSystem ps;
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
        foreach (Tile tile in tiles)
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

    public override void MakePhysicalPiece()
    {
        base.MakePhysicalPiece();
        HideFromInput();
        holder.localScale = Vector3.one;
        neutralColor = tiles[0].GetComponent<SpriteRenderer>().color;
        TurnOffGlow();
        //SetGlow(Color.white);
        IncrementSortingOrder(15);
        GameObject psObj = GameObject.Instantiate(Services.Prefabs.StructureParticles, holder);
        psObj.transform.position = GetCenterpoint();
        ps = psObj.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = ps.main;
        main.startSizeMultiplier *= Mathf.Sqrt(tiles.Count) / 2;
        if (this is Base)
        {
            Base thisAsBase = this as Base;
            if (thisAsBase.mainBase) SetParticleClaimMode(true);
        }
        ListenForInput();
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
        if (owner == null)
        {
            OnClaimLost();
            return;
        }
        ShiftColor(owner.ColorScheme[0]);
        SetOverlaySprite();
        Services.AudioManager.CreateTempAudio(Services.Clips.StructureClaimed, 1);
        owner.GainOwnership(this);
        SetParticleClaimMode(true);
    }

    public virtual void OnClaimLost()
    {
        SetToNeutralColor();
        owner.LoseOwnership(this);
        owner = null;
        SetOverlaySprite();
        SetParticleClaimMode(false);
    }

    public override void Rotate()
    {
        base.Rotate();
        ps.transform.position = GetCenterpoint();
    }

    void SetParticleClaimMode(bool claimed)
    {
        ParticleSystem.MainModule main = ps.main;
        ParticleSystem.EmissionModule emission = ps.emission;
        if (claimed)
        {
            main.gravityModifierMultiplier += -0.025f;
            main.startColor = (owner.ColorScheme[0] * 0.4f) + (Color.white * 0.6f);
            emission.rateOverTimeMultiplier *= 2f;
            ps.Play();
        }
        else
        {
            main.gravityModifierMultiplier -= 0.025f;
            main.startColor = Color.white;
            emission.rateOverTimeMultiplier /= 2f;
            ps.Stop();
        }
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        spriteOverlay.sprite = Services.UIManager.structureOverlay;
    }

    public override void OnInputDown()
    {
        if (!Services.UIManager.IsTouchMakingTooltipAlready(touchID))
        {
            Tooltip tooltipLeft = GameObject.Instantiate(Services.Prefabs.Tooltip,
                Services.UIManager.canvas).GetComponent<Tooltip>();
            tooltipLeft.Init(GetName(), GetDescription(), 90,
                Services.GameManager.MainCamera.WorldToScreenPoint(
                GetCenterpoint()));
            Tooltip tooltipRight = GameObject.Instantiate(Services.Prefabs.Tooltip,
                Services.UIManager.canvas).GetComponent<Tooltip>();
            tooltipRight.Init(GetName(), GetDescription(), -90,
                Services.GameManager.MainCamera.WorldToScreenPoint(
                GetCenterpoint()));
            tooltips.Add(tooltipLeft);
            tooltips.Add(tooltipRight);
            Services.GameEventManager.Register<TouchUp>(OnTouchUp);
            Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

            Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
            Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);

            Services.UIManager.OnTooltipCreated(touchID);
        }
    }

    public override void OnInputUp()
    {
        DestroyTooltips();
        touchID = -1;
    }
}
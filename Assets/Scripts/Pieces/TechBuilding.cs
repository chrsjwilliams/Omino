using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TechBuilding : Polyomino
{
    public static TechBuilding GetBuildingFromType(BuildingType type)
    {
        TechBuilding structure;
        switch (type)
        {
            case BuildingType.DYNAMO:
                structure = new Dynamo();
                break;
            case BuildingType.SUPPLYBOOST:
                structure = new SupplyBoost();
                break;
            case BuildingType.UPSIZE:
                structure = new Upsize();
                break;
            case BuildingType.SHIELDEDPIECES:
                structure = new ShieldedPieces();
                break;
            case BuildingType.ARMORY:
                structure = new Armory();
                break;
            case BuildingType.FISSION:
                structure = new Fission();
                break;
            case BuildingType.RECYCLING:
                structure = new Recycling();
                break;
            case BuildingType.CROSSSECTION:
                structure = new CrossSection();
                break;
            case BuildingType.ANNEX:
                structure = new Annex();
                break;
            case BuildingType.RETALIATE:
                structure = new Retaliate();
                break;
            default:
                return null;
        }

        return structure;
    }

    public static BuildingType GetBuildingFromString(string tech)
    {
        foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
        {
            if (type.ToString() == tech.ToUpper()) return type;
        }

        return BuildingType.NONE;
    }

    public static BuildingType[] techTypes = new BuildingType[]
    {
            BuildingType.DYNAMO,
            BuildingType.SUPPLYBOOST,
            BuildingType.ARMORY,
            BuildingType.FISSION,
            BuildingType.RECYCLING,
            BuildingType.CROSSSECTION,
            BuildingType.ANNEX,
            BuildingType.UPSIZE,
            BuildingType.SHIELDEDPIECES,
            BuildingType.RETALIATE
    };

    protected static int[,,] techBuilding = new int[3, 5, 5]
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

    public TechBuilding(int _units, int _index, Player _player) : base(_units, _index, _player, false)
    {
        index = _index;
        units = _units;
        placed = true;

        holderName = GetName() + " Holder";
    }

    public TechBuilding(int _index) : base(4, _index, null, false)
    {
        destructible = false;
        index = _index;
        placed = true;

        holderName = GetName() + " Holder";
        piece = techBuilding;
    }

    public override void MakePhysicalPiece()
    {
        base.MakePhysicalPiece();
        HideFromInput();
        ScaleHolder(Vector3.one);
        holder.spriteBottom.color = Color.white;
        TurnOffGlow();
        ListenForInput(true);
        foreach(Tile tile in tiles)
        {
            tile.mainSr.enabled = false;
            tile.SetFilledUIFillAmount(0);
            tile.highlightSr.sortingLayerName = "Underlay";
        }
    }

    public override void PlaceAtCurrentLocation(bool replace, bool isTerrain)
    {
        base.PlaceAtCurrentLocation(false, true);
        //placed = true;
        //OnPlace();

        //foreach (Tile tile in tiles)
        //{
        //    Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
        //    mapTile.SetOccupyingPiece(this);
        //    tile.OnPlace();
        //}
        //adjacentPieces = new List<Polyomino>();
        //SortOverlay();
        //SetOverlaySprite();
    }

    public virtual void OnClaimEffect(Player player){ }
    public virtual void OnLostEffect() { }

    public virtual void OnClaim(Player player)
    {
        owner = player;
        SetOverlaySprite();
        Services.AudioManager.RegisterSoundEffect(Services.Clips.StructureClaimed);
        owner.GainOwnership(this);
        StructClaimAura effect = GameObject.Instantiate(Services.Prefabs.StructClaimEffect, 
            holder.transform.position + GetCenterpoint(), Quaternion.identity).GetComponent<StructClaimAura>();
        
        effect.Init(owner);
        adjacentPieces = GetAdjacentPolyominos(owner);
        foreach(Polyomino piece in adjacentPieces)
        {
            if (!piece.adjacentPieces.Contains(this)) piece.adjacentPieces.Add(this);
        }
        
        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.HyperSOLO:
            case TitleSceneScript.GameMode.HyperVS:
                HyperModeManager.StructureClaimed(owner.ColorScheme[0], holder.transform.position + GetCenterpoint());
                break;
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
        holder.icon.sprite = Services.TechDataLibrary.GetIcon(buildingType);
        if (buildingType != BuildingType.BASE)
        {
            holder.dropShadow.sprite = Services.TechDataLibrary.techDropShadow;
            holder.dropShadow.transform.localPosition = GetCenterpoint();
        }
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        holder.spriteBottom.sprite = Services.UIManager.structureOverlay;
        if (owner == null)
        {
            SetNeutralVisualStatus();
        }
        else
        {
            holder.SetBaseColor(owner.ColorScheme[0]);
        }
    }

    public void SetNeutralVisualStatus()
    {
        holder.SetBaseColor(new Color(0.6f, 0.6f, 0.6f));
    }

    public override void OnInputDown(bool fromPlayTask)
    {
        if (!Services.UIManager.IsTouchMakingTooltipAlready(touchID) &&
            ((!Services.UIManager.tooltipsDisabled 
                && Services.GameManager.mode == TitleSceneScript.GameMode.Tutorial) 
                || !Services.GameScene.gamePaused) &&
            !Services.GameManager.disableUI)
        {
            if(Services.GameManager.mode == TitleSceneScript.GameMode.Tutorial &&
                Services.TutorialManager.currentIndex == Services.TutorialManager.tooltipInfos.Length - 1)
            {
                Services.GameScene.PauseGame();
            }

            if (!(Services.GameManager.Players[0] is AIPlayer))
            {
                Tooltip tooltipLeft = GameObject.Instantiate(Services.Prefabs.Tooltip,
                    Services.UIManager.canvas).GetComponent<Tooltip>();
                tooltipLeft.Init(GetName(), GetDescription(), 0,
                    Services.GameManager.MainCamera.WorldToScreenPoint(
                    holder.transform.position + GetCenterpoint()));
                tooltips.Add(tooltipLeft);
            }
            if (!(Services.GameManager.Players[1] is AIPlayer) && !Services.GameManager.pretendIphone)
            {
                Tooltip tooltipRight = GameObject.Instantiate(Services.Prefabs.Tooltip,
                    Services.UIManager.canvas).GetComponent<Tooltip>();
                tooltipRight.Init(GetName(), GetDescription(), 180,
                    Services.GameManager.MainCamera.WorldToScreenPoint(
                    holder.transform.position + GetCenterpoint()));
                tooltips.Add(tooltipRight);
            }

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
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Tutorial &&
            Services.TutorialManager.currentIndex == Services.TutorialManager.tooltipInfos.Length - 1)
        {
            Services.GameScene.UnpauseGame();
        }
    }

    public override void PathHighlight(float delay)
    {
        holder.StartPathHighlight(delay);
    }
}
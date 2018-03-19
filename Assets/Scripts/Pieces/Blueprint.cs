using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : Polyomino
{
    private const float alphaPrePlacement = 0.8f;
    public int maxRotations { get; protected set; }

    protected static int[,,] factory = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 0,0,0,0,0 },
                { 0,0,1,1,0 },
                { 0,1,1,1,0 },
                { 0,1,1,0,0 },
                { 0,0,0,0,0 }
            }
    };
    protected static int[,,] mine = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 0,0,0,0,0 },
                { 0,1,1,1,1 },
                { 0,1,1,1,1 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            }
    };
    protected static int[,,] bombFactory = new int[1, 5, 5]
    {       //  These hashes represent what the piece will look like
            //    #
            //   ###
            //   ###
            {
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,1,1,1,0 },
                { 0,1,1,1,0 },
                { 0,0,0,0,0 }
            }
    };

public Blueprint(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
    }

    public Blueprint(BuildingType _buildingType, Player _player) : base(0, 0, _player)
    {
        buildingType = _buildingType;
        owner = _player;

        switch (buildingType)
        {
            case BuildingType.FACTORY:
                holderName = "FactoryHolder";
                piece = factory;
                maxRotations = 2;
                break;
            case BuildingType.MINE:
                holderName = "MineHolder";
                piece = mine;
                maxRotations = 2;
                break;
            case BuildingType.BOMBFACTORY:
                holderName = "BombFactoryHolder";
                piece = bombFactory;
                maxRotations = 4;
                break;
            default:
                break;
        }

    }

    protected bool PiecesShareOwner(Tile tile)
    {
        if (tile.occupyingPiece == null) return false;
        if (tile.occupyingPiece is Structure) return false;
        return owner == tile.occupyingPiece.owner;
    }

    public override bool IsPlacementLegal()
    {
        bool isLegal = false;
        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (!PiecesShareOwner(mapTile)) return false;
            if (mapTile.PartOfExistingBlueprint()) return false;
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.connected)
            {
                isLegal = true;
            }
            else
            {
                return false;
            }
        }

        return isLegal;
    }

    public override void MakePhysicalPiece()
    {
        holder = GameObject.Instantiate(Services.Prefabs.PieceHolder,
            Services.GameScene.transform).transform;
        holder.gameObject.name = holderName;
        holderSr = holder.gameObject.GetComponent<SpriteRenderer>();
        spriteOverlay = holder.GetComponentsInChildren<SpriteRenderer>()[2];
        costText = holder.gameObject.GetComponentInChildren<TextMesh>();
        ToggleCostUIStatus(false);
        tooltips = new List<Tooltip>();

        if (piece == null) return;
        //tileRelativeCoords = new Dictionary<Tile, Coord>();

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    Tile newpiece = MonoBehaviour.Instantiate(Services.Prefabs.Tile, holder);

                    Coord myCoord = new Coord(-2 + x, -2 + y);
                    newpiece.Init(myCoord, this);
                    //tileRelativeCoords[newpiece] = myCoord;

                    string pieceName = newpiece.name.Replace("(Clone)", "");
                    newpiece.name = pieceName;
                    //newpiece.SetBaseTileColor(owner, buildingType);
                    newpiece.SetColor(Color.white);
                    newpiece.SetAlpha(alphaPrePlacement);
                    newpiece.IncrementSortingOrder(5);
                    tiles.Add(newpiece);
                }
            }
        }

        //foreach(Tile tile in tiles)
        //{
        //    tile.sr.enabled = false;
        //}
        EnterUnselectedState(false);
        SetSprites();

    }

    protected override void SetTileSprites()
    {
        foreach(Tile tile in tiles)
        {
            tile.sr.sprite = Services.UIManager.blueprintTile;
        }
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = false;
        //iconSr.enabled = true;
        //switch (buildingType)
        //{
        //    case BuildingType.FACTORY:
        //        iconSr.sprite = Services.UIManager.factoryIcon;
        //        break;
        //    case BuildingType.MINE:
        //        iconSr.sprite = Services.UIManager.mineIcon;
        //        break;
        //    case BuildingType.BOMBFACTORY:
        //        iconSr.sprite = Services.UIManager.bombFactoryIcon;
        //        break;
        //    case BuildingType.NONE:
        //        iconSr.enabled = false;
        //        break;
        //    default:
        //        break;
        //}
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        int spriteIndex = 0; 
        switch (buildingType)
        {
            case BuildingType.FACTORY:
                spriteIndex = ((owner.playerNum - 1) * 2) + numRotations % 2;
                spriteOverlay.sprite = 
                    Services.UIManager.factoryOverlays[spriteIndex];
                break;
            case BuildingType.MINE:
                spriteIndex = ((owner.playerNum - 1) * 2) + numRotations % 2;
                spriteOverlay.sprite = 
                    Services.UIManager.mineOverlays[spriteIndex];
                break;
            case BuildingType.BOMBFACTORY:
                spriteIndex = ((owner.playerNum - 1) * 4) + numRotations % 4;
                spriteOverlay.sprite = 
                    Services.UIManager.bombFactoryOverlays[spriteIndex];
                break;
            case BuildingType.NONE:
                spriteOverlay.enabled = false;
                break;
            default:
                break;
        }
        if (!placed)
        {
            spriteOverlay.color = new Color(spriteOverlay.color.r, spriteOverlay.color.g,
                spriteOverlay.color.b, alphaPrePlacement);
            spriteOverlay.enabled = false;
        }
    }

    public override void PlaceAtCurrentLocation()
    {
        placed = true;

        OnPlace();
        foreach (Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.occupyingBlueprint == null)
            {
                mapTile.occupyingPiece.AddOccupyingBlueprint(this);
                mapTile.SetOccupyingBlueprint(this);
            }
        }
        owner.OnPiecePlaced(this);
        SortOverlay();
    }

    public override void Remove()
    {
        List<Polyomino> constituentPieces = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            tile.OnRemove();
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (!constituentPieces.Contains(mapTile.occupyingPiece))
                constituentPieces.Add(mapTile.occupyingPiece);
            mapTile.SetOccupyingBlueprint(null);
        }
        for (int i = 0; i < constituentPieces.Count; i++)
        {
            constituentPieces[i].RemoveOccupyingBlueprint(this);
        }
        owner.OnPieceRemoved(this);
        GameObject.Destroy(holder.gameObject);
        HideFromInput();
    }

    protected override void OnPlace()
    {
        Services.AudioManager.CreateTempAudio(Services.Clips.BlueprintPlaced, 1);
        foreach(Tile tile in tiles)
        {
            if(Services.MapManager.Map[tile.coord.x, tile.coord.y].occupyingPiece.connected)
            {
                TogglePieceConnectedness(true);
                break;
            }
        }
        ListenForInput(true);
        Services.GeneralTaskManager.Do(new BlueprintPlacementTask(this));
        //MakeDustClouds();
        //spriteOverlay.color = spriteOverlay.color = new Color(spriteOverlay.color.r, spriteOverlay.color.g,
        //spriteOverlay.color.b, 1);
        //spriteOverlay.enabled = true;
    }

    public override void OnInputDown(bool fromPlayTask)
    {
        base.OnInputDown(fromPlayTask);
        if (!Services.UIManager.IsTouchMakingTooltipAlready(touchID))
        {
            Vector3 tooltipLocation;
            if (placed || owner.playerNum == 2)
            {
                Tooltip tooltipLeft = GameObject.Instantiate(Services.Prefabs.Tooltip,
                    Services.UIManager.canvas).GetComponent<Tooltip>();
                if (placed)
                {
                    tooltipLocation = Services.GameManager.MainCamera.WorldToScreenPoint(
                        GetCenterpoint());
                }
                else
                {
                    tooltipLocation = 
                        Services.UIManager.blueprintUIZones[1].transform.position;
                }
                tooltipLeft.Init(GetName(), GetDescription(), 90, tooltipLocation, !placed);
                tooltips.Add(tooltipLeft);
            }
            if (placed || owner.playerNum == 1)
            {
                Tooltip tooltipRight = GameObject.Instantiate(Services.Prefabs.Tooltip,
                Services.UIManager.canvas).GetComponent<Tooltip>();
                if (placed)
                {
                    tooltipLocation = Services.GameManager.MainCamera.WorldToScreenPoint(
                        GetCenterpoint());
                }
                else
                {
                    tooltipLocation =
                        Services.UIManager.blueprintUIZones[0].transform.position;
                }
                tooltipRight.Init(GetName(), GetDescription(), -90, tooltipLocation, !placed);
                tooltips.Add(tooltipRight);
            }

            if (!fromPlayTask)
            {
                Services.GameEventManager.Register<TouchUp>(OnTouchUp);
                Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

                Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
                Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
            }

            Services.UIManager.OnTooltipCreated(touchID);
        }
    }

    void PositionTooltips()
    {
        if (tooltips.Count > 0)
        {
            float rot;
            if (owner.playerNum == 2) rot = 90;
            else rot = -90;
            tooltips[0].Reposition(Services.GameManager.MainCamera.WorldToScreenPoint(
                    GetCenterpoint()), rot, true);
        }
    }

    public override void OnInputDrag(Vector3 inputPos)
    {
        base.OnInputDrag(inputPos);
        //DestroyTooltips();
        //PositionTooltips();
    }

    public override void OnInputUp()
    {
        DestroyTooltips();
        if (!placed)
        {
            Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
            Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
            Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);

            Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
            Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
            //DestroyRotationUI();
            SortOnSelection(false);
            if (IsPlacementLegal() && !owner.gameOver)
            {
                PlaceAtCurrentLocation();
                //ListenForInput();
                //IncrementSortingOrder(-20000);
            }
            else
            {
                owner.CancelSelectedBlueprint();
                EnterUnselectedState(false);
            }
        }
    }
}

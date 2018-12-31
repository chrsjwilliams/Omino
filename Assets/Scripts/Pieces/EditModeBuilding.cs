using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModeBuilding : TechBuilding
{
    public Player editModePlayer = Services.GameManager.Players[0];
    public EditModeBuilding(Player player) : base(1)
    {
        editModePlayer = player;
        buildingType = BuildingType.EDITMODE;
        placed = false;
        cost = 0;
    }

    public override void OnClaimEffect(Player player){  }

    public override void OnLostEffect()  {  }

    public override void OnClaim(Player player) {   }

    public override void OnClaimLost() {    }

    public override string GetName()
    {
        return "Tech Building";
    }

    public override string GetDescription()
    {
        return "Location of Tech Building.";
    }

    public override bool IsPlacementLegal(List<Polyomino> adjacentPieces, Coord hypotheticalCoord, bool pretendAttackResource = false)
    {
        List<Coord> hypotheticalTileCoords = new List<Coord>();

        foreach (Tile tile in tiles)
        {
            hypotheticalTileCoords.Add(hypotheticalCoord.Add(tile.relativeCoord));
        }
        foreach (Coord coord in hypotheticalTileCoords)
        {
            if (!Services.MapManager.IsCoordContainedInMap(coord)) return false;

            MapTile mapTile = Services.MapManager.Map[coord.x, coord.y];
            if (mapTile.IsOccupied()) return false;
            
        }
        return true;
    }

    public override void SetLegalityGlowStatus()
    {
        bool isLegal = IsPlacementLegal();
        foreach (MapTile mapTile in previouslyHoveredMapTiles)
        {
            mapTile.SetMapSprite();
        }
        List<MapTile> hoveredMapTiles = new List<MapTile>();
        foreach (Tile tile in tiles)
        {
            tile.ToggleIllegalLocationIcon(false);
            Coord coord = tile.coord;
            if (Services.MapManager.IsCoordContainedInMap(coord))
            {
                MapTile mapTile = Services.MapManager.Map[coord.x, coord.y];
                hoveredMapTiles.Add(mapTile);
            }
        }
        previouslyHoveredMapTiles = hoveredMapTiles;

        foreach (MapTile mapTile in hoveredMapTiles)
        {
            mapTile.SetMapSpriteHovered(isLegal);
        }

        if (isLegal)
        {
            //SetGlow(Services.UIManager.legalGlowColor);
            holder.legalityOverlay.enabled = false;
        }
        else
        {
            //SetGlow(new Color(1, 0.2f, 0.2f));
            if (!(this is Blueprint) && !connected)
            {
                holder.legalityOverlay.enabled = false;
            }
            else
            {
                holder.legalityOverlay.enabled = false;
                List<Tile> illegalTiles = GetIllegalTiles();
                foreach (Tile tile in tiles)
                {
                    //tile.ToggleIllegalLocationIcon(illegalTiles.Contains(tile));
                }
            }
        }
        
    }

    public override void OnInputDown(bool fromPlayTask)
    {
        if (!((EditSceneScript)Services.GameScene).editting) return;
        lastPositions = new Queue<Coord>();
        //ScaleHolder(Vector3.one);
        holder.transform.localPosition = new Vector3(holder.transform.position.x, holder.transform.position.y, -4);
        editModePlayer.OnPieceSelected(this);
        SortOnSelection(true);
        OnInputDrag(holder.transform.position);
        Services.AudioManager.RegisterSoundEffect(Services.Clips.PiecePicked);

        if (!(owner is AIPlayer))
        {
            Services.GameEventManager.Register<TouchUp>(OnTouchUp);
            Services.GameEventManager.Register<TouchMove>(OnTouchMove);
            Services.GameEventManager.Register<TouchDown>(CheckTouchForRotateInput);
            Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

            Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
            Services.GameEventManager.Register<MouseMove>(OnMouseMoveEvent);
            Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
        }
        
    }

    public override void OnInputUp(bool forceCancel = false)
    {
        DestroyTooltips();
        touchID = -1;
        bool piecePlaced;
        if (!placed)
        {
            if (IsPlacementLegal())
            {
                PlaceAtCurrentLocation();
                editModePlayer.OnPiecePlaced(this, null);
                piecePlaced = true;
                placed = true;
            }
            else
            {
                editModePlayer.CancelSelectedPiece();
                //EnterUnselectedState(false);
                piecePlaced = false;
            }
            CleanUpUI(piecePlaced);
        }
        
        SortOnSelection(false);
        holder.transform.localPosition = new Vector3(holder.transform.position.x, holder.transform.position.y, 0);
        if (!(owner is AIPlayer))
        {
            Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
            Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
            Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);
            Services.GameEventManager.Register<TouchDown>(OnTouchDown);

            Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
            Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
            Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        }
    }

    public override void OnInputDrag(Vector3 inputPos)
    {
        if (placed && editModePlayer.selectedPiece != this) return;

        if(placed)
        {
            foreach(Tile tile in tiles)
            {
                Services.MapManager.Map[tile.coord.x, tile.coord.y].SetOccupyingPiece(null);
            }
        }
        placed = false;

            Vector3 screenInputPos =
                Services.GameManager.MainCamera.WorldToScreenPoint(inputPos);
            Vector3 screenOffset;
            float mapEdgeScreenHeight = Services.CameraController.GetMapEdgeScreenHeight();

        screenOffset = baseDragOffset +
            (((mapEdgeScreenHeight - (Screen.height / 2)
                - baseDragOffset.y) / (Screen.height / 2))
            * screenInputPos.y * Vector3.up);
          
            Vector3 offsetInputPos = Services.GameManager.MainCamera.ScreenToWorldPoint(
                screenInputPos);
           
            Coord roundedInputCoord = new Coord(
                Mathf.RoundToInt(offsetInputPos.x),
                Mathf.RoundToInt(offsetInputPos.y));
            Coord snappedCoord = roundedInputCoord;
            if (!IsPlacementLegal(roundedInputCoord, true))
            {
                List<Coord> nearbyCoords = new List<Coord>();
                foreach (Coord direction in Coord.Directions())
                {
                    Coord nearbyCoord = roundedInputCoord.Add(direction);
                    if (nearbyCoords.Count == 0) nearbyCoords.Add(nearbyCoord);
                    else
                    {
                        bool added = false;
                        for (int i = 0; i < nearbyCoords.Count; i++)
                        {
                            if (Vector2.Distance(new Vector2(nearbyCoord.x, nearbyCoord.y),
                                offsetInputPos) <
                                Vector2.Distance(new Vector2(nearbyCoords[i].x, nearbyCoords[i].y),
                                offsetInputPos))
                            {
                                nearbyCoords.Insert(i, nearbyCoord);
                                added = true;
                                break;
                            }
                        }
                        if (!added) nearbyCoords.Add(nearbyCoord);
                    }
                }
                for (int i = 0; i < nearbyCoords.Count; i++)
                {
                    Coord nearbyCoord = nearbyCoords[i];
                    if (IsPlacementLegal(nearbyCoord, true))
                    {
                        snappedCoord = nearbyCoord;
                        break;
                    }
                }
            }
            SetTileCoords(snappedCoord);
            Reposition(new Vector3(
                snappedCoord.x,
                snappedCoord.y,
                holder.transform.position.z));
            QueuePosition(snappedCoord);
        

        if (!Services.GameManager.disableUI) SetLegalityGlowStatus();
    }
}

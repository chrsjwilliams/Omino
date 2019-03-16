using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModePlayer : Player {

    public EditModeBuilding editModeBuilding;
    
    // Use this for initialization
    void Start () {
		
	}

    public override void Init(int playerNum_)
    {
        boardPieces = new List<Polyomino>();
        hand = new List<Polyomino>();
        blueprints = new List<Blueprint>();

        playerNum = playerNum_;

        colorScheme = Services.GameManager.colorSchemes[playerNum - 1];
        if (Services.GameManager.loadedLevel)
        {
            if (playerNum == 1)
            {
                homeBasePos = Services.GameManager.levelSelected.p1HomeBasePos;
            }
            else
            {
                homeBasePos = Services.GameManager.levelSelected.p2HomeBasePos;
            }
        }
        else
        {
            if (playerNum == 1) homeBasePos = new Coord(1, 1);
            else
            {
                homeBasePos = new Coord(
                    Services.MapManager.MapWidth - 2,
                    Services.MapManager.MapHeight - 2);
            }
        }
        Services.MapManager.CreateMainBase(this, homeBasePos);
        startingHandSize = 0;

        normalDrawRate = 0;

        resourceGainRate = 0;
        startingResources = 0;
        destructorDrawRate = 0;

        editModeBuilding = new EditModeBuilding(this);
        AddEditModeBuilding(editModeBuilding);
        editModeBuilding.holder.gameObject.SetActive(false);
        ((EditSceneScript)Services.GameScene).editModeBuilding = editModeBuilding;

        //  Erase Function
        //  Destructible Function
        //  Indestructible Function

        //  How do I save?
        //  How do I bring up the keyboard?
    }

    public override void DrawPiece(bool onlyDestructors)
    {
    }

    public override void DrawPiece(Vector3 startPos, bool onlyDestructors)
    {

    }

    public override void OnPiecePlaced(Polyomino piece, List<Polyomino> subpieces)
    {
        
        Services.GameEventManager.Fire(new PiecePlaced(piece));
        BuildingType blueprintType = piece.buildingType;

        if (piece is EditModeBuilding)
        {
            AddEditModeBuilding(System.Activator.CreateInstance(
                piece.GetType(), new Object[] { this }) as EditModeBuilding);
        }
        else if (piece is Blueprint)
        {
            AddBluePrint(System.Activator.CreateInstance(
                piece.GetType(), new Object[] { this }) as Blueprint);
        }

        selectedPiece = null;
        OrganizeHand(hand);
        piece.SetGlowState(false);


    }

    public override void OnPieceSelected(Polyomino piece)
    {
        if (selectedPiece != null) CancelSelectedPiece();
        selectedPiece = piece;
  
        if (piece is EditModeBuilding)
        {
            ((EditSceneScript)Services.GameScene).TurnOffAllButtons();
        }

        OrganizeHand(hand);
    }

    public override void CancelSelectedPiece()
    {
        if (selectedPiece == null) return;
        if (selectedPiece.cost > resources)
        {
            //Services.UIManager.UIMeters[playerNum - 1].FailedPlayFromLackOfResources(
            //    selectedPiece.cost - resources);
            Services.UIManager.UIMeters[playerNum - 1]
                .FailedPlayFromLackOfResources(false);
        }

        bool overlappingConnectedOpponentTile = false;
        foreach (Tile tile in selectedPiece.tiles)
        {
            if (Services.MapManager.IsCoordContainedInMap(tile.coord))
            {
                MapTile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                if (mapTile.IsOccupied() && mapTile.occupyingPiece.owner != null &&
                    mapTile.occupyingPiece.owner != this && mapTile.occupyingPiece.connected)
                    overlappingConnectedOpponentTile = true;
            }
        }

        if (overlappingConnectedOpponentTile && selectedPiece.cost > attackResources)
        {
            //Services.UIManager.UIMeters[playerNum - 1].FailedPlayFromLackOfResources(
            //    selectedPiece.cost - attackResources, true);
            Services.UIManager.UIMeters[playerNum - 1]
                .FailedPlayFromLackOfResources(true);
        }

        if (selectedPiece is EditModeBuilding)
        {
            selectedPiece.Reposition(GetBlueprintPosition(selectedPiece));
        }
        
        selectedPiece.SetGlowState(false);
        selectedPiece = null;
        OrganizeHand(hand);
    }

    public void AddEditModeBuilding(EditModeBuilding techBuilding)
    {
        techBuilding.MakePhysicalPiece();
        techBuilding.Reposition(GetBlueprintPosition(techBuilding));
    }

    public override Vector3 GetBlueprintPosition(Polyomino piece)
    {
        Vector3 screenPos = Vector3.zero;
        screenPos = Services.UIManager.UIMeters[playerNum - 1].mineBlueprintLocation.position;

        Vector3 rawWorldPos = screenPos;// Services.GameManager.MainCamera.ScreenToWorldPoint(screenPos);
        Vector3 centerpoint = piece.GetCenterpoint() * Polyomino.UnselectedScale.x;
        rawWorldPos -= centerpoint;
        return new Vector3(rawWorldPos.x, rawWorldPos.y, 0);
    }

    // Update is called once per frame
    void Update () {
		
	}
}

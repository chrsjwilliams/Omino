using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModePlayer : Player {

    public EditModeBuilding editModeBuilding;
    public readonly int TOTAL_NUM_EDIT_BUILDINGS = Services.TechDataLibrary.dataArray.Length - 2;
    private int numUsedEditBuildings;
    public int numPlacedEditBuildings
    {
        get { return numUsedEditBuildings; }
        set
        {
            
            numUsedEditBuildings = value;
            if(numUsedEditBuildings > TOTAL_NUM_EDIT_BUILDINGS)
            {
                numUsedEditBuildings = TOTAL_NUM_EDIT_BUILDINGS;
            }
            else if(numUsedEditBuildings < 0)
            {
                numUsedEditBuildings = 0;
            }
            ((EditSceneScript)Services.GameScene).usedTechCounter.text = numUsedEditBuildings + " / " + TOTAL_NUM_EDIT_BUILDINGS;
        }
    }


    public override void Init(int playerNum_)
    {
        numPlacedEditBuildings = ((EditSceneScript)Services.GameScene).hasExpansions ?
                                    Services.MapManager.structuresOnMap.Count - 2 : Services.MapManager.structuresOnMap.Count;
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

        Services.GameEventManager.Register<EditModeBuildingRemoved>(OnEditModeBuildingRemoved);

        editModeBuilding = new EditModeBuilding(this);

        AddEditModeBuilding(editModeBuilding);
        editModeBuilding.holder.gameObject.SetActive(false);
        
        ((EditSceneScript)Services.GameScene).editModeBuilding = editModeBuilding;

        Generator mine = new Generator(this);
        AddBluePrint(mine);
        mine.Lock();
        mine.holder.gameObject.SetActive(false);
    }

    public void HideEditModeBuilding()
    {
        editModeBuilding.holder.gameObject.SetActive(false);

    }

    public override void DrawPiece(bool onlyDestructors)
    {
    }

    public override void DrawPiece(Vector3 startPos, bool onlyDestructors)
    {

    }

    //  TODO: Hold to delete

    public void OnEditModeBuildingRemoved(EditModeBuildingRemoved e)
    {
        numPlacedEditBuildings--;
        UpdateEditModeBuildingUI(editModeBuilding);
    }

    public override void OnPiecePlaced(Polyomino piece, List<Polyomino> subpieces)
    {
        Services.GameEventManager.Fire(new PiecePlaced(piece));
        BuildingType blueprintType = piece.buildingType;

        if (piece is EditModeBuilding)
        {
            numPlacedEditBuildings++;
            UpdateEditModeBuildingUI(piece);
        }

        selectedPiece = null;
        OrganizeHand(hand);
        piece.SetGlowState(false);
    }

    public void UpdateEditModeBuildingUI(Polyomino piece)
    {
        if (numPlacedEditBuildings < TOTAL_NUM_EDIT_BUILDINGS)
        {
            AddEditModeBuilding(System.Activator.CreateInstance(
                piece.GetType(), new Object[] { this }) as EditModeBuilding);
        }
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
        if (selectedPiece is EditModeBuilding && !((EditModeBuilding)selectedPiece).wasPlaced)
        {
            selectedPiece.Reposition(GetBlueprintPosition(blueprints[0]));
            selectedPiece.SetGlowState(false);
        }
        else if(selectedPiece is EditModeBuilding && ((EditModeBuilding)selectedPiece).wasPlaced)
        {
            numPlacedEditBuildings--;
        }
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

        Vector3 rawWorldPos = screenPos;
        Vector3 centerpoint = piece.GetCenterpoint() * Polyomino.UnselectedScale.x;
        rawWorldPos -= centerpoint;
        return new Vector3(rawWorldPos.x, rawWorldPos.y, 0);
    }
}

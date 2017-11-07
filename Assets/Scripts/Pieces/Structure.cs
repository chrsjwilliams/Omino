using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : Polyomino
{
    protected bool isActivated;
    private TaskManager tm = new TaskManager();
    private Color nuetralColor;
    protected static int[,,] structure = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  ###
            //  ###
            //  ###
            {
                { 0,0,0,0,0 },
                { 0,1,1,0,0 },
                { 0,1,1,1,0 },
                { 0,0,1,1,0 },
                { 0,0,0,0,0 }
            }
    };

    public Structure(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
        index = _index;
        units = _units;
        isActivated = false;
        isFortified = true;
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

    public void SetToNuetralColor()
    {
        foreach(Tile tile in tiles)
        {
            tile.GetComponent<SpriteRenderer>().color = nuetralColor;
        }
    }

    public virtual void ActivateStructureCheck()
    {
        List<Polyomino> adjacentPiecies = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
                {
                    Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.IsOccupied() && !adjacentPiecies.Contains(adjTile.occupyingPiece) &&
                        adjTile.occupyingPiece != this)
                    {
                        adjacentPiecies.Add(adjTile.occupyingPiece);
                    }
                }
            }
        }

        if(adjacentPiecies.Count < 1)
        {
            SetToNuetralColor();
            isActivated = false;
            owner = null;
            return;
        }

        bool connectedToBase = false;
        
        foreach (Polyomino piece in adjacentPiecies)
        {
            connectedToBase = Services.MapManager.ConnectedToBase(piece, new List<Polyomino>(), 0);
            if (connectedToBase && owner == null)
            {
                isActivated = true;
                owner = piece.owner;
                ToggleAltColor(true);
                return;
            }
            else if(connectedToBase && owner != piece.owner)
            {
                List<Polyomino> recheck = new List<Polyomino>();
                foreach(Polyomino newPiece in adjacentPiecies)
                {
                    if(newPiece != piece && !recheck.Contains(newPiece) &&
                       newPiece.owner != piece.owner)
                    {
                        recheck.Add(newPiece);
                    }
                }

                foreach (Polyomino opponentPieces in recheck)
                {
                    if (Services.MapManager.ConnectedToBase(opponentPieces, new List<Polyomino>(), 0))
                        return;
                }

                isActivated = true;
                owner = piece.owner;
                ToggleAltColor(true);
                return;
            }
        }

        if (!connectedToBase)
        {
            SetToNuetralColor();
            isActivated = false;
            owner = null;
        }
    }

    public override void MakePhysicalPiece(bool isViewable)
    {
        base.MakePhysicalPiece(isViewable);
        HideFromInput();
        holder.localScale = Vector3.one;
        nuetralColor = tiles[0].GetComponent<SpriteRenderer>().color;
    }

    protected override void OnPlace()
    {
        //CreateTimerUI();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move 
{
    public float score;
    public const int MAX_SCORE = 100;
    public const int MAX_ROTATIONS = 3;

    public Dictionary<BuildingType, bool> blueprintsAvailable = new Dictionary<BuildingType, bool>();

    public Move blueprintPlay { get; private set; }
    public bool canMakeBlueprint;

    public Dictionary<Tile, Coord> relativeCoords { get; private set; }
    public Polyomino piece { get; private set; }
    public Coord targetCoord { get; private set; }
    public int rotations { get; private set; }

    private bool canMakeMine = false;
    private bool canMakeBombFactory = false;
    private bool canMakeFactory = false;

    private int coordCountBuffer = 200;
    private int movesTriedBuffer = 50;

    public Move(Polyomino _piece, Coord _targetCoord, int _rotations)
    {
        canMakeBlueprint = false;
        piece = _piece;
        relativeCoords = piece.tileRelativeCoords;
        targetCoord = _targetCoord;
        rotations = _rotations;
        blueprintPlay = null;
    }

    public Move (AIPlayer.MoveData moveData, float winWeight, float structureWeight)
    {
        canMakeBlueprint = false;
        piece = moveData.piece;
        relativeCoords = moveData.piece.tileRelativeCoords;
        targetCoord = moveData.targetCoord;
        rotations = moveData.rotations;
        

        if (moveData.blueprintMap.blueprint != null)
        {
            canMakeBlueprint = true;
            blueprintPlay = new Move(   moveData.blueprintMap.blueprint, 
                                        moveData.blueprintMap.targetCoord, 
                                        moveData.blueprintMap.rotations);
        }
        score = CalculateScore(winWeight, structureWeight, canMakeBlueprint);
    }


    public void PopulateDictionary()
    {
        blueprintsAvailable.Add(BuildingType.MINE, false);
        blueprintsAvailable.Add(BuildingType.FACTORY, false);
        blueprintsAvailable.Add(BuildingType.BOMBFACTORY, false);
    }

    public float CalculateScore(float winWeight, float structWeight, bool canMakeBluePrint)
    {
        //  Temporary
        float blueprintWeight = 0;
        if (canMakeBlueprint)
            blueprintWeight = 10000;

        return WinAndStructScore(winWeight, structWeight) + blueprintWeight;
    }
    
    private float WinAndStructScore(float winWeight, float structWeight)
    {
        float structDist = float.MaxValue;
        float pieceDistFromTarget = float.MaxValue;
        foreach (Tile tile in piece.tiles)
        {
            Coord tileRelCoord = relativeCoords[tile];
            float tileDistFromTarget = ((AIPlayer)piece.owner).primaryTarget.Distance(
                                                    tileRelCoord.Add(targetCoord));

            foreach (Coord coord in Services.MapManager.structureCoords)
            {
                if (Services.MapManager.Map[coord.x, coord.y].occupyingStructure.owner == null) {
                    float testDistance = coord.Distance(tileRelCoord.Add(targetCoord));

                    if (testDistance < structDist)
                    {
                        structDist = testDistance;
                    }
                }
            }

            if (tileDistFromTarget < pieceDistFromTarget)
            {
                pieceDistFromTarget = tileDistFromTarget;
            }
        }

        float winScore;
        float structScore;
        if (pieceDistFromTarget == 0)
        {
            winScore = winWeight * MAX_SCORE;
        }
        else
        {
            winScore = winWeight / pieceDistFromTarget;
        }
        if(structDist == 0)
        {
            structScore = structWeight * MAX_SCORE;
        }
        else
        {
            structScore = structWeight / structDist;
        }

        return winScore + structScore;
    }

    public void ExecuteMove()
    {

        
        Task playTask;
        if (blueprintPlay == null)
        {
            playTask = new PlayTask(this);
        }
        else
        {
            playTask = new PlayTask(this);
            playTask.Then(new ActionTask(blueprintPlay.ExecuteMove));
        }
        Services.GeneralTaskManager.Do(playTask);
    }
}

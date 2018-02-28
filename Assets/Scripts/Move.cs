using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move 
{
    public float score;
    public const int MAX_SCORE = 100;
    public const int MAX_ROTATIONS = 3;
    private float finalWinScore;
    private float finalStructScore;
    private float finalBlueprintScore;

    public Move blueprintMove { get; private set; }

    public Dictionary<Tile, Coord> relativeCoords { get; private set; }
    public Polyomino piece { get; private set; }
    public Coord targetCoord { get; private set; }
    public int rotations { get; private set; }

    private List<BlueprintMap> possibleBlueprintMoves;

    public Move(Polyomino _piece, Coord _targetCoord, int _rotations, List<BlueprintMap> _possibleBlueprintMoves, float winWeight, 
        float structureWeight, float mineWeight, float factoryWeight, float bombFactoryWeight)
    {
        piece = _piece;
        relativeCoords = piece.tileRelativeCoords;
        targetCoord = _targetCoord;
        rotations = _rotations;
        possibleBlueprintMoves = _possibleBlueprintMoves;
        blueprintMove = null;
        score = CalculateScore(winWeight, structureWeight, mineWeight, factoryWeight, bombFactoryWeight);
    }

    public Move(Blueprint blueprint, Coord _targetCoord, int _rotations)
    {
        piece = blueprint;
        targetCoord = _targetCoord;
        rotations = _rotations;
    }

    public float CalculateScore(float winWeight, float structWeight, float mineWeight, float factoryWeight, float bombFactoryWeight)
    {
        HashSet<Coord> pieceCoords = new HashSet<Coord>();
        foreach(Tile tile in piece.tiles)
        {
            pieceCoords.Add(relativeCoords[tile].Add(targetCoord));
        }

        Move mineMove = null;
        Move factoryMove = null;
        Move bombFactoryMove = null;

        foreach (BlueprintMap blueprintMap in possibleBlueprintMoves)
        {
            if (pieceCoords.IsSupersetOf(blueprintMap.missingCoords))
            {
                if (blueprintMap.blueprint is Mine)
                {
                    mineMove = new Move(blueprintMap.blueprint, blueprintMap.targetCoord, blueprintMap.rotations);
                }
                else if(blueprintMap.blueprint is Factory)
                {
                    factoryMove = new Move(blueprintMap.blueprint, blueprintMap.targetCoord, blueprintMap.rotations);
                }
                else if (blueprintMap.blueprint is BombFactory)
                {
                    bombFactoryMove = new Move(blueprintMap.blueprint, blueprintMap.targetCoord, blueprintMap.rotations);
                }
                if (mineMove != null && factoryMove != null && bombFactoryMove != null) break;
            }

            #region Smart Blueprint Placement Test
            /*
            //  find which missing peice I'm a superset of
            moveBlueprintMap = blueprintMap;
            int numFilledCoords = moveBlueprintMap.coords.Intersect(pieceCoords).Count();
            if(numFilledCoords > moveBlueprintMap.numCoordsFilled)
            {
                moveBlueprintMap = blueprintMap;
                //   we can do other things here that distinguish between blueprints
                if (numFilledCoords == piece.tiles.Count) break;
            }
            */
            #endregion

        }
        

        return WinAndStructScore(winWeight, structWeight) + BlueprintScore(mineMove, factoryMove, bombFactoryMove, mineWeight,
            factoryWeight, bombFactoryWeight);
    }

    private float BlueprintScore(Move mineMove, Move factoryMove, Move bombFactoryMove, float mineWeight, float factoryWeight,
        float bombFactoryWeight)
    {
        float blueprintScore = 0;
        if (mineMove != null)
        {
            blueprintScore = mineWeight;
            blueprintMove = mineMove;
        }
        if (factoryMove != null && factoryWeight > blueprintScore)
        {
            blueprintScore = factoryWeight;
            blueprintMove = factoryMove;
        }
        if (bombFactoryMove != null && bombFactoryWeight > blueprintScore)
        {
            blueprintScore = bombFactoryWeight;
            blueprintMove = bombFactoryMove;
        }
        finalBlueprintScore = blueprintScore;
        return blueprintScore;

    }
    
    private float WinAndStructScore(float winWeight, float structWeight)
    {
        float structDist = float.MaxValue;
        float pieceDistFromTarget = float.MaxValue;
        foreach (Tile tile in piece.tiles)
        {
            Coord tileRelCoord = relativeCoords[tile];
            foreach (Coord coord in ((AIPlayer)piece.owner).primaryTargets)
            {
                float tileDistFromTarget = coord.Distance(tileRelCoord.Add(targetCoord));

                if (tileDistFromTarget < pieceDistFromTarget)
                {
                    pieceDistFromTarget = tileDistFromTarget;
                }
            }

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

        }

        float winScore;
        float structScore;
        if (pieceDistFromTarget == 0)
        {
            winScore = winWeight;
        }
        else
        {
            winScore = winWeight / Mathf.Pow(pieceDistFromTarget, 2);
        }
        if(structDist == 0)
        {
            structScore = structWeight;
        }
        else
        {
            structScore = structWeight / Mathf.Pow(structDist, 2);
        }
        finalWinScore = winScore;
        finalStructScore = structScore;
        return winScore + structScore;
    }

    public void ExecuteMove()
    {
        Task playTask;
        if (blueprintMove == null)
        {
            playTask = new PlayTask(this);
        }
        else
        {
            playTask = new PlayTask(this);
            playTask.Then(new ActionTask(blueprintMove.ExecuteMove));
        }
        Services.GeneralTaskManager.Do(playTask);
        //if(!(piece is Blueprint))
        //{
        //    Debug.Log("player " + piece.owner.playerNum + " playing move with score:" + score +
        //        ", winScore: " + finalWinScore + ", structScore: " + finalStructScore +
        //        ", blueprintScore: " + finalBlueprintScore);
        //}
    }
}

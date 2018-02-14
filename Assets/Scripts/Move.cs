using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public float score;
    public const int MAX_SCORE = 100;
    public const int MAX_ROTATIONS = 3;

    public Dictionary<Tile, Coord> relativeCoords { get; private set; }
    public Polyomino piece { get; private set; }
    public Coord targetCoord { get; private set; }
    public int rotations { get; private set; }

    public Move(Polyomino _piece, Coord _targetCoord, int _rotations, float winWeight, float structureWeight)
    {
        piece = _piece;
        relativeCoords = piece.tileRelativeCoords;
        targetCoord = _targetCoord;
        rotations = _rotations;
        score = CalculateScore(winWeight, structureWeight);
    }

    public Move(Polyomino _piece, Vector3 _targetCoord, int _rotations)
    {
        piece = _piece;
        targetCoord = new Coord((int)_targetCoord.x, (int)_targetCoord.y);
        rotations = _rotations;
    }

    public float CalculateScore(float winWeight, float structWeight)
    {
        return WinAndStructScore(winWeight, structWeight);
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
        Task playTask = new PlayTask(piece, piece.holder.transform.position, targetCoord.WorldPos(), rotations);
        Services.GeneralTaskManager.Do(playTask);
    }
}

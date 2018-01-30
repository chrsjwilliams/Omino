using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public bool executed { get; private set; }
    public const int MAX_ROTATIONS = 3;


    public Dictionary<Tile, Coord> relativeCoords { get; private set; }
    public Polyomino piece { get; private set; }
    public Coord targetCoord { get; private set; }
    public int rotations { get; private set; }

    public Move(Polyomino _piece, Coord _targetCoord, int _rotations)
    {
        executed = false;
        piece = _piece;
        relativeCoords = piece.tileRelativeCoords;
        targetCoord = _targetCoord;
        rotations = _rotations;
    }

    public Move(Polyomino _piece, Vector3 _targetCoord, int _rotations)
    {
        executed = false;
        piece = _piece;
        targetCoord = new Coord((int)_targetCoord.x, (int)_targetCoord.y);
        rotations = _rotations;
    }

    public void ExecuteMove()
    {
        //for (int i = 0; i < rotations; i++)
        // piece.Rotate();
        Debug.Log("Target Coord: " + targetCoord);
        Task playTask = new PlayTask(piece, piece.holder.transform.position, targetCoord.ScreenPos(), rotations);
        playTask.Then(new ActionTask(piece.owner.SetHandStatus));
        Services.GeneralTaskManager.Do(playTask);
        executed = true;
    }
}

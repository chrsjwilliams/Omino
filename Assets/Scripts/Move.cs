using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public const int MAX_ROTATIONS = 3;

    public Polyomino piece { get; private set; }
    public Coord targetCoord { get; private set; }
    public int rotations { get; private set; }

    public Move(Polyomino _piece, Coord _targetCoord, int _rotations)
    {
        piece = _piece;
        targetCoord = _targetCoord;
        rotations = _rotations % MAX_ROTATIONS;
    }

    public Move(Polyomino _piece, Vector3 _targetCoord, int _rotations)
    {
        piece = _piece;
        targetCoord = new Coord((int)_targetCoord.x, (int)_targetCoord.y);
        rotations = _rotations % MAX_ROTATIONS;
    }

    public void ExecuteMove()
    {
        Debug.Log(rotations);
        //for (int i = 0; i < rotations; i++)
           // piece.Rotate();

        Task playTask = new PlayTask(piece, piece.holder.transform.position, targetCoord.ScreenPos());
        playTask.Then(new ActionTask(piece.owner.SetHandStatus));
        Services.GeneralTaskManager.Do(playTask);
    }
}

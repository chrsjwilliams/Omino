using UnityEngine;
using System.Collections;
using EasingEquations;

public class PlayTask : Task
{
    private AIPlayer player;
    private Polyomino piece;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timeElapsed;
    private float duration;
    //private int rotationsCompleted;
    private int rotations;

    public PlayTask(Polyomino piece_, Vector3 startPos_, Vector3 targetPos_, int rotations_)
    {
        piece = piece_;
        startPos = startPos_;
        targetPos = targetPos_;
        rotations = rotations_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        piece.OnInputDown();
        Debug.Log(rotations);

        for (int rotationsCompleted = 0; rotationsCompleted < rotations; rotationsCompleted++)
        {
            piece.Rotate();
        }
        duration = Polyomino.drawAnimDur;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        

        foreach (Tile tile in piece.tiles)
        {
            //Debug.Log(tile.transform.localPosition);
        }

        Vector3 lerpPos = Vector3.Lerp(startPos, targetPos,
            Easing.QuadEaseIn(timeElapsed / duration));
        piece.OnInputDrag(lerpPos);
        piece.Reposition(lerpPos);
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        Debug.Log("Center Coord: " + piece.centerCoord);

        foreach (Tile tile in piece.tiles)
        {
            //Debug.Log(tile.coord.ToString());
        }

        piece.OnInputUp();

        //Debug.Log("Target Pos: " + targetPos);

        
    }
}

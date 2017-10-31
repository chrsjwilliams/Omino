using UnityEngine;
using System.Collections;
using EasingEquations;

public class DrawTask : Task
{
    private Polyomino piece;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timeElapsed;
    private float duration;

    public DrawTask(Polyomino piece_, Vector3 startPos_)
    {
        piece = piece_;
        startPos = startPos_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        piece.MakePhysicalPiece(true);
        piece.Reposition(startPos);
        duration = Polyomino.drawAnimDur;
        targetPos = piece.owner.GetHandPosition(piece.owner.handCount);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        piece.Reposition(Vector3.Lerp(startPos, targetPos, 
            Easing.QuadEaseIn(timeElapsed / duration)));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        piece.owner.AddPieceToHand(piece);
    }
}

using UnityEngine;
using System.Collections;

public class BurnPiece : Task
{
    private float duration;
    private float timeElapsed;
    private Vector3 startPos;
    private Vector3 targetPos;
    private Polyomino piece;

    public BurnPiece(Polyomino piece_)
    {
        piece = piece_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Polyomino.burnPieceDuration;
        startPos = piece.holder.position;
        targetPos = startPos;
        Vector3 offset = Polyomino.burnPieceOffset;
        if(piece.owner.playerNum == 1)
        {
            targetPos += offset;
        }
        else
        {
            targetPos += new Vector3(-offset.x, offset.y, offset.z);
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        piece.holder.transform.position = Vector3.Lerp(startPos, targetPos,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        piece.SetTint(new Color(piece.baseColor.r, piece.baseColor.g, piece.baseColor.b, 
            Mathf.Lerp(1,0,EasingEquations.Easing.QuadEaseIn(timeElapsed/duration))), 1);

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}

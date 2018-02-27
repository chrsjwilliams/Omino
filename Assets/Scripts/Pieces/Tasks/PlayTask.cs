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
        player = piece.owner as AIPlayer;
    }

    public PlayTask(Move move)
    {
        piece = move.piece;
        startPos = move.piece.holder.transform.position;
        targetPos = move.targetCoord.WorldPos();
        rotations = move.rotations;
        player = piece.owner as AIPlayer;
    }

    protected override void Init()
    {
        if (piece.burningFromHand)
        {
            SetStatus(TaskStatus.Aborted);
            return;
        }
        timeElapsed = 0;

        for (int rotationsCompleted = 0; rotationsCompleted < rotations; rotationsCompleted++)
        {
            piece.Rotate();
        }
        piece.OnInputDown();

        duration = Polyomino.drawAnimDur;
    }

    internal override void Update()
    {
        if (piece.owner.gameOver || piece.burningFromHand)
        {
            SetStatus(TaskStatus.Aborted);
            return;
        }
        timeElapsed += Time.deltaTime;

        Vector3 lerpPos = Vector3.Lerp(startPos, targetPos,
            Easing.QuadEaseIn(timeElapsed / duration));
        piece.OnInputDrag(lerpPos);
        piece.Reposition(lerpPos);
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        piece.OnInputUp();
    }

    protected override void OnAbort()
    {
        base.OnAbort();
        player.PlayTaskAborted(piece);
    }

    protected override void CleanUp()
    {
        base.CleanUp();
        player.PlayTaskComplete();
    }
}

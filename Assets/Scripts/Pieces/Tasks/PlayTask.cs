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
    private Move move;

    public PlayTask(Move move_)
    {
        move = move_;
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
        piece.OnInputDown(true);

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
        if (move.blueprintMove != null)
        {
            move.blueprintMove.playTask.Abort();
            Debug.Log("ABORT");
        }
        player.PlayTaskAborted(piece);
    }

    protected override void CleanUp()
    {
        base.CleanUp();
        player.PlayTaskComplete(move);
    }
}

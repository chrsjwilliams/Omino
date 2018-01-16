using UnityEngine;
using System.Collections;
using EasingEquations;

public class PlayTask : Task
{
    private Player player;
    private Polyomino piece;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timeElapsed;
    private float duration;

    public PlayTask(Polyomino piece_, Vector3 startPos_, Vector3 targetPos_)
    {
        piece = piece_;
        startPos = startPos_;
        targetPos = targetPos_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        piece.OnInputDown();
        duration = Polyomino.drawAnimDur;
    }

    internal override void Update()
    {
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
}

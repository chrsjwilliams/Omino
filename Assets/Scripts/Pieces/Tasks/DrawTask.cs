using UnityEngine;


public class DrawTask : Task
{
    private Polyomino piece;
    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 startScale;
    private Vector3 targetScale;
    private float timeElapsed;
    private float duration;
    public static bool[] pieceInTransit = new bool[2];

    public DrawTask(Polyomino piece_, Vector3 startPos_, Player player = null)
    {
        piece = piece_;
        startPos = startPos_;
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Edit || Services.GameManager.mode == TitleSceneScript.GameMode.DungeonEdit)
        {
            pieceInTransit[player.playerNum] = true;
        }
        else
        {
            pieceInTransit[piece.owner.playerNum - 1] = true;
        }
    }

    protected override void Init()
    {
        timeElapsed = 0;
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Edit || Services.GameManager.mode == TitleSceneScript.GameMode.DungeonEdit)
        {
            piece.MakePhysicalPiece();
            piece.Reposition(startPos);
        }
        startPos = piece.holder.transform.position;
        duration = Polyomino.drawAnimDur;
        targetPos = piece.owner.GetHandPosition(piece.owner.handCount) 
            - (piece.GetCenterpoint() * Polyomino.UnselectedScale.x);
        piece.SetAffordableStatus(piece.owner);
        startScale = piece.holder.transform.localScale;
        targetScale = Polyomino.UnselectedScale;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        piece.Reposition(Vector3.Lerp(startPos, targetPos, 
            Easing.QuadEaseIn(timeElapsed / duration)));
        piece.ScaleHolder( Vector3.Lerp(startScale, targetScale,
            Easing.QuadEaseIn(timeElapsed/duration)));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        piece.owner.AddPieceToHand(piece);
        piece.OnDrawn();
        pieceInTransit[piece.owner.playerNum - 1] = false;
    }
}

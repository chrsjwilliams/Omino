using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandPieceEntry : Task
{
    private float timeElapsed;
    private const float animDuration = 0.3f;
    private const float staggerTime = 0.1f;
    private List<Polyomino> hand;
    private bool[] piecesOn;
    private Vector3[] pieceTargetPositions;
    private Vector3 startPos;

    public HandPieceEntry(List<Polyomino> hand_)
    {
        hand = hand_;
    }

    protected override void Init()
    {
        piecesOn = new bool[hand.Count];
        pieceTargetPositions = new Vector3[hand.Count];
        startPos = Services.GameManager.MainCamera.ScreenToWorldPoint(
            Services.UIManager.GetBarPosition(hand[0].owner.playerNum, false));
        startPos = new Vector3(startPos.x, startPos.y, 0);
        for (int i = 0; i < hand.Count; i++)
        {
            Polyomino piece = hand[i];
            pieceTargetPositions[i] = piece.holder.transform.position;
            piece.Reposition(startPos);
        }
        timeElapsed = 0;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < hand.Count; i++)
        {
            if(timeElapsed >= i* staggerTime && !piecesOn[i])
            {
                hand[i].holder.gameObject.SetActive(true);
                piecesOn[i] = true;
            }
            hand[i].Reposition(Vector3.Lerp(startPos, pieceTargetPositions[i], 
                EasingEquations.Easing.QuadEaseOut(
                Mathf.Min(1, (timeElapsed - (i * staggerTime)) / animDuration))));
        }

        if (timeElapsed >= animDuration + ((hand.Count + 1) * staggerTime)) SetStatus(TaskStatus.Success);
    }

}

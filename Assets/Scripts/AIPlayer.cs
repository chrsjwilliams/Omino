using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{

    public override void Init(Color[] playerColorScheme, int posOffset)
    {
        deckClumpCount = 1;
        handSpacing = new Vector3(5.5f, -3.5f, 0);
        handOffset = new Vector3(-12.4f, 5.5f, 0);
        startingHandSize = 4;
        maxHandSize = 5;
        piecesPerHandColumn = 5;
        startingResources = 90;
        baseMaxResources = 100;
        boardPieces = new List<Polyomino>();
        resourceGainIncrementFactor = 1;
        drawRateFactor = 1;
        base.Init(playerColorScheme, posOffset);
        
    }

    // Update is called once per frame
    protected override void Update () {
        base.Update();
	}
}

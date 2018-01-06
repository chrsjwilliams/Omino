using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    int lastframeBoardPieces;
    public List<Coord> playablePostions { get; protected set; }

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
        playablePostions = FindAllPlayablePositions();
    }

    protected List<Coord> FindAllPlayablePositions()
    {
        List<Coord> _playablePositions = new List<Coord>();

        foreach (Polyomino piece in boardPieces)
        {
            foreach (Coord coord in piece.GetAdjacentEmptyTiles())
            {
                if(!_playablePositions.Contains(coord))
                {
                    _playablePositions.Add(coord);
                }
            }
        }

        return _playablePositions;
    }

    protected void PlayPiece(List<Coord> _playablePositions)
    {
        
        int possilbeRotations = 4;
        foreach(Polyomino piece in hand)
        {
            foreach (Coord coord in _playablePositions)
            {
                piece.SetTileCoords(coord);
                if(piece.IsPlacementLegal() && resources > piece.cost &&!gameOver)
                {
                    OnPieceSelected(piece);
                    piece.PlaceAtLocation(coord);      
                    playablePostions = FindAllPlayablePositions();
                    return;
                }
                else
                {
                }
            }
        }
    }

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();
        if(Input.GetKeyDown(KeyCode.P))
            PlayPiece(playablePostions);
    }
}

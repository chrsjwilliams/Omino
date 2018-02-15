using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *      AI TODO:
 *                  AI tries to build blueprints
 *                  Needs to know geometry of pieces on board
 * 
 */ 


public class AIPlayer : Player
{

    [SerializeField]
    private float playDelay;

    public bool beganGame { get; protected set; }
    public bool drawingPiece { get; protected set; }
    public bool playingPiece { get; protected set; }
    public bool isThinking { get; protected set; }
    public Coord primaryTarget { get; protected set; }
    private int frameBuffer;
    private int moveCountBuffer;

    protected float winWeight;
    protected float structWeight;

    public override void Init(Color[] playerColorScheme, int posOffset, float _winWeight, float _structureWeight)
    {
        beganGame = false;
        playingPiece = false;
        isThinking = false;
        drawingPiece = false;
        playDelay = 1.5f;
        deckClumpCount = 4;

        frameBuffer = 1;
        moveCountBuffer = 300;

        winWeight = _winWeight;
        structWeight = _structureWeight;

        handSpacing = new Vector3(5.5f, -2.35f, 0);
        handOffset = new Vector3(-12.6f, 9.125f, 0);

        startingHandSize = 4;
        maxHandSize = 5;
        piecesPerHandColumn = 5;
        startingResources = 7;
        baseMaxResources = 9;
        boardPieces = new List<Polyomino>();
        resourceGainFactor = 1;
        drawRateFactor = 1;
        resourcesPerTick = 1;
        base.Init(playerColorScheme, posOffset, winWeight, structWeight);
        if (playerNum == 1)
        {
            primaryTarget = new Coord(Services.MapManager.MapWidth - 1, Services.MapManager.MapLength - 1);
        }
        else
        {
            primaryTarget = new Coord(0, 0);
        }

        //GeneratePossibleMoves(playableCoords);

        

        //scoredMoves = CreateScoredMoveArray(new Coord(0, 0));
    }

    public List<Coord> FindAllPlayableCoords(int range, bool tossOccupied)
    {
        List<Coord> _playablePositions = new List<Coord>();

        foreach (Polyomino piece in boardPieces)
        {
            foreach (Coord coord in piece.GetAdjacentEmptyTiles())
            {
                for (int dx = -range; dx <= range; dx++)
                {
                    for (int dy = -range; dy <= range; dy++)
                    {
                        Coord radiusOffset = new Coord(dx, dy);
                        Coord candidateCoord = coord.Add(radiusOffset);
                        if (!_playablePositions.Contains(candidateCoord) 
                            && Services.MapManager.IsCoordContainedInMap(candidateCoord))
                        {
                            if (!tossOccupied ||
                              (Services.MapManager.Map[coord.x, coord.y].occupyingPiece == null ||
                               Services.MapManager.Map[coord.x, coord.y].occupyingPiece.owner != this))
                            {
                                _playablePositions.Add(candidateCoord);
                            }
                        }
                    }
                 }
            }
        }

        return _playablePositions;
    }

    


   

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();
        bool canAffordAPiece = false;
        for (int i = 0; i < hand.Count; i++)
        {
            if(hand[i].cost <= resources)
            {
                canAffordAPiece = true;
                break;
            }
        }
        if (canAffordAPiece && !isThinking && !playingPiece && !drawingPiece && Services.GameScene.gameInProgress)
        {
            StartCoroutine(GeneratePossibleMoves());
        }
    }

    protected IEnumerator GeneratePossibleMoves()
    {
        List<Polyomino> currentHand = hand;

        isThinking = true;

        #region playableCoords
        //List<Coord> playableCoords = FindAllPlayableCoords(2, true);
        List<Coord> playableCoords = new List<Coord>();
        //  Finding possibile center coords for pieces
        int countedPlayableCoords = 0;
        int framesTaken = 0;
        foreach (Polyomino piece in boardPieces)
        {
            foreach (Coord coord in piece.GetAdjacentEmptyTiles())
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        countedPlayableCoords++;

                        Coord radiusOffset = new Coord(dx, dy);
                        Coord candidateCoord = coord.Add(radiusOffset);

                        if (countedPlayableCoords % 200 == 0)
                        {
                            framesTaken++;
                            yield return null;
                        }

                        if (!playableCoords.Contains(candidateCoord)
                            && Services.MapManager.IsCoordContainedInMap(candidateCoord))
                        {

                            playableCoords.Add(candidateCoord);
                        }
                    }
                }
            }
        }
        #endregion
        Debug.Log("frames taken for playable coords " + framesTaken);
        framesTaken = 0;
        
        //  Finding tiles that could possibly adjacent pieces AI cares about
        #region touchableCoord
        //List<Coord> touchableCoords = FindAllPlayableCoords(4, false);
        List<Coord> touchableCoords = new List<Coord>();

        int countedTouchedCoords = 0;
        foreach (Polyomino piece in boardPieces)
        {
            foreach (Coord coord in piece.GetAdjacentEmptyTiles())
            {
                for (int dx = -4; dx <= 4; dx++)
                {
                    for (int dy = -4; dy <= 4; dy++)
                    {
                        countedTouchedCoords++;
                        Coord radiusOffset = new Coord(dx, dy);
                        Coord candidateCoord = coord.Add(radiusOffset);

                        if (countedTouchedCoords % 200 == 0)
                        {
                            framesTaken++;
                            yield return null;
                        }

                        if (!touchableCoords.Contains(candidateCoord)
                            && Services.MapManager.IsCoordContainedInMap(candidateCoord))
                        {
                            if (Services.MapManager.Map[coord.x, coord.y].occupyingPiece == null ||
                               Services.MapManager.Map[coord.x, coord.y].occupyingPiece.owner != this)
                                    touchableCoords.Add(candidateCoord);

                            
                        }
                    }
                }
            }
        }
        #endregion
        Debug.Log("frames taken for touchable coords " + framesTaken);
        framesTaken = 0;

        List<Polyomino>[,] adjPiecesByCoord = new List<Polyomino>[
            Services.MapManager.MapWidth,
            Services.MapManager.MapLength];
        Move nextPlay = null;

        int coordsConsidered = 0;
        foreach(Coord coord in touchableCoords)
        {
            adjPiecesByCoord[coord.x, coord.y] = Polyomino.GetAdjacentPolyominosToCoord(coord, this);
            coordsConsidered++;
            if (coordsConsidered % (touchableCoords.Count / 5) == 0)
            {
                framesTaken++;
                yield return null;
            }
        }
        Debug.Log("frames taken for adjacent pieces " + framesTaken);

        int movesTried = 0;
        // if 
        foreach (Polyomino piece in currentHand)
        {
            if(piece.cost <= resources) { 
            //  Check the target coord and center coord
            // rotate piece
            Coord roundedPos = new Coord((int)piece.holder.transform.position.x, (int)piece.holder.transform.position.y);
            piece.SetTileCoords(roundedPos);
            int numRotations = Polyomino.rotationDictionary[piece.tiles.Count][piece.index];
                for (int i = 0; i < playableCoords.Count; i++)
                {
                    // each piece should know how many times it can be rotated
                    for (int rotations = 0; rotations < 4; rotations++)
                    {
                        piece.Rotate(false, true, true);
                        //  For play positions +/- a pieces radius
                        piece.SetTileCoords(playableCoords[i]);
                        piece.TurnOffGlow();
                        if (rotations < numRotations)
                        {
                            movesTried++;
                            if (movesTried % 50 == 0)
                            {
                                //Debug.Log("Player " + playerNum + " Thinking: " + Time.time);
                                yield return null;
                            }
                            List<Polyomino> adjacentPolyominos = new List<Polyomino>();
                            foreach (Tile tile in piece.tiles)
                            {
                                if (Services.MapManager.IsCoordContainedInMap(tile.coord))
                                {
                                    adjacentPolyominos.AddRange(adjPiecesByCoord[tile.coord.x, tile.coord.y]);
                                }
                            }
                            if (piece.IsPlacementLegal(adjacentPolyominos))
                            {
                                Move newMove = new Move(piece, playableCoords[i], (rotations + 1) % 4, winWeight, structWeight);
                                if (nextPlay == null) nextPlay = newMove;
                                else if (newMove.score > nextPlay.score)
                                {
                                    nextPlay = newMove;
                                }
                            }
                        }

                    }
                }
                //  Then choose a new play position
            }
            //  Choose a new piece
            //  Wait for a certain number of frames before evaluating next piece

           
            /*
            for (int frames = 0; frames < frameBuffer; frames++)
            {
                yield return null;
            }
            */
            
        }

        if (nextPlay != null)
        {
            MakePlay(nextPlay);
        }
        isThinking = false;
        //Debug.Log("Player " + playerNum + " Moves Tried: " + movesTried);
    }


    private void MakePlay(Move nextPlay)
    {
        playingPiece = true;
        nextPlay.ExecuteMove();
    }

    public override int GainResources(int numResources)
    {
        int _resources = base.GainResources(numResources);
        return _resources;
    }

    public override void OnPiecePlaced(Polyomino piece)
    {
        base.OnPiecePlaced(piece);
        StopThinking();
        playingPiece = false;
    }

    public override void DrawPiece(Vector3 startPos, bool onlyDestructors)
    {
        base.DrawPiece(startPos, onlyDestructors);
        drawingPiece = true;
    }

    public override void OnPieceRemoved(Polyomino piece)
    {
        base.OnPieceRemoved(piece);
        StopThinking();
    }

    public override void AddPieceToHand(Polyomino piece)
    {
        base.AddPieceToHand(piece);
        StopThinking();
        drawingPiece = false;
    }

    void StopThinking()
    {
        StopCoroutine(GeneratePossibleMoves());
        isThinking = false;
    }

    public override void CancelSelectedPiece()
    {
        base.CancelSelectedPiece();
        playingPiece = false;
        StopThinking();
    }

}

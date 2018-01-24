using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    struct PlayPosition
    {
        public float distance;
        public Move move;
    }

    int lastframeBoardPieces;
    public List<Coord> playablePositions { get; protected set; }
    private PlayPosition[] playPositions;
    public List<Move> possibleMoves { get; protected set; }

    public override void Init(Color[] playerColorScheme, int posOffset)
    {
        deckClumpCount = 4;

        handSpacing = new Vector3(5.5f, -2.35f, 0);
        handOffset = new Vector3(-12.6f, 9.125f, 0);

        startingHandSize = 4;
        maxHandSize = 5;
        piecesPerHandColumn = 5;
        startingResources = 70;
        baseMaxResources = 100;
        boardPieces = new List<Polyomino>();
        possibleMoves = new List<Move>();
        resourceGainFactor = 1;
        drawRateFactor = 1;
        resourcesPerTick = 10;
        base.Init(playerColorScheme, posOffset);
        playablePositions = FindAllPlayablePositions();
        //playPositions = SortPlayableMovesByDistance(new Coord(5, 5), playablePositions);

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

    //  Not quite working yet.
    private void GeneratePossibleMoves(List<Coord> playCoords)
    {
        foreach(Polyomino piece in hand)
        {
            for (int i = 0; i < playCoords.Count; i++)
            {
                // each piece should know how many times it can be rotated
                for (int rotations = 0; rotations < Move.MAX_ROTATIONS; rotations++)
                {
                    //  For play positions +/- a pieces radius
                    for (int dx = -2; dx < 2; dx++)
                    {
                        for (int dy = -2; dy < 2; dy++)
                        {
                            Coord radiusOffset = new Coord(dx, dy);
                            Coord candidateCoord = playCoords[i].Add(radiusOffset);
                            piece.SetTileCoords(candidateCoord);
                            if (piece.IsPlacementLegal() && piece.cost <=+ resources)
                            {
                                Move newMove = new Move(piece, candidateCoord, rotations);
                                if(!possibleMoves.Contains(newMove))
                                {
                                    possibleMoves.Add(newMove);
                                }

                            }
                        }
                    }
                    // rotate piece
                    //piece.Rotate();
                }
                //  Then choose a new play position
            }  
            //  Then choose a new piece
        }
        //  No playable moves
    }

    private PlayPosition[] SortPlayableMovesByDistance(Coord targetCoord, List<Move> _playablePositions)
    {
        if (targetCoord == null)
        {
            int xRand = Random.Range(0, Services.MapManager.MapLength);
            int yRand = Random.Range(0, Services.MapManager.MapWidth);
            targetCoord = new Coord(xRand, yRand);
        }

        //  Create an array for each playable position and its distance to the target
        PlayPosition[] playPosition = new PlayPosition[_playablePositions.Count];

        for (int i = 0; i < _playablePositions.Count; i++)
        {
            playPosition[i] = new PlayPosition();
            //  Playable move and possible positions are different!
            playPosition[i].move = _playablePositions[i];
            playPosition[i].distance = targetCoord.Distance(_playablePositions[i].targetCoord);       
        }

        // sort playable positions beased on closeness to targetCoord
        quick_sort(playPosition, 0, playPosition.Length - 1);

        return playPosition;
    }

    protected void PlayPiece()
    {
        int moveIndex = Random.Range(0, possibleMoves.Count - 1);

        //  Sort moves!

        //possibleMoves[moveIndex].ExecuteMove();
        //possibleMoves.Clear();

        playPositions[0].move.ExecuteMove();
        possibleMoves.Clear();
        playablePositions = null;
    }

    protected void PlayPiece(Polyomino piece, bool onlyDestructors)
    {
        //  For now I'm just selecting the first piece in
        //  the player's hand
        Vector3 startPos = hand[0].holder.transform.position;

        //  Right now this selects a random play position
        //  This does not take into account if a piece can fit
        //  in the allotted spcae based on its center coord
        Vector3 targetPos = PlayPositionToVector3(playPositions[Random.Range(0, playPositions.Length - 1)]);
        
        Task playTask = new PlayTask(hand[0], startPos, targetPos);
        playTask.Then(new ActionTask(SetHandStatus));
        Services.GeneralTaskManager.Do(playTask);
    }

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.C))
            Debug.Log(hand.Count);

        if (Input.GetKeyDown(KeyCode.P))
        {
            
            playablePositions = FindAllPlayablePositions();
            GeneratePossibleMoves(playablePositions);
            playPositions = SortPlayableMovesByDistance(new Coord(0, 0), possibleMoves);
            PlayPiece();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            playablePositions = FindAllPlayablePositions();
            //playPositions = SortPlayableMovesByDistance(new Coord(5, 5), playablePositions);
            //Polyomino pieceToPlay = SelectPiece(playPositions);
            
            PlayPiece(null, false);         
        }
    }

    private Vector3 PlayPositionToVector3(PlayPosition playPosition)
    {
        return Services.MapManager.Map[playPosition.move.targetCoord.x, playPosition.move.targetCoord.y].transform.position;
    }

    private void quick_sort(PlayPosition[] playPositions, int start, int end)
    {
        if (start < end)
        {
            int pIndex = partition(playPositions, start, end);
            quick_sort(playPositions, start, pIndex - 1);
            quick_sort(playPositions, pIndex + 1, end);
        }
    }

    private int partition(PlayPosition[] playPositions, int start, int end)
    {
        PlayPosition pivot = playPositions[end];
        int pIndex = start;
        PlayPosition temp;

        for (int i = start; i < end; i++)
        {
            if (playPositions[i].distance <= pivot.distance)
            {
                temp = playPositions[i];
                playPositions[i] = playPositions[pIndex];
                playPositions[pIndex] = temp;
                pIndex++;
            }
        }

        temp = playPositions[pIndex];
        playPositions[pIndex] = playPositions[end];
        playPositions[end] = temp;
        return pIndex;
    }
}

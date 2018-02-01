using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    struct ScoredMove
    {
        public float distance;
        public Move move;
    }

    [SerializeField]
    private float playDelay;

    public bool playWasMade { get; protected set; }
    public List<Coord> playableCoords { get; protected set; }
    private ScoredMove[] scoredMoves;
    public List<Move> possibleMoves { get; protected set; }
    private Coord primaryTarget;

    public override void Init(Color[] playerColorScheme, int posOffset)
    {
        

        playDelay = 1.5f;
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
        if (playerNum == 1)
        {
            primaryTarget = new Coord(Services.MapManager.MapWidth - 1, Services.MapManager.MapLength - 1);
        }
        else
        {
            primaryTarget = new Coord(0, 0);
        }

        playableCoords = FindAllPlayableCoords();
        GeneratePossibleMoves(playableCoords);
        scoredMoves = SortPlayableMovesByDistance(new Coord(0, 0));

    }

    public List<Coord> FindAllPlayableCoords()
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

    public void GeneratePossibleMoves(List<Coord> playCoords)
    {
        possibleMoves.Clear();

        foreach (Polyomino piece in hand)
        {
            //  Check the target coord and center coord
            // rotate piece
            Coord roundedPos = new Coord((int)piece.holder.transform.position.x, (int)piece.holder.transform.position.y);
            piece.SetTileCoords(roundedPos);
            for (int i = 0; i < playCoords.Count; i++)
            {
                // each piece should know how many times it can be rotated
                for (int rotations = 0; rotations <= Move.MAX_ROTATIONS ; rotations++)
                {
                    piece.Rotate(false);
                    //  For play positions +/- a pieces radius
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            Coord radiusOffset = new Coord(dx, dy);
                            Coord candidateCoord = playCoords[i].Add(radiusOffset);
                            piece.SetTileCoords(candidateCoord);

                            

                            if (piece.IsPlacementLegal() && piece.cost <= resources)
                            {


                                Move newMove = new Move(piece, candidateCoord, (rotations + 1) % 4);
                                if(!possibleMoves.Contains(newMove))
                                {
                                    if (piece.index == 0 && candidateCoord.x == 14 && candidateCoord.y == 17)
                                    {
                                        Debug.Log("Line Rotations: " + (rotations + 1) % 4);
                                        Debug.Log("Candidate: " + candidateCoord.ToString());
                                        foreach (Tile tile in piece.tiles)
                                        {
                                            Debug.Log(tile.coord.ToString());
                                        }
                                    }
                                    possibleMoves.Add(newMove);
                                }

                            }
                        }
                    }           
                   
                }
                //  Then choose a new play position
            }
          //    Choose a new piece  
        }
    }

    private ScoredMove[] SortPlayableMovesByDistance(Coord targetCoord)
    {
        if (targetCoord == null)
        {
            int xRand = UnityEngine.Random.Range(0, Services.MapManager.MapLength);
            int yRand = UnityEngine.Random.Range(0, Services.MapManager.MapWidth);
            targetCoord = new Coord(xRand, yRand);
        }

        //  Create an array for each playable position and its distance to the target
        ScoredMove[] playPosition = new ScoredMove[possibleMoves.Count];

        for (int i = 0; i < possibleMoves.Count; i++)
        {
            playPosition[i] = new ScoredMove();
            playPosition[i].move = possibleMoves[i];
            float pieceDistance = float.MaxValue;
            //  Saves the distance of the closest tile

            foreach (Tile tile in possibleMoves[i].piece.tiles)
            {
                float tileDistance = targetCoord.Distance(
                    possibleMoves[i].relativeCoords[tile].Add(possibleMoves[i].targetCoord));

                if (tileDistance < pieceDistance)
                {
                    pieceDistance = tileDistance;
                }
            }
            playPosition[i].distance = pieceDistance;
        }

        // sort playable positions beased on closeness to targetCoord
        quick_sort(playPosition, 0, playPosition.Length - 1);

        return playPosition;
    }

    protected void PlayPiece()
    {
        int moveIndex = UnityEngine.Random.Range(0, possibleMoves.Count - 1);

        if (!scoredMoves[0].move.executed || scoredMoves[0].move != null)
            scoredMoves[0].move.ExecuteMove();

        playableCoords = null;
    }

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.C))
            Debug.Log(hand.Count);

        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach(Polyomino piece in hand)
            {
                Coord roundedPos = new Coord((int)piece.holder.transform.position.x, (int)piece.holder.transform.position.y);
                piece.SetTileCoords(roundedPos);
                piece.Rotate(false);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.P) || (resources > 30 && selectedPiece == null && hand.Count > 0 && possibleMoves.Count > 0))
        {
            MakePlay();

        }
    }

    private void AssesMoves()
    {
        playableCoords = FindAllPlayableCoords();
        GeneratePossibleMoves(playableCoords);
        scoredMoves = SortPlayableMovesByDistance(primaryTarget);
    }

    private void MakePlay()
    {
        AssesMoves();
        PlayPiece();
    }

    private Vector3 PlayPositionToVector3(ScoredMove playPosition)
    {
        return Services.MapManager.Map[playPosition.move.targetCoord.x, playPosition.move.targetCoord.y].transform.position;
    }

    private void quick_sort(ScoredMove[] playPositions, int start, int end)
    {
        if (start < end)
        {
            int pIndex = partition(playPositions, start, end);
            quick_sort(playPositions, start, pIndex - 1);
            quick_sort(playPositions, pIndex + 1, end);
        }
    }

    private int partition(ScoredMove[] playPositions, int start, int end)
    {
        ScoredMove pivot = playPositions[end];
        int pIndex = start;
        ScoredMove temp;

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

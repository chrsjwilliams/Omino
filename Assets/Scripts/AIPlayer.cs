using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *      AI TODO:
 *                  Implement scoring for moves
 *                  Implement Structure claiming behaviour
 *                  Reduce number of calculations
 * 
 */ 


public class AIPlayer : Player
{
    struct ScoredMove
    {
        public const float MAX_SCORE = 100;
        public float score;
        public float distance;
        public Move move;
    }

    [SerializeField]
    private float playDelay;

    public bool beganGame { get; protected set; }
    public bool drawingPiece { get; protected set; }
    public bool playingPiece { get; protected set; }
    public bool isThinking { get; protected set; }
    public List<Coord> playableCoords { get; protected set; }
    protected List <Coord> structureCoords { get; private set; }
    private ScoredMove[] scoredMoves;
    public List<Move> possibleMoves { get; protected set; }
    private Coord primaryTarget;
    private int frameBuffer;

    protected float winConditionMod;
    protected float structConditionMod;

    public override void Init(Color[] playerColorScheme, int posOffset)
    {
        beganGame = false;
        playingPiece = false;
        isThinking = false;
        drawingPiece = false;
        playDelay = 1.5f;
        deckClumpCount = 4;

        frameBuffer = 1;

        winConditionMod = 0.8f;
        structConditionMod = 1.0f;

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
        //GeneratePossibleMoves(playableCoords);

        structureCoords = new List<Coord>();
        foreach(Structure structure in Services.MapManager.structuresOnMap)
        {
            foreach(Tile tile in structure.tiles)
            {
                if(!structureCoords.Contains(tile.coord))
                {
                    structureCoords.Add(tile.coord);
                }
            }
        }

        //scoredMoves = CreateScoredMoveArray(new Coord(0, 0));
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

    private ScoredMove[] CreateScoredMoveArray(Coord targetCoord)
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
            float structDist = float.MaxValue;

            foreach (Tile tile in possibleMoves[i].piece.tiles)
            {
                float winDistance = targetCoord.Distance(
                    possibleMoves[i].relativeCoords[tile].Add(possibleMoves[i].targetCoord));
                
                foreach(Coord coord in structureCoords)
                {
                    float testDistance = coord.Distance(
                        possibleMoves[i].relativeCoords[tile].Add(possibleMoves[i].targetCoord));

                    if (testDistance < structDist && 
                        Services.MapManager.Map[coord.x, coord.y].occupyingStructure.owner == null)
                    {
                        structDist = testDistance;
                    }
                }

                if (winDistance < pieceDistance)
                {
                    pieceDistance = winDistance;
                }
            }
            playPosition[i].distance = pieceDistance;
            playPosition[i].score = CalculateScore(playPosition[i].distance, structDist);
        }

        return playPosition;
    }

    public float CalculateScore(float win, float structure)
    {
        if (win == 0 && structure == 0) win = float.MaxValue;
        return ScoredMove.MAX_SCORE / ((win * winConditionMod) + (structure * structConditionMod));
    }

    protected void PlayPiece()
    {
        playingPiece = true;
        int moveIndex = UnityEngine.Random.Range(0, possibleMoves.Count - 1);

        ScoredMove nextPlay = scoredMoves[0];        

        for (int i = 0; i < scoredMoves.Length; i++)
        {
            //  While I'm going through every move in the array
            //  I will adjusts its score based on our parameters
            if (nextPlay.score < scoredMoves[i].score)
                nextPlay = scoredMoves[i];
        }

        if (scoredMoves.Length > 0)
            nextPlay.move.ExecuteMove();

        playableCoords = null;
    }

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();

        if (Services.GameScene.gameInProgress && !beganGame)
        {
            beganGame = true;
            StartCoroutine(GeneratePossibleMoves());
        }

        if (!isThinking && !playingPiece && !drawingPiece && Services.GameScene.gameInProgress)
        {
            StartCoroutine(GeneratePossibleMoves());
        }
    }

    protected IEnumerator GeneratePossibleMoves()
    {
        isThinking = true;
        playableCoords = FindAllPlayableCoords();

        foreach (Polyomino piece in hand)
        {
            //  Check the target coord and center coord
            // rotate piece
            Coord roundedPos = new Coord((int)piece.holder.transform.position.x, (int)piece.holder.transform.position.y);
            piece.SetTileCoords(roundedPos);
            for (int i = 0; i < playableCoords.Count; i++)
            {
                // each piece should know how many times it can be rotated
                for (int rotations = 0; rotations <= Move.MAX_ROTATIONS; rotations++)
                {
                    piece.Rotate(false);
                    //  For play positions +/- a pieces radius
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            Coord radiusOffset = new Coord(dx, dy);
                            Coord candidateCoord = playableCoords[i].Add(radiusOffset);
                            piece.SetTileCoords(candidateCoord);
                            piece.TurnOffGlow();
                            if (piece.IsPlacementLegal() && piece.cost <= resources)
                            {
                                Move newMove = new Move(piece, candidateCoord, (rotations + 1) % 4);
                                if (!possibleMoves.Contains(newMove))
                                {
                                    possibleMoves.Add(newMove);
                                }
                            }
                        }
                    }
                }
                //  Then choose a new play position
            }
            //  Choose a new piece
            //  Wait for a certain number of frames before evaluating next piece
            for (int frames = 0; frames < frameBuffer; frames++)
            {
                yield return null;
            }
        }

        scoredMoves = CreateScoredMoveArray(primaryTarget);
        if (Input.GetKeyDown(KeyCode.P) || (resources > 20 && selectedPiece == null && hand.Count > 0 && possibleMoves.Count > 0))
        {
            MakePlay();
            possibleMoves.Clear();
        }
    }

    private void AssesMoves()
    {
        playableCoords = FindAllPlayableCoords();
        GeneratePossibleMoves(playableCoords);
        scoredMoves = CreateScoredMoveArray(primaryTarget);
    }

    private void MakePlay()
    {
        PlayPiece();
    }

    public override int GainResources(int numResources)
    {
        int _resources = base.GainResources(numResources);
        if (resources > 20)
        {
            StartCoroutine(GeneratePossibleMoves());
            isThinking = false;
        }
        return _resources;
    }

    public override void OnPiecePlaced(Polyomino piece)
    {
        base.OnPiecePlaced(piece);
        playingPiece = false;
        isThinking = false;
    }

    public override void DrawPiece(Vector3 startPos, bool onlyDestructors)
    {
        base.DrawPiece(startPos, onlyDestructors);
        StopCoroutine(GeneratePossibleMoves());
        drawingPiece = true;
        isThinking = false;
    }

    public override void OnPieceRemoved(Polyomino piece)
    {
        base.OnPieceRemoved(piece);
    }

    public override void AddPieceToHand(Polyomino piece)
    {
        base.AddPieceToHand(piece);
        drawingPiece = false;
    }

    public override void CancelSelectedPiece()
    {
        base.CancelSelectedPiece();
        StartCoroutine(GeneratePossibleMoves());
    }

    private Vector3 PlayPositionToVector3(ScoredMove playPosition)
    {
        return Services.MapManager.Map[playPosition.move.targetCoord.x, playPosition.move.targetCoord.y].transform.position;
    }
}

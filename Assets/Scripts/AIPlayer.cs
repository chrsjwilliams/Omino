using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    public bool drawingPiece { get; protected set; }
    public bool playingPiece { get; protected set; }
    public bool isThinking { get; protected set; }
    public List<Coord> primaryTargets { get; protected set; }

    private int coordCountBuffer;
    private int movesTriedBuffer;
    private int tilesUntilBlueprint;
    private int totalRandomizations;
    private int kragerAlgorithmBuffer;

    protected float winWeight;
    protected float structWeight;
    protected float blueprintWeight;
    protected float destructionWeight;
    private IEnumerator thinkingCoroutine;

    public override void Init(Color[] playerColorScheme, int posOffset, float _winWeight, float _structureWeight, 
        float _blueprintWeight, float _destructionWeight)
    {
        playingPiece = false;
        isThinking = false;
        drawingPiece = false;
        deckClumpCount = 4;

        movesTriedBuffer = 50;
        coordCountBuffer = 200;
        tilesUntilBlueprint = 5;
        totalRandomizations = 500;
        kragerAlgorithmBuffer = 100;

        winWeight = _winWeight;
        structWeight = _structureWeight;
        blueprintWeight = _blueprintWeight;
        destructionWeight = _destructionWeight;

        handSpacing = new Vector3(5.5f, -2.35f, 0);
        handOffset = new Vector3(-12.6f, 9.125f, 0);

        startingHandSize = 4;
        maxHandSize = 5;
        piecesPerHandColumn = 5;
        startingResources = 6;
        baseMaxResources = 9;
        boardPieces = new List<Polyomino>();
        resourceGainFactor = 1;
        drawRateFactor = 1;
        resourcesPerTick = 1;
        base.Init(playerColorScheme, posOffset, winWeight, structWeight, blueprintWeight, destructionWeight);
        Debug.Log("player " + playerNum + "using " + "\nwin weight: " + winWeight);
        Debug.Log("struct weight: " + structWeight + "\nblueprint weight: " + blueprintWeight);

        if (playerNum == 1)
        {
            primaryTargets = new List<Coord>()
            {
                new Coord(Services.MapManager.MapWidth - 3, Services.MapManager.MapHeight - 3),
                new Coord(Services.MapManager.MapWidth - 2, Services.MapManager.MapHeight - 3),
                new Coord(Services.MapManager.MapWidth - 1, Services.MapManager.MapHeight - 3),
                new Coord(Services.MapManager.MapWidth-3, Services.MapManager.MapHeight -2),
                new Coord(Services.MapManager.MapWidth -3, Services.MapManager.MapHeight -1)
            };
        }
        else
        {
            primaryTargets = new List<Coord>()
            {
                new Coord(0,2),
                new Coord(1,2),
                new Coord(2,2),
                new Coord(2,1),
                new Coord(2,0)
            };
        }
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
        if (canAffordAPiece && !isThinking && !playingPiece && !drawingPiece && Services.GameScene.gameInProgress && !gameOver)
        {
            thinkingCoroutine = GeneratePossibleMoves();
            StartCoroutine(thinkingCoroutine);
        }
    }

    // I need to run the blueprint check for every move



    public Graph<Polyomino> MakeOpponentPieceGraph(List<Polyomino> currentAllBoardPieces)
    {
        Graph<Polyomino> graph = new Graph<Polyomino>();
        foreach (Polyomino piece in currentAllBoardPieces)
        {
            if (piece.owner != null &&
                piece.owner != this)
            {
                //  the first parameter is the vertex
                //  the second parameter are its adajcent verticies
                graph.Vertices.Add(piece, piece.GetAdjacentPolyominos(piece.owner));
            }
        }

        return graph;
    }

    protected IEnumerator GeneratePossibleMoves()
    {
        List<Polyomino> currentHand = new List<Polyomino>(hand);
        List<Polyomino> currentBoardPieces = new List<Polyomino> (boardPieces);
        
        #region Failed Attempt Krager's Algorithm
        /*
        //  Collect the board pieces so I can make a graph of the opponent pieces
        List<Polyomino> allBoardPieces = Services.GameScene.allBoardPieces;

        //  These are the polyominos I can cut
        Dictionary<Polyomino, List<Polyomino>> cuts = new Dictionary<Polyomino, List<Polyomino>>();
        for(int i = 0; i < totalRandomizations; i++)
        {
            Graph<Polyomino> opponentGraph = MakeOpponentPieceGraph(allBoardPieces);
            opponentGraph.ApplyKrager();
            foreach(var vertex in opponentGraph.Vertices)
            {
                //  use the key and value of the cut to find the edge(s) which would
                //  make a bipartite graph
                cuts.Add(vertex.Key, vertex.Value);
            }
        }

        //  We can then use the list of cuts and extract coords to pass along to our move
        */
        #endregion

        #region Calculate economy stuff

        float mineWeight = 0;
        float factoryWeight = 0;
        float bombFactoryWeight = 0;

        int normalPieceCost = biggerBricks ? 5 : 4;
        int destructorCost = biggerBombs ? 4 : 3;

        float expenditurePerSecond = (normalDrawRate * normalPieceCost) + (destructorDrawRate * destructorCost);

        float productionRatio = expenditurePerSecond / resourceGainRate;

        if(productionRatio > 1)
        {
            mineWeight = blueprintWeight;
        }
        else
        {
            factoryWeight = blueprintWeight;
            bombFactoryWeight = blueprintWeight;
        }

        #endregion

        isThinking = true;
        //  Finding tiles that my center coord could be placed
        #region Gets Playable Coords
        //List<Coord> playableCoords = FindAllPlayableCoords(2, true);
        List<Coord> playableCoords = new List<Coord>();
        HashSet<Coord> possibleBlueprintCoords = new HashSet<Coord>();
        List<Coord> touchableCoords = new List<Coord>();
        List<BlueprintMap> possibleBlueprintMoves = new List<BlueprintMap>();

        //  Finding possibile center coords for pieces
        int countedPlayableCoords = 0;
        foreach (Polyomino piece in currentBoardPieces)
        {
            if (piece.connected || piece is Structure)
            {
                foreach (Tile tile in piece.tiles)
                {
                    for (int dx = -5; dx <= 5; dx++)
                    {
                        for (int dy = -5; dy <= 5; dy++)
                        {
                            countedPlayableCoords++;

                            Coord radiusOffset = new Coord(dx, dy);
                            Coord candidateCoord = tile.coord.Add(radiusOffset);

                            if (countedPlayableCoords % coordCountBuffer == 0)
                            {
                                yield return null;
                            }
                            bool containedInMap = Services.MapManager.IsCoordContainedInMap(candidateCoord);
                            Tile mapTile = null;
                            if (containedInMap) mapTile = Services.MapManager.Map[candidateCoord.x, candidateCoord.y];
                            if (Mathf.Abs(dx) <= 3 && Mathf.Abs(dy) <= 3)
                            {
                                if (!playableCoords.Contains(candidateCoord) &&
                                    containedInMap &&
                                    mapTile.occupyingPiece == null &&
                                    mapTile.occupyingStructure == null)
                                {
                                    playableCoords.Add(candidateCoord);
                                }

                                if (!possibleBlueprintCoords.Contains(candidateCoord) &&
                                    containedInMap &&
                                    mapTile.occupyingStructure == null &&
                                    mapTile.occupyingBlueprint == null)
                                {
                                    possibleBlueprintCoords.Add(candidateCoord);
                                }
                            }

                            if (!touchableCoords.Contains(candidateCoord) &&
                                containedInMap &&
                                (mapTile.occupyingPiece == null || mapTile.occupyingPiece.owner != this))
                            {
                                touchableCoords.Add(candidateCoord);
                            }
                        }
                    }
                }
            }
        }

        //Debug.Log("Counted Blueprint Coords: " + countedBlueprintCoords);
        #endregion

        //  Finding tiles that could possibly adjacent pieces AI cares about
        List<Polyomino>[,] adjPiecesByCoord = new List<Polyomino>[
            Services.MapManager.MapWidth,
            Services.MapManager.MapHeight];

        Move nextPlay = null;
        int coordsConsidered = 0;
        foreach(Coord coord in touchableCoords)
        {
            adjPiecesByCoord[coord.x, coord.y] = Polyomino.GetAdjacentPolyominosToCoord(coord, this);
            coordsConsidered++;
            if (coordsConsidered % (touchableCoords.Count / 5) == 0)
            {
                yield return null;
            }
        }
        //Debug.Log("touchable coords: " + touchableCoords.Count());
        #region Blueprint Placement Logic
        int blueprintsTried = 0;
        foreach (Blueprint blueprint in blueprints)
        { 
            Coord roundedPos = new Coord((int)blueprint.holder.transform.position.x,
                                            (int)blueprint.holder.transform.position.y);

            blueprint.SetTileCoords(roundedPos);
            int numRotations = blueprint.maxRotations;
            foreach (Coord coord in possibleBlueprintCoords)
            {
                for (int rotations = 0; rotations < 4; rotations++)
                {
                    blueprint.Rotate(false, true);
                    //  For play positions +/- a pieces radius
                    blueprint.SetTileCoords(coord);
                    blueprint.TurnOffGlow();
                    if (rotations < numRotations)
                    {
                        bool illegalPlacement = false;
                        HashSet<Coord> missingCoordHashSet = new HashSet<Coord>();
                        foreach (Tile tile in blueprint.tiles)
                        {
                            bool containedInMap = Services.MapManager.IsCoordContainedInMap(tile.coord);
                            Tile mapTile = null;
                            if (containedInMap) mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                            if (!containedInMap ||
                                (mapTile.occupyingPiece != null &&
                                    (mapTile.occupyingPiece is Structure || mapTile.occupyingPiece.owner != this ||
                                    !mapTile.occupyingPiece.connected)) ||
                                mapTile.occupyingBlueprint != null)
                            {
                                illegalPlacement = true;
                                break;
                            }
                            if (mapTile.occupyingPiece == null)
                            {
                                missingCoordHashSet.Add(tile.coord);
                            }
                        }
                        if (illegalPlacement) continue;
                        blueprintsTried++;
                        if (blueprintsTried % movesTriedBuffer == 0)
                        {
                            yield return null;
                        }

                        if (missingCoordHashSet.Count() <= tilesUntilBlueprint)
                        {
                            BlueprintMap blueprintMap = new BlueprintMap(blueprint, missingCoordHashSet, coord, (rotations + 1) % 4);
                            possibleBlueprintMoves.Add(blueprintMap);
                        }
                    }
                }
            }
        }
        //Debug.Log("Blueprints Tried: " + blueprintsTried);
        #endregion

        #region Polyomino Placement Logic
        int movesTried = 0;
        foreach (Polyomino piece in currentHand)
        {
            if (piece.cost <= resources)
            {
                //  Check the target coord and center coord
                Coord roundedPos = new Coord((int)piece.holder.transform.position.x, 
                                             (int)piece.holder.transform.position.y);

                piece.SetTileCoords(roundedPos);

                int numRotations = Polyomino.pieceRotationDictionary[piece.tiles.Count][piece.index];

                for (int i = 0; i < playableCoords.Count; i++)
                {
                    // each piece should know how many times it can be rotated
                    for (int rotations = 0; rotations < 4; rotations++)
                    {
                        piece.Rotate(false, true);
                        piece.SetTileCoords(playableCoords[i]);
                        piece.TurnOffGlow();
                        if (rotations < numRotations)
                        {
                            movesTried++;
                            if (movesTried % movesTriedBuffer == 0)
                            {
                                yield return null;
                            }

                            List<Polyomino> adjacentPolyominos = new List<Polyomino>();

                            foreach (Tile tile in piece.tiles)
                            {
                                if (Services.MapManager.IsCoordContainedInMap(tile.coord))
                                {
                                    if(adjPiecesByCoord[tile.coord.x, tile.coord.y] != null)
                                    {
                                        adjacentPolyominos.AddRange(adjPiecesByCoord[tile.coord.x, tile.coord.y]);
                                    }
                                }
                            }

                            if (piece.IsPlacementLegal(adjacentPolyominos))
                            {
                                HashSet<Coord> pieceCoords = new HashSet<Coord>();
                                foreach (Tile tile in piece.tiles)
                                {
                                    if (!pieceCoords.Contains(tile.coord))
                                    {
                                        pieceCoords.Add(tile.coord);
                                    }
                                }

                                Move newMove = new Move(piece, playableCoords[i], (rotations + 1) % 4, possibleBlueprintMoves,
                                    winWeight, structWeight, destructionWeight, mineWeight, factoryWeight, bombFactoryWeight);
                                
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
        }
        #endregion

        //Debug.Log("BlueprintMaps Tried: " + blueprintMapsTried);

        if (nextPlay != null)
        {
           
            MakePlay(nextPlay);
        }
        isThinking = false;
    }

    private void MakePlay(Move nextPlay)
    {
        if (nextPlay.piece != null)
        {
            playingPiece = true;
            nextPlay.ExecuteMove();
        }
    }

    public override int GainResources(int numResources)
    {
        int _resources = base.GainResources(numResources);
        return _resources;
    }

    public override void OnPiecePlaced(Polyomino piece)
    {
        base.OnPiecePlaced(piece);
    }

    public override void OnOpposingPiecePlaced()
    {
        base.OnOpposingPiecePlaced();
        StopThinking();
    }

    public void PlayTaskComplete(Move playedMove)
    {
        if(playedMove.blueprintMove == null) playingPiece = false;
    }

    public void PlayTaskAborted(Polyomino piece)
    {
        if (selectedPiece == piece) selectedPiece = null;
    }

    public override void DrawPiece(Vector3 startPos, bool onlyDestructors)
    {
        base.DrawPiece(startPos, onlyDestructors);
        drawingPiece = true;
    }

    protected override void BurnFromHand(Polyomino piece)
    {
        base.BurnFromHand(piece);
        StopThinking();
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
        if (isThinking)
        {
            StopCoroutine(thinkingCoroutine);
            isThinking = false;
        }
    }

    public override void CancelSelectedPiece()
    {
        base.CancelSelectedPiece();
        playingPiece = false;
        StopThinking();
    }
}

public struct AIStrategy
{
    public float winWeight;
    public float structWeight;
    public float blueprintWeight;
    public float destructionWeight;

    public AIStrategy(float winWeight_, float structWeight_, float blueprintWeight_, float destructionWeight_)
    {
        winWeight = winWeight_;
        structWeight = structWeight_;
        blueprintWeight = blueprintWeight_;
        destructionWeight = destructionWeight_;
    }
}

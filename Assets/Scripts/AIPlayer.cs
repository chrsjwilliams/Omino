using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * 
 *      TODO:
 *              Have a weight for each structure
 *              (*)Blueprint foresight
 *              (*)Have AI save destructors
 *              (*)Have AI know when it's in danger
 *              Discrete AI levels
 * 
 */

//  TODO:   AI KNOW WHEN IN DANGER
//
//          Have AI examine the area near its home base
//          if opponent is an a range of 6 away,
//          we are in danger
//          block base or perfrom a cut or destroy their piece

public class AIPlayer : Player
{
    public bool drawingPiece { get; protected set; }
    public bool playingPiece { get; protected set; }
    public bool isThinking { get; protected set; }
    public List<Coord> primaryTargets { get; protected set; }
    private const int rollsPerLevel = 2;
    private int level;
    private const int bestMovesCount = 20;
    private float baseBrickWorkRate;
    private float baseBarracksRate;
    private float baseSmithRate;

    private float futileBrickWorksPercentInc;
    private float futileSmithPercentInc;
    private float futileBarracksPercentInc;

    private int coordCountBuffer;
    private int movesTriedBuffer;
    private int tilesUntilBlueprint;
    private int totalRandomizations;
    private int kargerAlgorithmBuffer;

    protected float winWeight;
    protected float structWeight;
    protected float blueprintWeight;
    protected float destructionWeight;
    protected float blueprintdestructionWeight;
    protected float disconnectionWeight;
    protected float destructorForBlueprintWeight;
    private IEnumerator thinkingCoroutine;

    public override void Init(int playerNum_, AIStrategy strategy, int level_)
    {
        playingPiece = false;
        isThinking = false;
        drawingPiece = false;
        deckClumpCount = 4;

        movesTriedBuffer = 50;
        coordCountBuffer = 200;
        tilesUntilBlueprint = 5;
        totalRandomizations = 100;
        kargerAlgorithmBuffer = 10;

        level = level_;

        

        futileBrickWorksPercentInc = Factory.drawRateBonus * 0.25f;
        futileBarracksPercentInc = BombFactory.drawRateBonus * 0.25f;
        futileSmithPercentInc = Mine.resourceRateBonus * 0.25f;

        winWeight = strategy.winWeight;
        structWeight = strategy.structWeight;
        blueprintWeight = strategy.blueprintWeight;
        destructionWeight = strategy.destructionWeight;
        blueprintdestructionWeight = strategy.blueprintDestructionWeight;
        disconnectionWeight = strategy.disconnectionWeight;
        destructorForBlueprintWeight = strategy.destructorForBlueprintWeight;

        handSpacing = new Vector3(5.5f, -2.35f, 0);
        handOffset = new Vector3(-12.6f, 9.125f, 0);

        startingHandSize = 4;
        maxHandSize = 5;
        piecesPerHandColumn = 5;
        startingResources = 2;
        baseMaxResources = 3;
        boardPieces = new List<Polyomino>();
        resourceGainFactor = 1;
        drawRateFactor = 1;
        resourcesPerTick = 1;
        base.Init(playerNum_);
        Debug.Log("player " + playerNum + "using " + "\nwin weight: " + winWeight);
        Debug.Log("struct weight: " + structWeight + "\nblueprint weight: " + blueprintWeight);
        Debug.Log("destruction weight: " + destructionWeight +
                    "\nblueprint destruction weight: " + blueprintdestructionWeight); 
         Debug.Log("disconnection weight: " + disconnectionWeight + 
                    "\ndestructor4Blueprint weight: " + destructorForBlueprintWeight);


        //int normalPieceCost = biggerBricks ? 5 : 4;
        int normalPieceCost = 1;
        //int destructorCost = biggerBombs ? 4 : 3;
        int destructorCost = 1;

        baseBrickWorkRate = Factory.drawRateBonus / (normalDrawRate * normalPieceCost);
        baseBarracksRate = BombFactory.drawRateBonus / (destructorDrawRate * destructorCost);
        baseSmithRate = Mine.resourceRateBonus / resourceGainRate;
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

    public Graph MakeOpponentPieceGraph(List<Polyomino> currentAllBoardPieces)
    {
        Graph graph = new Graph();
        foreach (Polyomino piece in currentAllBoardPieces)
        {
            if (piece.owner != null &&
                piece.owner != this &&
                !(piece is Blueprint) &&
                (piece.connected || piece is Structure))
            {
                //  the first parameter is the vertex
                //  the second parameter are its adajcent verticies
                //List<Polyomino> adjPieces = piece.GetAdjacentPolyominos(piece.owner);
                List<Polyomino> adjPieces = piece.adjacentPieces;
                List<Edge<Polyomino>> pieceEdges = new List<Edge<Polyomino>>();
                foreach(Polyomino adjPiece in adjPieces)
                {
                    Edge<Polyomino> edge = null;
                    if (!graph.EdgeDict.ContainsKey(adjPiece))
                    {
                        edge = new Edge<Polyomino>(piece, adjPiece);
                        graph.Edges.Add(edge);
                    }
                    else
                    {
                        foreach(Edge<Polyomino> adjPieceEdge in graph.EdgeDict[adjPiece])
                        {
                            if(adjPieceEdge.curFirstVertex.Equals(piece) ||
                                adjPieceEdge.curSecondVertex.Equals(piece))
                            {
                                edge = adjPieceEdge;
                                break;
                            }
                        }
                    }
                    pieceEdges.Add(edge);
                }
                graph.EdgeDict.Add(piece, pieceEdges);
                graph.Vertices.Add(piece);
            }
        }

        //List<Polyomino> verticesToRemove = new List<Polyomino>();
        //foreach(Polyomino vertex in graph.Vertices)
        //{
        //    if(graph.EdgeDict[vertex].Count < 2)
        //    {
        //        verticesToRemove.Add(vertex);
        //    }
        //}

        //foreach(Polyomino vertex in verticesToRemove)
        //{
        //    if (graph.EdgeDict[vertex].Count > 0)
        //    {
        //        Edge<Polyomino> edge = graph.EdgeDict[vertex][0];

        //        Polyomino otherVertex;
        //        if (edge.curFirstVertex.Equals(vertex))
        //        {
        //            otherVertex = edge.curSecondVertex;
        //        }
        //        else
        //        {
        //            otherVertex = edge.curFirstVertex;
        //        }

        //        graph.EdgeDict[otherVertex].Remove(edge);
        //        graph.EdgeDict.Remove(vertex);
        //        graph.Edges.Remove(edge);
        //        graph.Vertices.Remove(vertex);
        //    }
        //}

        return graph;
    }


    public List<CutCoordSet> MakeLegalCuts(List<Cut> cuts)
    {
        List<CutCoordSet> legalCuts = new List<CutCoordSet>();
        //Debug.Log("logging cuts for player " + playerNum + " at time " + Time.time);
        foreach(Cut cut in cuts)
        {
            //Debug.Log("found cut of size: " + cut.size);
            //foreach (Edge<Polyomino> edge in cut.edges)
            //{
            //    Debug.Log("edge " + edge.originalFirstVertex.centerCoord +
            //        " to " + edge.originalSecondVertex.centerCoord);
            //}
            legalCuts.AddRange(GenerateCombinations(new List<CutCoordSet>(), cut));
        }

        return legalCuts;
    }
    
    protected List<CutCoordSet> GenerateCombinations(List<CutCoordSet> sets, Cut cutEdges)
    {
        if (sets.Count == 0) sets.Add(new CutCoordSet());

        List<CutCoordSet> newSets = new List<CutCoordSet>();
        
        if (cutEdges.edges.Count == 0) return sets;
        else 
        {
            Edge<Polyomino> edgeToBeRemoved = cutEdges.edges.ElementAt(0);

            Coord firstCoord = edgeToBeRemoved.originalFirstVertex.centerCoord;
            Coord secondCoord = edgeToBeRemoved.originalSecondVertex.centerCoord;

            bool includeFirstCoord = !(edgeToBeRemoved.originalFirstVertex is Structure);
            bool includeSecondCoord = !(edgeToBeRemoved.originalSecondVertex is Structure);

            foreach (CutCoordSet set in sets)
            {
                if (includeFirstCoord)
                {
                    if (!set.coords.Contains(firstCoord))
                    {
                        CutCoordSet newSet = new CutCoordSet(set.coords, cutEdges.size);
                        newSet.coords.Add(firstCoord);
                        newSets.Add(newSet);
                    }
                    else
                    {
                        newSets.Add(set);
                    }
                }
                if (includeSecondCoord)
                {
                    if (!set.coords.Contains(secondCoord))
                    {
                        CutCoordSet newSet = new CutCoordSet(set.coords, cutEdges.size);
                        newSet.coords.Add(secondCoord);
                        newSets.Add(newSet);
                    }
                    else
                    {
                        newSets.Add(set);
                    }
                }
            }

            List<CutCoordSet> setsToRemove = new List<CutCoordSet>();
            foreach(CutCoordSet set in newSets)
            {
                foreach(CutCoordSet otherSet in newSets)
                {
                    if (set.coords.IsSupersetOf(otherSet.coords) && set != otherSet)
                    {
                        setsToRemove.Add(set);
                        break;
                    }
                }
            }

            foreach(CutCoordSet set in setsToRemove)
            {
                newSets.Remove(set);
            }

            cutEdges.edges.Remove(edgeToBeRemoved);
            return GenerateCombinations(newSets, cutEdges);
        }
        
    }

    protected IEnumerator GeneratePossibleMoves()
    {
        //Debug.Log("starting to think at time " + Time.time);
        isThinking = true;
        List<Polyomino> currentHand = new List<Polyomino>(hand);
        List<Polyomino> currentBoardPieces = new List<Polyomino> (boardPieces);

        #region Successful Karger's Algorithm

        //  Collect the board pieces so I can make a graph of the opponent pieces
        int opposingIndex;
        if (playerNum == 1) opposingIndex = 1;
        else opposingIndex = 0;

        List<Polyomino> opposingBoardPieces =
                    new List<Polyomino>(Services.GameManager.Players[opposingIndex].boardPieces);

        //  These are the polyominos I can cut
        List<Cut> cuts = new List<Cut>();
        //Graph opponentGraph = MakeOpponentPieceGraph(opposingBoardPieces);
        for (int i = 0; i < totalRandomizations; i++)
        {
            if (i % kargerAlgorithmBuffer == 0)
            {
                yield return null;
            }
            //Graph tempGraph = new Graph(opponentGraph);
            Graph tempGraph = MakeOpponentPieceGraph(opposingBoardPieces);
            int cutSize = tempGraph.ApplyKarger();
            //Debug.Log("cut includes: ");
            //foreach (Edge<Polyomino> edge in opponentGraph.Edges)
            //{
            //    Coord firstEdgeNodeCoord = edge.originalFirstVertex.centerCoord;
            //    Coord secondEdgeNodeCoord = edge.originalSecondVertex.centerCoord;
            //    Debug.Log("edge from " + firstEdgeNodeCoord.x + "," +
            //        firstEdgeNodeCoord.y + " to " +
            //        secondEdgeNodeCoord.x + "," + secondEdgeNodeCoord.y);
            //}

            if (tempGraph.Edges.Count <= 3)
            {
                bool redundantCut = false;
                Cut newCut = new Cut(
                    new HashSet<Edge<Polyomino>>(tempGraph.Edges), 
                    cutSize);
                foreach (Cut cut in cuts)
                {
                    if (newCut.IsEquivalentTo(cut))
                    {
                        redundantCut = true;
                        break;
                    }
                }

                if (!redundantCut && newCut.edges.Count > 0)
                {
                    cuts.Add(newCut);
                }

            }
        }
        List<CutCoordSet> possibleCutMoves = MakeLegalCuts(cuts);

        //foreach (HashSet<Coord> set in possibleCutMoves)
        //{
        //    Debug.Log("---");
        //    foreach (Coord coord in set)
        //    {
        //        Debug.Log(coord);
        //    }
        //}
        //  We can then use the list of cuts and extract coords to pass along to our move 

        #endregion

        #region Calculate economy stuff

        float mineWeight = 0;
        float factoryWeight = 0;
        float bombFactoryWeight = 0;

        //int normalPieceCost = biggerBricks ? 5 : 4;
        int normalPieceCost = 1;
        //int destructorCost = biggerBombs ? 4 : 3;
        int destructorCost = 1;

        float normalPieceExpenditure = normalDrawRate * normalPieceCost;
        float destructivePieceExpenditure = destructorDrawRate * destructorCost;

        float expenditurePerSecond = normalPieceExpenditure + destructivePieceExpenditure;

        float brickWorksWeightMod = (Factory.drawRateBonus / normalPieceExpenditure) / baseBrickWorkRate;
        float barracksWeightMod = (BombFactory.drawRateBonus / destructivePieceExpenditure) / baseBarracksRate;
        float smithWeightMod = (Mine.resourceRateBonus / resourceGainRate) / baseSmithRate;

        float productionRatio = expenditurePerSecond / resourceGainRate;
        if (productionRatio > 1)
        {
            mineWeight = blueprintWeight * smithWeightMod;
        }
        else
        {
            factoryWeight = blueprintWeight * brickWorksWeightMod;
            bombFactoryWeight = blueprintWeight * barracksWeightMod;
        }

        #endregion

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
                                if (
                                    //!playableCoords.Contains(candidateCoord) &&
                                    containedInMap &&
                                    mapTile.occupyingPiece == null &&
                                    mapTile.occupyingStructure == null)
                                {
                                    playableCoords.Add(candidateCoord);
                                }

                                if (
                                    //!possibleBlueprintCoords.Contains(candidateCoord) &&
                                    containedInMap &&
                                    mapTile.occupyingStructure == null &&
                                    mapTile.occupyingBlueprint == null)
                                {
                                    possibleBlueprintCoords.Add(candidateCoord);
                                }
                            }

                            if (
                                //!touchableCoords.Contains(candidateCoord) &&
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
        playableCoords = playableCoords.Distinct().ToList();
        possibleBlueprintCoords = new HashSet<Coord>(
            possibleBlueprintCoords.Distinct());
        touchableCoords = touchableCoords.Distinct().ToList();
        //Debug.Log("Counted Blueprint Coords: " + countedBlueprintCoords);
        #endregion

        //  Finding tiles that could possibly adjacent pieces AI cares about
        List<Polyomino>[,] adjPiecesByCoord = new List<Polyomino>[
            Services.MapManager.MapWidth,
            Services.MapManager.MapHeight];

        List<Move> movesToConsider = new List<Move>();
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

                        if (missingCoordHashSet.Count > 0  && missingCoordHashSet.Count() <= tilesUntilBlueprint )
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
                                //HashSet<Coord> pieceCoords = new HashSet<Coord>();
                                //foreach (Tile tile in piece.tiles)
                                //{
                                //    if (!pieceCoords.Contains(tile.coord))
                                //    {
                                //        pieceCoords.Add(tile.coord);
                                //    }
                                //}

                                Move newMove = new Move(piece, playableCoords[i], (rotations + 1) % 4, 
                                                        possibleBlueprintMoves, possibleCutMoves,
                                                        winWeight, structWeight, destructionWeight, 
                                                        mineWeight, factoryWeight, bombFactoryWeight);
                                
                                //if (nextPlay == null) nextPlay = newMove;
                                //else if (newMove.score > nextPlay.score)
                                //{
                                //    nextPlay = newMove;
                                //}
                                if(movesToConsider.Count < bestMovesCount ||
                                    newMove.score > movesToConsider[0].score)
                                {
                                    movesToConsider = InsertMoveIntoSortedList(
                                        newMove, movesToConsider);
                                    if(movesToConsider.Count > bestMovesCount)
                                    {
                                        movesToConsider.RemoveAt(0);
                                    }
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
        // Choose a play by rolling
        if (movesToConsider.Count > 0) {
            //Debug.Log("player " + playerNum + " considering moves of score:");
            //foreach(Move move in movesToConsider)
            //{
            //    Debug.Log(move.score);
            //}
            for (int i = 0; i < level * rollsPerLevel; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, movesToConsider.Count);
                Move potentialMove = movesToConsider[randomIndex];
                if(nextPlay == null || potentialMove.score > nextPlay.score)
                {
                    nextPlay = potentialMove;
                }
            }
            //Debug.Log("picking move of score " + nextPlay.score);
        }
        if (nextPlay != null && nextPlay.score > 0)
        {
            MakePlay(nextPlay);
        }
        isThinking = false;
    }

    private List<Move> InsertMoveIntoSortedList(Move move, List<Move> sortedMoveList)
    {
        int startIndex = 0;
        int endIndex = sortedMoveList.Count;
        while (endIndex > startIndex)
        {
            int windowSize = endIndex - startIndex;
            int middleIndex = startIndex + windowSize / 2;
            float middleScore = sortedMoveList[middleIndex].score;
            if(move.score < middleScore)
            {
                endIndex = middleIndex;
            }
            else
            {
                startIndex = middleIndex + 1;
            }
        }
        sortedMoveList.Insert(startIndex, move);
        return sortedMoveList;
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

    public override void OnOpposingPiecePlaced(Polyomino piece)
    {
        base.OnOpposingPiecePlaced(piece);
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
        //StopThinking();
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
    public float blueprintDestructionWeight;
    public float disconnectionWeight;
    public float destructorForBlueprintWeight;

    public AIStrategy(  float winWeight_, float structWeight_, float blueprintWeight_, float destructionWeight_,
                        float blueprintDestructionWeight_, float disconnectionWeight_, float destructorForBlueprintWeight_)
    {
        winWeight = winWeight_;
        structWeight = structWeight_;
        blueprintWeight = blueprintWeight_;
        destructionWeight = destructionWeight_;
        blueprintDestructionWeight = blueprintDestructionWeight_;
        disconnectionWeight = disconnectionWeight_;
        destructorForBlueprintWeight = destructorForBlueprintWeight_;
    }
}

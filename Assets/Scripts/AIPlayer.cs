using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *      AI TODO:
 *                  AI tries to build blueprints
 *                  Needs to know geometry of pieces on board
 * 
 */ 


/*
 *      Things AI needs to know:
 *              I need to know if this move would result in a successful blueprint
 * 
 * 
 */ 


/*
 *      I need a hashset of coords a blueprint could go
 * 
 * 
 */ 

public class AIPlayer : Player
{
    public bool drawingPiece { get; protected set; }
    public bool playingPiece { get; protected set; }
    public bool isThinking { get; protected set; }
    public Coord primaryTarget { get; protected set; }

    private int frameBuffer;
    private int moveCountBuffer;
    private int coordCountBuffer;
    private int movesTriedBuffer;
    private int blueprintMapsTriedBuffer;
    private int tilesUntilBlueprint;

    protected float winWeight;
    protected float structWeight;
    private IEnumerator thinkingCoroutine;

    public struct MoveData
    {
        public Polyomino piece;
        public Coord targetCoord;
        public int rotations;
        public BlueprintMap blueprintMap;
        
        public MoveData(Polyomino _piece, Coord _coord, int _rotations, BlueprintMap _blueprintMap)
        {
            piece = _piece;
            targetCoord = _coord;
            rotations = _rotations;
            blueprintMap = _blueprintMap;
        }
    }

    public override void Init(Color[] playerColorScheme, int posOffset, float _winWeight, float _structureWeight)
    {
        playingPiece = false;
        isThinking = false;
        drawingPiece = false;
        deckClumpCount = 4;

        frameBuffer = 1;
        movesTriedBuffer = 50;
        moveCountBuffer = 300;
        coordCountBuffer = 200;
        blueprintMapsTriedBuffer = 500;
        tilesUntilBlueprint = 5;

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
            thinkingCoroutine = GeneratePossibleMoves();
            StartCoroutine(thinkingCoroutine);
        }
    }

    // I need to run the blueprint check for every move


    protected IEnumerator GeneratePossibleMoves()
    {
        List<Polyomino> currentHand = new List<Polyomino>(hand);
        List<Polyomino> currentBoardPieces = new List<Polyomino> (boardPieces);

        isThinking = true;
        //  Finding tiles that my center coord could be placed
        #region Gets Playable Coords
        //List<Coord> playableCoords = FindAllPlayableCoords(2, true);
        List<Coord> playableCoords = new List<Coord>();
        List<BlueprintMap> possibleBlueprintCoords = new List<BlueprintMap>();
        HashSet<Coord> boardPieceHashSet = new HashSet<Coord>();

        //  Finding possibile center coords for pieces
        int countedPlayableCoords = 0;
        int countedBlueprintCoords = 0;
        int framesTaken = 0;
        foreach (Polyomino piece in currentBoardPieces)
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

                        if (countedPlayableCoords % coordCountBuffer == 0)
                        {
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

            if (!(piece is Base) && !(piece is Structure))
            {
                foreach (Tile tile in piece.tiles)
                {
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            countedBlueprintCoords++;

                            Coord radiusOffset = new Coord(0, 0);
                            Coord candidateCoord = tile.coord.Add(radiusOffset);

                            if (countedPlayableCoords % coordCountBuffer == 0)
                            {
                                yield return null;
                            }

                            if (!boardPieceHashSet.Contains(candidateCoord) &&
                                Services.MapManager.IsCoordContainedInMap(candidateCoord) &&
                                tile.occupyingBlueprint == null)
                            {

                                boardPieceHashSet.Add(candidateCoord);
                            }
                        }
                    }
                }
            }
        }

        //Debug.Log("Counted Blueprint Coords: " + countedBlueprintCoords);
        #endregion

        //  Finding tiles that could possibly adjacent pieces AI cares about
        #region Gets Touchable Coords 
        //List<Coord> touchableCoords = FindAllPlayableCoords(4, false);
        List<Coord> touchableCoords = new List<Coord>();

        int countedTouchedCoords = 0;
        foreach (Polyomino piece in currentBoardPieces)
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

                        if (countedTouchedCoords % coordCountBuffer == 0)
                        {
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
                yield return null;
            }
        }

        #region Blueprint Placement Logic
        int blueprintsTried = 0;
        foreach (Blueprint blueprint in blueprints)
        { 
            Coord roundedPos = new Coord((int)blueprint.holder.transform.position.x,
                                            (int)blueprint.holder.transform.position.y);

            blueprint.SetTileCoords(roundedPos);
            int numRotations = blueprint.maxRotations;
            foreach (Coord coord in boardPieceHashSet)
            {
                for (int rotations = 0; rotations < 4; rotations++)
                {
                    HashSet<Coord> blueprintHashset = new HashSet<Coord>();
                    blueprint.Rotate(false, true, true);
                    //  For play positions +/- a pieces radius
                    blueprint.SetTileCoords(coord);
                    blueprint.TurnOffGlow();

                    foreach (Tile tile in blueprint.tiles)
                    {
                        if (!blueprintHashset.Contains(tile.coord) && 
                            Services.MapManager.IsCoordContainedInMap(tile.coord) &&
                            !(Services.MapManager.Map[tile.coord.x, tile.coord.y].occupyingPiece is Base) &&
                            !(Services.MapManager.Map[tile.coord.x, tile.coord.y].occupyingPiece is Structure) &&
                            Services.MapManager.Map[tile.coord.x, tile.coord.y].occupyingBlueprint == null)
                        {
                            blueprintHashset.Add(tile.coord);
                        }
                    }
           
                    if(blueprintHashset.Count == blueprint.tiles.Count)
                    { 
                        HashSet<Coord> missingCordHashSet = new HashSet<Coord>(blueprintHashset.Except(boardPieceHashSet));

                        if (rotations < numRotations)
                        {
                            blueprintsTried++;
                            if (blueprintsTried % movesTriedBuffer == 0)
                            {
                                yield return null;
                            }

                            if (missingCordHashSet.Count() <= tilesUntilBlueprint)
                            {
                                BlueprintMap blueprintMap = new BlueprintMap(blueprint, missingCordHashSet, coord, (rotations + 1) % 4);
                                possibleBlueprintCoords.Add(blueprintMap);
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("Blueprints Tried: " + blueprintsTried);
        #endregion

        #region Polyomino Placement Logic
        int movesTried = 0;
        int blueprintMapsTried = 0;
        BlueprintMap selectedBluePrintMap = new BlueprintMap();
        foreach (Polyomino piece in currentHand)
        {
            if (piece.cost <= resources)
            {
                //  Check the target coord and center coord
                Coord roundedPos = new Coord((int)piece.holder.transform.position.x, 
                                             (int)piece.holder.transform.position.y);

                piece.SetTileCoords(roundedPos);

                int numRotations = Polyomino.peiceRotationDictionary[piece.tiles.Count][piece.index];

                for (int i = 0; i < playableCoords.Count; i++)
                {
                    // each piece should know how many times it can be rotated
                    for (int rotations = 0; rotations < 4; rotations++)
                    {
                        piece.Rotate(false, true, true);
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
                                    adjacentPolyominos.AddRange(adjPiecesByCoord[tile.coord.x, tile.coord.y]);
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

                                foreach (BlueprintMap blueprintMap in possibleBlueprintCoords)
                                {
                                    if (pieceCoords.IsSupersetOf(blueprintMap.missingCoords) || blueprintMap.missingCoords.Count == 0)
                                    {
                                        selectedBluePrintMap = blueprintMap;
                                        break;
                                    }

                                    blueprintMapsTried++;
                                    if (blueprintMapsTried % blueprintMapsTriedBuffer == 0)
                                    {
                                        yield return null;
                                    }

                                    #region Smart Blueprint Placement Test
                                    /*
                                    //  find which missing peice I'm a superset of
                                    moveBlueprintMap = blueprintMap;
                                    int numFilledCoords = moveBlueprintMap.coords.Intersect(pieceCoords).Count();
                                    if(numFilledCoords > moveBlueprintMap.numCoordsFilled)
                                    {
                                        moveBlueprintMap = blueprintMap;
                                        //   we can do other things here that distinguish between blueprints
                                        if (numFilledCoords == piece.tiles.Count) break;
                                    }
                                    */
                                    #endregion
                                }

                                MoveData moveData = new MoveData(piece, playableCoords[i], (rotations + 1) % 4, selectedBluePrintMap);
                                Move newMove = new Move(moveData, winWeight, structWeight);
                                
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

        Debug.Log("BlueprintMaps Tried: " + blueprintMapsTried);

        if (nextPlay != null)
        {
            MakePlay(nextPlay);
        }
        isThinking = false;
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

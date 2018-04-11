using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class Move
{
    public float score;
    public const int MAX_SCORE = 100;
    public const int MAX_ROTATIONS = 3;

    //private AdjacencyListGraph<Tile,Edge<bool>> opponentPieces;

    public Move blueprintMove { get; private set; }

    //public Dictionary<Tile, Coord> relativeCoords { get; private set; }
    public Polyomino piece { get; private set; }
    public Coord targetCoord { get; private set; }
    public int rotations { get; private set; }

    private List<BlueprintMap> possibleBlueprintMoves;
    private List<CutCoordSet> possibleCutMoves;

    float blueprintDestructionWeight = 0.2f;
    float disconnectionWeight = 0.4f;

    float normalPieceForBlueprintWeight = 1;
    float destructorForBlueprintWeight = 4.0f;
    public int finalCutSize;

    public Move(Polyomino _piece, Coord _targetCoord, int _rotations,
        List<BlueprintMap> _possibleBlueprintMoves,
        List<CutCoordSet> _possibleCutMoves, float winWeight,
        float structureWeight, float destructionWeight, float mineWeight,
        float factoryWeight, float bombFactoryWeight)
    {
        piece = _piece;
        //relativeCoords = piece.tileRelativeCoords;
        targetCoord = _targetCoord;
        rotations = _rotations;
        possibleBlueprintMoves = _possibleBlueprintMoves;
        possibleCutMoves = _possibleCutMoves;
        blueprintMove = null;
        score = CalculateScore(winWeight, structureWeight, destructionWeight, mineWeight, factoryWeight, bombFactoryWeight);
    }

    public Move(Blueprint blueprint, Coord _targetCoord, int _rotations)
    {
        piece = blueprint;
        targetCoord = _targetCoord;
        rotations = _rotations;
    }

    public float CalculateScore(float winWeight, float structWeight, float destructionWeight, float mineWeight, float factoryWeight, float bombFactoryWeight)
    {

        HashSet<Coord> pieceCoords = new HashSet<Coord>();
        foreach (Tile tile in piece.tiles)
        {
            pieceCoords.Add(tile.relativeCoord.Add(targetCoord));
        }
        pCoord = pieceCoords;
        #region OLD BLUEPRINT CREATION TECHNIQUE
        Move mineMove = null;
        Move factoryMove = null;
        Move bombFactoryMove = null;
        #endregion

        BlueprintMap closestSmithMap = null;
        BlueprintMap closestBrickworksMap = null;
        BlueprintMap closestBarracksMap = null;

        int smithCoordDifference = int.MaxValue;
        int brickworksCoordDifference = int.MaxValue;
        int barracksCoordDifference = int.MaxValue;

        foreach (BlueprintMap blueprintMap in possibleBlueprintMoves)
        {
            HashSet<Coord> relativeComplement = new HashSet<Coord>(blueprintMap.missingCoords);
            relativeComplement.ExceptWith(pieceCoords);

            if (blueprintMap.blueprint is Mine)
            {
                if (relativeComplement.Count < smithCoordDifference)
                {
                    closestSmithMap = blueprintMap;
                    smithCoordDifference = relativeComplement.Count;
                }
            }
            else if (blueprintMap.blueprint is Factory)
            {
                if (relativeComplement.Count < brickworksCoordDifference)
                {
                    closestBrickworksMap = blueprintMap;
                    brickworksCoordDifference = relativeComplement.Count;
                }
            }
            else if (blueprintMap.blueprint is BombFactory)
            {
                if (relativeComplement.Count < barracksCoordDifference)
                {
                    closestBarracksMap = blueprintMap;
                    barracksCoordDifference = relativeComplement.Count;
                }
            }

            #region OLD BLUEPRINT CREATION TECHNIQUE
            /*
            if (pieceCoords.IsSupersetOf(blueprintMap.missingCoords))
            {
                if (blueprintMap.blueprint is Mine)
                {
                    mineMove = new Move(blueprintMap.blueprint, blueprintMap.targetCoord, blueprintMap.rotations);
                }
                else if(blueprintMap.blueprint is Factory)
                {
                    factoryMove = new Move(blueprintMap.blueprint, blueprintMap.targetCoord, blueprintMap.rotations);
                }
                else if (blueprintMap.blueprint is BombFactory)
                {
                    bombFactoryMove = new Move(blueprintMap.blueprint, blueprintMap.targetCoord, blueprintMap.rotations);
                }
                if (mineMove != null && factoryMove != null && bombFactoryMove != null) break;

            }
            */
            #endregion
        }

        float score = 0;
        if (piece is Destructor && piece.owner.splashDamage)
        {
            score = DestructionScore(destructionWeight, pieceCoords);
        }
        else
        {
            score = WinAndStructScore(winWeight, structWeight) +
                BlueprintScore(closestSmithMap, closestBrickworksMap,
                                closestBarracksMap, smithCoordDifference,
                                brickworksCoordDifference, barracksCoordDifference,
                                mineWeight, factoryWeight, bombFactoryWeight) +
                DestructionScore(destructionWeight, pieceCoords);
        }

        return score;

    }

    private float BlueprintScore(   BlueprintMap smithMap, BlueprintMap brickWorksMap, 
                                    BlueprintMap barracksMap, int missingSmithCoords, 
                                    int missingBrickWorksCoords, int missingBarracksCoords, 
                                    float mineWeight, float factoryWeight, float bombFactoryWeight)
    {
        if (piece is Blueprint) return 0;
        float destructorModifier = piece is Destructor ? destructorForBlueprintWeight : 2;

        Move mineMove = null;
        Move factoryMove = null;
        Move bombFactoryMove = null;

        float smithScore;
        float brickWorksScore;
        float barracksScore;

        float smithBlueprintMod;
        float brickWorksBlueprintMod;
        float barracksBlueprintMod;

        float blueprintScore = 0;
        if (missingSmithCoords == 0)
        {
            mineMove = new Move(smithMap.blueprint, smithMap.targetCoord, smithMap.rotations);
        }
        else if (missingBrickWorksCoords == 0)
        {
            factoryMove = new Move(brickWorksMap.blueprint, brickWorksMap.targetCoord, brickWorksMap.rotations);
        }
        else if (missingBarracksCoords == 0)
        {
            bombFactoryMove = new Move(barracksMap.blueprint, barracksMap.targetCoord, barracksMap.rotations);
        }

        smithBlueprintMod = Mathf.Pow(1 /destructorModifier, missingSmithCoords);
        brickWorksBlueprintMod = Mathf.Pow(1 / destructorModifier, missingBrickWorksCoords);
        barracksBlueprintMod = Mathf.Pow(1 / destructorModifier, missingBarracksCoords);

        smithScore = mineWeight * smithBlueprintMod;
        brickWorksScore = factoryWeight * brickWorksBlueprintMod;
        barracksScore = bombFactoryWeight * barracksBlueprintMod;
        
        if (smithScore > blueprintScore)
        {
            blueprintScore = smithScore;
            if (mineMove != null)
            {
                blueprintMove = mineMove;
            }
        }
        if (brickWorksScore > blueprintScore)
        {
            blueprintScore = brickWorksScore;
            if (factoryMove != null)
            {
                blueprintMove = factoryMove;
            }
        }
        if (barracksScore > blueprintScore)
        {
            blueprintScore = barracksScore;
            if (bombFactoryMove != null)
            {
                blueprintMove = bombFactoryMove;
            }
        }
        return blueprintScore;
    }

    private float DestructionScore(float destructionWeight, HashSet<Coord> pieceCoords)
    {
        if (!(piece is Destructor)) return 0;
        else
        {
            List<Blueprint> blueprintsDestroyed = new List<Blueprint>();
            int tilesIdestroy = 0;
            int destructionRange = piece.owner.splashDamage ? 1 : 0;
            int cutSize = 0;
            HashSet<Coord> coordsToBeDestroyed = new HashSet<Coord>();
            
            foreach (Tile tile in piece.tiles)
            {
                for (int x = -destructionRange; x <= destructionRange; x++)
                {
                    for (int y = -destructionRange; y <= destructionRange; y++)
                    {
                        Coord destructionRadius = new Coord(x, y);
                        Coord coordToBeDestroyed = tile.coord.Add(destructionRadius);
                        coordsToBeDestroyed.Add(coordToBeDestroyed);
                        if (Services.MapManager.IsCoordContainedInMap(coordToBeDestroyed))
                        {
                            Tile mapTile = Services.MapManager.Map[coordToBeDestroyed.x, coordToBeDestroyed.y];
                            if( mapTile.occupyingPiece!= null &&
                                mapTile.occupyingPiece.owner != null &&
                                mapTile.occupyingPiece.owner != piece.owner &&
                                !(mapTile.occupyingPiece is Structure))
                            {
                                tilesIdestroy++;
                                Blueprint blueprint = mapTile.occupyingBlueprint;
                                if( blueprint != null &&
                                    !blueprintsDestroyed.Contains(blueprint))
                                {
                                    blueprintsDestroyed.Add(blueprint);
                                }
                            }
                        }
                    }
                }
                #region Dumb Cut Hueristic
                /*
                if (tilesIDestroy > 1 && !cutsOffOpponent)
                {
                    int numberOfOpponentNeighbors = 0;
                    foreach (Coord dir in Coord.Directions())
                    {
                        Coord newCoord = tile.coord.Add(dir);
                        if (Services.MapManager.IsCoordContainedInMap(newCoord))
                        {
                            Tile mapTile = Services.MapManager.Map[newCoord.x, newCoord.y];
         
                            if (mapTile.occupyingPiece != null &&
                                mapTile.occupyingPiece.owner != null &&
                                mapTile.occupyingPiece.owner != piece.owner)
                            {
                                numberOfOpponentNeighbors++;
                            }
                        }
                    }

                    if (numberOfOpponentNeighbors == 2)
                    {
                        cutsOffOpponent = true;
                    }
                }
                */
                #endregion
            }

            foreach (CutCoordSet cutSet in possibleCutMoves)
            {
                if (coordsToBeDestroyed.IsSupersetOf(cutSet.coords))
                {
                    if (cutSet.size > cutSize)
                    {
                        cutSize = cutSet.size;
                        Debug.Log("move has cutsize score of " + cutSize);
                    }
                }
            }

            float destructionMod = tilesIdestroy > 0 ? 1 : 0.75f;
            finalCutSize = cutSize;
            float blueprintDestructionScore = blueprintsDestroyed.Count * blueprintDestructionWeight;
            float disconnectionScore = cutSize * disconnectionWeight;

            return destructionWeight * (blueprintDestructionScore + disconnectionScore) * destructionMod;
        }
    }
    
    private float WinAndStructScore(float winWeight, float structWeight)
    {
        float structDist = float.MaxValue;
        float pieceDistFromTarget = float.MaxValue;
        foreach (Tile tile in piece.tiles)
        {
            Coord tileRelCoord = tile.relativeCoord;
            foreach (Coord coord in ((AIPlayer)piece.owner).primaryTargets)
            {
                float tileDistFromTarget = coord.Distance(tileRelCoord.Add(targetCoord));

                if (tileDistFromTarget < pieceDistFromTarget)
                {
                    pieceDistFromTarget = tileDistFromTarget;
                }
            }

            foreach (Coord coord in Services.MapManager.structureCoords)
            {
                if (Services.MapManager.Map[coord.x, coord.y].occupyingStructure.owner == null) {
                    float testDistance = coord.Distance(tileRelCoord.Add(targetCoord));

                    if (testDistance < structDist)
                    {
                        structDist = testDistance;
                    }
                }
            }

        }

        float winScore;
        float structScore;
        if (pieceDistFromTarget == 0)
        {
            winScore = winWeight;
        }
        else
        {
            winScore = winWeight / Mathf.Pow(pieceDistFromTarget, 2);
        }
        if(structDist == 0)
        {
            structScore = structWeight;
        }
        else
        {
            structScore = structWeight / Mathf.Pow(structDist, 2);
        }
        return winScore + structScore;
    }


    HashSet<Coord> rComp = new HashSet<Coord>();
    HashSet<Coord> pCoord = new HashSet<Coord>();

    public void ExecuteMove()
    {
        Task playTask;
        if (blueprintMove == null)
        {
            playTask = new PlayTask(this);
        }
        else
        {        
            playTask = new PlayTask(this);
            playTask.Then(new ActionTask(blueprintMove.ExecuteMove));
        }
        Services.GeneralTaskManager.Do(playTask);
        //Debug.Log("player " + piece.owner.playerNum + " making move of score: " + score + " at time " + Time.time);
        //if (finalCutSize > 0) Debug.Log("making cut of size" + finalCutSize);
        //if(!(piece is Blueprint))
        //{
        //    Debug.Log("player " + piece.owner.playerNum + " playing move with score:" + score +
        //        ", winScore: " + finalWinScore + ", structScore: " + finalStructScore +
        //        ", blueprintScore: " + finalBlueprintScore);
        //}
    }
}

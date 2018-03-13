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
    private float finalWinScore;
    private float finalStructScore;
    private float finalBlueprintScore;

    //private AdjacencyListGraph<Tile,Edge<bool>> opponentPieces;

    public Move blueprintMove { get; private set; }

    //public Dictionary<Tile, Coord> relativeCoords { get; private set; }
    public Polyomino piece { get; private set; }
    public Coord targetCoord { get; private set; }
    public int rotations { get; private set; }

    private List<BlueprintMap> possibleBlueprintMoves;

    public Move(Polyomino _piece, Coord _targetCoord, int _rotations, List<BlueprintMap> _possibleBlueprintMoves, float winWeight, 
        float structureWeight, float destructionWeight, float mineWeight, float factoryWeight, float bombFactoryWeight)
    {
        piece = _piece;
        //relativeCoords = piece.tileRelativeCoords;
        targetCoord = _targetCoord;
        rotations = _rotations;
        possibleBlueprintMoves = _possibleBlueprintMoves;
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
        foreach(Tile tile in piece.tiles)
        {
            pieceCoords.Add(tile.relativeCoord.Add(targetCoord));
        }

        Move mineMove = null;
        Move factoryMove = null;
        Move bombFactoryMove = null;

        foreach (BlueprintMap blueprintMap in possibleBlueprintMoves)
        {
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
        }
        

        return  WinAndStructScore(winWeight, structWeight) + 
                BlueprintScore(mineMove, factoryMove, bombFactoryMove, mineWeight,
                                factoryWeight, bombFactoryWeight) +
                DestructionScore(destructionWeight, pieceCoords);
    }

    private float BlueprintScore(Move mineMove, Move factoryMove, Move bombFactoryMove, float mineWeight, float factoryWeight,
        float bombFactoryWeight)
    {
        float destructorModifier = 1;
        if(piece is Destructor)
        {
            destructorModifier = 0.5f;
        }

        float blueprintScore = 0;
        if (mineMove != null)
        {
            blueprintScore = mineWeight * destructorModifier;
            blueprintMove = mineMove;
        }
        if (factoryMove != null && factoryWeight > blueprintScore)
        {
            blueprintScore = factoryWeight * destructorModifier;
            blueprintMove = factoryMove;
        }
        if (bombFactoryMove != null && bombFactoryWeight > blueprintScore)
        {
            blueprintScore = bombFactoryWeight * destructorModifier;
            blueprintMove = bombFactoryMove;
        }
        finalBlueprintScore = blueprintScore;
        return blueprintScore;

    }

    private float DestructionScore(float destructionWeight, HashSet<Coord> pieceCoords)
    {
        if (!(piece is Destructor)) return 0;
        else
        {

            List<Coord> tileCoordsIDestroy = new List<Coord>();
            int tilesIDestroy = 0;
            List<Blueprint> blueprintsDestroyed = new List<Blueprint>();
            int destructionRange = piece.owner.splashDamage ? 1 : 0;
            bool bisectsOpponentsPieces = false;
            
            foreach (Tile tile in piece.tiles)
            {
                for (int x = -destructionRange; x < destructionRange; x++)
                {
                    for (int y = -destructionRange; y < destructionRange; y++)
                    {
                        Coord destructionRadius = new Coord(x, y);
                        Coord coordToBeDestroyed = tile.coord.Add(destructionRadius);

                        if (Services.MapManager.IsCoordContainedInMap(coordToBeDestroyed))
                        {
                            Tile mapTile = Services.MapManager.Map[coordToBeDestroyed.x, coordToBeDestroyed.y];
                            if( mapTile.occupyingPiece!= null &&
                                mapTile.occupyingPiece.owner != null &&
                                mapTile.occupyingPiece.owner != piece.owner &&
                                !(mapTile.occupyingPiece is Structure))
                            {
                                tilesIDestroy++;
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

                if (tilesIDestroy > 1 && !bisectsOpponentsPieces)
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
                        bisectsOpponentsPieces = true;
                    }
                }   
            }
            
            float tileDestructionWeight = tilesIDestroy * 0.05f;
            float blueprintDestructionWeight = blueprintsDestroyed.Count * 0.2f;
            float disconnectionWeight = bisectsOpponentsPieces ? 0.3f : 0;

            return destructionWeight + tileDestructionWeight + blueprintDestructionWeight + disconnectionWeight;
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
        finalWinScore = winScore;
        finalStructScore = structScore;
        return winScore + structScore;
    }

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
        //if(!(piece is Blueprint))
        //{
        //    Debug.Log("player " + piece.owner.playerNum + " playing move with score:" + score +
        //        ", winScore: " + finalWinScore + ", structScore: " + finalStructScore +
        //        ", blueprintScore: " + finalBlueprintScore);
        //}
    }
}

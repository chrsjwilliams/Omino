using System.Collections.Generic;
using BeatManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum BuildingType
{
    BASE = -5, FACTORY = -4, GENERATOR = -3, BARRACKS = -2, EDITMODE = -1, NONE = 0,
    DYNAMO, SUPPLYBOOST, UPSIZE, ATTACKUPSIZE, COMBUSTION, SHIELDEDPIECES,
    ARMORY, FISSION, RECYCLING, CROSSSECTION, REPAINT, RETALIATE, RECOUP, PLUNDER
}

public class Polyomino : IVertex
{
    public bool isTerrain { get; protected set; }
    public bool destructible = true;
    public List<Tile> tiles = new List<Tile>();
    public List<Coord> pieceCoords = new List<Coord>();
    protected string holderName;
    public PieceHolder holder { get; protected set; }
    public BuildingType buildingType { get; protected set; }
    public static int[][] pieceRotationDictionary = new int[][]
    {
        new int []{ },
        new int [1] {1},
        new int [1] {2},
        new int [2] {2, 4},
        new int [7] {2, 4, 4, 4, 2, 2, 1},
        new int [12] {4, 2, 4, 4, 4, 4, 4, 4, 4, 1, 4, 2}

    };

    public int index { get; protected set; }
    public int units { get; protected set; }
    protected int[,,] piece;
    public Player owner { get; protected set; }
    public Color baseColor { get; private set; }
    public Coord centerCoord;
    public bool placed { get; protected set; }
    private const float rotationInputRadius = 8f;
    private const float rotationDeadZone = 50f;
    protected int touchID;
    protected readonly Vector3 baseDragOffset = 20f * Vector3.up;
    private static Vector3 unselectedScale = 0.5f * Vector3.one;
    public static Vector3 UnselectedScale {
        get
        {
            if (Services.GameManager != null && Services.GameManager.onIPhone)
            {
                return 0.6f * Vector3.one;
            }
            else
            {
                return 0.5f * Vector3.one;
            }
        }
    }
    public static Vector3 QueueScale
    {
        get
        {
            if (Services.GameManager.onIPhone)
            {
                return 0.3f * Vector3.one;
            }
            else
            {
                return 0.25f * Vector3.one;
            }
        
        }
    }
    public const float drawAnimDur = 0.5f;
    public const float deathAnimDur = 0.5f;
    public const float deathAnimScaleUp = 1.5f;
    private const float handPosApproachFactor = 0.25f;
    public const float burnPieceDuration = 0.5f;
    private const float alphaWhileUnaffordable = 0.8f;
    private const float alphaWhileAffordable = 1f;
    private List<TechBuilding> highlightedStructures;
    public List<Polyomino> piecesToRemove { get; private set; }

    public List<Blueprint> occupyingBlueprints { get; protected set; }
    public int cost { get; protected set; }
    private bool affordable;
    public bool connected { get; protected set; }
    public bool dead { get; private set; }
    public float shieldDurationRemaining { get; private set; }
    protected List<Tooltip> tooltips;
    protected int numRotations;
    protected Queue<Coord> lastPositions;
    private const int framesBeforeLockIn = 10;
    private const int leniencyFrames = 5;
    public bool burningFromHand { get; private set; }

    public List<Polyomino> adjacentPieces;

    protected List<MapTile> previouslyHoveredMapTiles;

    protected static int[,,] monomino = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            }
    };

    protected static int[,,] domino = new int[1, 5, 5]
        { 
            //  ##
            {
                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,0,1,1,0 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            }
        };

    protected static int[,,] triomino = new int[2, 5, 5]
        { 
            //  ###
            {
                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,1,1,1,0 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            },
            //  #
            //  ##
            {

                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,1,0 },
                { 0,0,0,0,0 }
            }
        };

    protected static int[,,] tetromino = new int[7, 5, 5]
        { 
            //  ####
            {
                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,1,1,1,1 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            },
            //  #
            //  #
            //  ##
            {
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,1,0 },
                { 0,0,0,0,0 }
            },
            //  #
            //  #
            // ##
            {
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,0,0 },
                { 0,1,1,0,0 },
                { 0,0,0,0,0 }
            },
            //  #
            //  ##
            //  #
            {
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,0,1,1,0 },
                { 0,0,1,0,0 },
                { 0,0,0,0,0 }
            },
            //  ##
            //   ##
            {
                { 0,0,0,0,0 },
                { 0,1,1,0,0 },
                { 0,0,1,1,0 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            },
            //   ##
            //  ##
            {
                { 0,0,0,0,0 },
                { 0,0,1,1,0 },
                { 0,1,1,0,0 },
                { 0,0,0,0,0 },
                { 0,0,0,0,0 }
            },
            //  ##
            //  ##
            {
                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,0,1,1,0 },
                { 0,0,1,1,0 },
                { 0,0,0,0,0 }
            }
        };

    protected static int[,,] pentomino = new int[12, 5, 5]
        { 
            //  F Shape
            //   ##
            //  ##
            //   #
            {
                {0,0,0,0,0 },
                {0,0,1,1,0 },
                {0,1,1,0,0 },
                {0,0,1,0,0 },
                {0,0,0,0,0 }
            },
            //  I Shape
            //  #
            //  #
            //  #
            //  #
            //  #
            {
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {1,1,1,1,1 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  L Shape
            //  #
            //  #
            //  #
            //  ##
            {
                {0,0,1,0,0 },
                {0,0,1,0,0 },
                {0,0,1,0,0 },
                {0,0,1,1,0 },
                {0,0,0,0,0 }
            },
            //  N Shape
            //  ###
            //    ##
            {
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {1,1,1,0,0 },
                {0,0,1,1,0 },
                {0,0,0,0,0 }
            },
            //  P Shape
            //  ##
            //  ##
            //  #
            {
                {0,0,0,0,0 },
                {0,0,1,1,0 },
                {0,0,1,1,0 },
                {0,0,1,0,0 },
                {0,0,0,0,0 }
            },
            //  T Shape
            //  ###
            //   #
            //   #
            {
                {0,0,0,0,0 },
                {0,1,1,1,0 },
                {0,0,1,0,0 },
                {0,0,1,0,0 },
                {0,0,0,0,0 }
            },
            //  U Shape
            //  # #
            //  ###
            {
                {0,0,0,0,0 },
                {0,1,0,1,0 },
                {0,1,1,1,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  V Shape
            //  #
            //  #
            //  ###
            {
                {0,0,0,0,0 },
                {0,0,1,0,0 },
                {0,0,1,0,0 },
                {0,0,1,1,1 },
                {0,0,0,0,0 }
            },
            //  W Shape
            //  #
            //  ##
            //   ##
            {
                {0,0,0,0,0 },
                {0,1,0,0,0 },
                {0,1,1,0,0 },
                {0,0,1,1,0 },
                {0,0,0,0,0 }
            },
            //  X Shape
            //   #
            //  ###
            //   #
            {
                {0,0,0,0,0 },
                {0,0,1,0,0 },
                {0,1,1,1,0 },
                {0,0,1,0,0 },
                {0,0,0,0,0 }
            },
            //  Y Shape
            //   #
            //  ##
            //   #
            //   #
            {
                {0,0,0,0,0 },
                {0,0,1,0,0 },
                {0,1,1,0,0 },
                {0,0,1,0,0 },
                {0,0,1,0,0 }
            },
            //  Z Shape
            //  ##
            //   #
            //   ##
            {
                {0,0,0,0,0 },
                {0,1,1,0,0 },
                {0,0,1,0,0 },
                {0,0,1,1,0 },
                {0,0,0,0,0 }
            }
        };

    protected static int[,,] playerBase = new int[1, 5, 5]
    {   
            //  These hashes represent what the piece will look like
            //  ###
            //  ###
            //  ###
            {
                { 0,0,0,0,0 },
                { 0,1,1,1,0 },
                { 0,1,1,1,0 },
                { 0,1,1,1,0 },
                { 0,0,0,0,0 }
            }
    };

    public static int[] pieceTypes = new int[6] {   0,
                                                    monomino.GetLength(0),
                                                    domino.GetLength(0),
                                                    triomino.GetLength(0),
                                                    tetromino.GetLength(0),
                                                    pentomino.GetLength(0) };

    public Polyomino(int _units, int _index, Player _player, bool _isTerrain = false, bool _destructible = true)
    {
        index = _index;
        units = _units;
        owner = _player;
        isTerrain = _isTerrain;
        destructible = _destructible;
        occupyingBlueprints = new List<Blueprint>();
        cost = 1;
        if (owner != null) baseColor = owner.ColorScheme[0];

        buildingType = BuildingType.NONE;

        switch (_units)
        {
            case 1:
                holderName = "MonominoHolder";
                piece = monomino;
                break;
            case 2:
                holderName = "DominoHolder";
                piece = domino;
                break;
            case 3:
                holderName = "TriominoHolder";
                piece = triomino;
                break;
            case 4:
                holderName = "TetrominoHolder";
                piece = tetromino;
                break;
            case 5:
                holderName = "PentominoHolder";
                piece = pentomino;
                break;
            case 9:
                
                buildingType = BuildingType.BASE;
                piece = playerBase;
                destructible = false;
                break;
            default:
                break;
        }
    }

    public void SetAffordableStatus(Player player, bool energyUI = false)
    {
        if (energyUI) holder.SetEnergyDisplayStatus(true);
        affordable = player.resources >= cost;
        if (affordable)
        {
            SetTint(new Color(baseColor.r, baseColor.g, baseColor.b, alphaWhileAffordable), 1);
            //foreach (Tile tile in tiles)
            //{
            //    tile.SetFilledUIFillAmount(1);
            //}
            holder.SetEnergyLevel(1);
        }
        else
        {
            holder.SetEnergyLevel(player.resourceMeterFillAmt);
            SetTint(new Color(baseColor.r, baseColor.g, baseColor.b, alphaWhileUnaffordable), 1);
            //int tileCompletionProportion = 
            //    Mathf.FloorToInt(player.resourceMeterFillAmt * tiles.Count);
            //for (int i = 0; i < tiles.Count; i++)
            //{
            //    Tile tile = tiles[i];
            //    if (i <= tileCompletionProportion)
            //    {
            //        if (i == tileCompletionProportion)
            //        {
            //            tile.SetFilledUIFillAmount(
            //                (player.resourceMeterFillAmt - ((float)tileCompletionProportion / tiles.Count))
            //                * tiles.Count);
            //        }
            //        else
            //        {
            //            tile.SetFilledUIFillAmount(1);
            //        }
            //    }
            //    else tile.SetFilledUIFillAmount(0);
            //}
        }
    }

    public void QueueUp()
    {
        HideFromInput();
        foreach(Tile tile in tiles)
        {
            tile.SetHighlightColor(new Color(0, 0, 0, 0));
        }
        SetTint(new Color(baseColor.r, baseColor.g, baseColor.b, 0.75f), 1);
        ScaleHolder(QueueScale);
        Vector3 centerpoint = GetCenterpoint() * QueueScale.x;
        Reposition(holder.transform.position - centerpoint);
    }

    public void OnDrawn()
    {
        ListenForInput(false);
    }

    public bool IsWithinBounds()
    {
        bool withinBounds = false;
        foreach (Tile tile in tiles)
        {
            if (0 <= tile.coord.x && tile.coord.x < Services.MapManager.MapWidth &&
               0 <= tile.coord.y && tile.coord.y < Services.MapManager.MapHeight)
            {
                withinBounds = true;
            }
            else
            {
                withinBounds = false;
            }
        }

        return withinBounds;
    }

    public virtual void PlaceAtCurrentLocation()
    {
        PlaceAtCurrentLocation(false);
    }

    protected virtual void AssignLocation(Coord coord)
    {
        placed = true;
        SetTileCoords(coord);
        Reposition(new Vector3(coord.x, coord.y, holder.transform.position.z));
        Services.MapManager.Map[coord.x, coord.y].SetOccupyingPiece(this);
        tiles[0].OnPlace();
        SetTileSprites();
        ScaleHolder(Vector3.one);
        if (owner != null && owner.shieldedPieces) CreateShield();
    }

    protected virtual Polyomino CreateSubPiece()
    {
        Polyomino monomino = new Polyomino(1, 0, owner);
        monomino.MakePhysicalPiece();
        return monomino;
    }

    public virtual void PlaceTerrainAtCurrentLocation()
    {
        placed = true;
        
        OnPlace();

        foreach (Tile tile in tiles)
        {
            MapTile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            mapTile.SetOccupyingPiece(this);
            tile.OnPlace();
        }
        adjacentPieces = new List<Polyomino>();
        SortOverlay();
        SetOverlaySprite();
    }

    public virtual void PlaceAtCurrentLocation(bool replace)
    {
        if (isTerrain)
        {
            placed = true;
            OnPlace();

            foreach (Tile tile in tiles)
            {
                MapTile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                mapTile.SetOccupyingPiece(this);
                SetTileSprites();
                tile.OnPlace();
            }
            adjacentPieces = new List<Polyomino>();
            SortOverlay();
            SetOverlaySprite();
        }
        else
        {
            //place the piece on the board where it's being hovered now
            List<Polyomino> annexedMonominos = GetPiecesInRange(false);
            OnPlace();
            DestroyThis();
            if (owner != null && owner.crossSection)
            {
                List<Tile> overlappingTilesToRemove = new List<Tile>();
                foreach (Tile tile in tiles)
                {
                    MapTile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                    if (mapTile.occupyingPiece != null &&
                        (mapTile.occupyingPiece.owner == owner || 
                        (mapTile.occupyingPiece.owner == null && (mapTile.occupyingPiece.isTerrain && !mapTile.occupyingPiece.destructible))) &&
                        !overlappingTilesToRemove.Contains(tile))
                    {
                        overlappingTilesToRemove.Add(tile);
                    }
                }

                foreach (Tile tile in overlappingTilesToRemove)
                {
                    tiles.Remove(tile);
                }
            }

            List<Coord> monominoCoords = new List<Coord>();
            List<Polyomino> monominos = new List<Polyomino>();
            foreach (Tile tile in tiles)
            {
                monominoCoords.Add(tile.coord);
                Polyomino monomino = CreateSubPiece();
                monominos.Add(monomino);
            }

            if (owner != null && owner.annex)
            {
                foreach (Polyomino annexedPiece in annexedMonominos)
                {
                    monominoCoords.Add(annexedPiece.tiles[0].coord);

                    annexedPiece.Remove(false, false);

                    Polyomino monomino = CreateSubPiece();
                    monominos.Add(monomino);
                }
            }

            for (int i = 0; i < monominos.Count; i++)
            {
                monominos[i].AssignLocation(monominoCoords[i]);
            }

            if (!(this is Destructor && owner.splashDamage))
            {
                foreach (Polyomino monomino in monominos)
                {
                    monomino.adjacentPieces =
                        monomino.GetAdjacentPolyominos(monomino.owner);
                    foreach (Polyomino adjPiece in monomino.adjacentPieces)
                    {
                        if (!adjPiece.adjacentPieces.Contains(monomino))
                            adjPiece.adjacentPieces.Add(monomino);
                    }
                }
            }

            List<List<Polyomino>> distanceLevels = new List<List<Polyomino>>();
            //find distance level 0
            List<Polyomino> distanceLevelZero = new List<Polyomino>();
            List<Polyomino> tempMonominos = new List<Polyomino>(monominos);
            foreach (Polyomino monomino in monominos)
            {
                foreach (Polyomino neighbor in monomino.adjacentPieces)
                {
                    if (!monominos.Contains(neighbor))
                    {
                        distanceLevelZero.Add(monomino);
                        tempMonominos.Remove(monomino);
                        break;
                    }
                }
            }
            distanceLevels.Add(distanceLevelZero);
            int lastDistanceLevelIndex = 0;
            for (int i = 0; i < 5; i++)
            {
                if (distanceLevels[i].Count == 0) break;
                List<Polyomino> nextDistanceLevel = new List<Polyomino>();
                foreach (Polyomino monomino in tempMonominos)
                {
                    foreach (Polyomino neighbor in monomino.adjacentPieces)
                    {
                        if (distanceLevels[i].Contains(neighbor))
                        {
                            nextDistanceLevel.Add(monomino);
                            break;
                        }
                    }
                }
                foreach (Polyomino monomino in nextDistanceLevel)
                {
                    tempMonominos.Remove(monomino);
                }
                distanceLevels.Add(nextDistanceLevel);
                lastDistanceLevelIndex = i;
            }
            //for (int i = 0; i < distanceLevels.Count; i++)
            //{
            //    foreach(Polyomino monomino in distanceLevels[i])
            //    {
            //        monomino.tiles[0].StartScaling(Tile.scaleUpStaggerTime * i);
            //    }
            //}
            owner.OnPiecePlaced(this, monominos);

            switch (Services.GameManager.mode)
            {
                case TitleSceneScript.GameMode.HyperSOLO:
                case TitleSceneScript.GameMode.HyperVS:
                    HyperModeManager.Placement(owner.ColorScheme[0], holder.transform.position);
                    break;
            }

            List<Polyomino> shortestPath = AStarSearch.ShortestPath(
                owner.mainBase, distanceLevels[lastDistanceLevelIndex][0]);
            shortestPath.Reverse();
            List<Polyomino> pathToHighlight = new List<Polyomino>();
            for (int i = 0; i < shortestPath.Count; i++)
            {
                Polyomino pathPiece = shortestPath[i];
                if (pathPiece.occupyingBlueprints.Count > 0)
                {
                    Blueprint pathBlueprint = shortestPath[i].occupyingBlueprints[0];
                    if (!pathToHighlight.Contains(pathBlueprint))
                        pathToHighlight.Add(pathBlueprint);
                }
                else
                {
                    pathToHighlight.Add(pathPiece);
                }
            }
            foreach (Polyomino piece in monominos)
            {
                if (!pathToHighlight.Contains(piece)) pathToHighlight.Add(piece);
            }
            for (int i = 0; i < pathToHighlight.Count; i++)
            {
                //pathToHighlight[i].PathHighlight(i * Player.pathHighlightTotalDuration / pathToHighlight.Count);
            }
            int entranceIndex = 0;
            for (int i = 0; i < distanceLevels.Count; i++)
            {
                distanceLevels[i].OrderBy(t => t.centerCoord.Distance(owner.mainBase.centerCoord));
                for (int j = 0; j < distanceLevels[i].Count; j++)
                {
                    Tile tile = distanceLevels[i][j].tiles[0];

                    var index1 = entranceIndex;
                    Services.Clock.SyncFunction(() =>
                    {
                        tile.StartEntrance(Services.Clock.SixteenthLength()
                                           * index1 + float.Epsilon);
                        Services.AudioManager.PlaySoundEffect(Services.Clips.IndividualPieceLighting);
                    }, Clock.BeatValue.Sixteenth);
                    entranceIndex++;

                }
            }
        }
    }

    public List<Coord> GetAdjacentEmptyTiles()
    {
        List<Coord> adjacentEmptyTiles = new List<Coord>();

        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
                {
                    MapTile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.occupyingPiece == null)
                    {
                        adjacentEmptyTiles.Add(adjTile.coord);
                    }
                }
            }
        }

        return adjacentEmptyTiles;
    }

    public virtual List<TechBuilding> GetAdjacentStructures()
    {
        List<TechBuilding> adjacentStructures = new List<TechBuilding>();

        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
                {
                    MapTile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.IsOccupied() && adjTile.occupyingPiece is TechBuilding &&
                        !adjacentStructures.Contains((TechBuilding)adjTile.occupyingPiece))
                    {
                        adjacentStructures.Add((TechBuilding)adjTile.occupyingPiece);
                    }
                }
            }
        }

        return adjacentStructures;
    }

    public List<Polyomino> GetAdjacentPolyominos(Player player)
    {
        return GetAdjacentPolyominos(player, centerCoord);
    }

    public List<Polyomino> GetAdjacentPolyominos(Player player, Coord hypotheticalCenterCoord)
    {
        List<Polyomino> adjacentPieces = new List<Polyomino>();
        List<Coord> coordsChecked = new List<Coord>();
        List<Coord> hypotheticalTileCoords = new List<Coord>();
        foreach(Tile tile in tiles)
        {
            hypotheticalTileCoords.Add(hypotheticalCenterCoord.Add(tile.relativeCoord));
        }
        foreach (Coord tileCoord in hypotheticalTileCoords)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tileCoord.Add(direction);
                if (!coordsChecked.Contains(adjacentCoord))
                {
                    coordsChecked.Add(adjacentCoord);
                    if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
                    {
                        MapTile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                        if (adjTile.IsOccupied() && !adjacentPieces.Contains(adjTile.occupyingPiece) &&
                            adjTile.occupyingPiece != this && adjTile.occupyingPiece.owner == player)
                        {
                            adjacentPieces.Add(adjTile.occupyingPiece);
                        }
                    }
                }
            }
        }

        return adjacentPieces;
    }

    public static List<Polyomino> GetAdjacentPolyominosToCoord(Coord coord, Player player)
    {
        List<Polyomino> adjacentPieces = new List<Polyomino>();

        foreach (Coord direction in Coord.Directions())
        {
            Coord adjacentCoord = coord.Add(direction);
            if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
            {
                MapTile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                if (adjTile.IsOccupied() && !adjacentPieces.Contains(adjTile.occupyingPiece) 
                    && adjTile.occupyingPiece.owner == player)
                {
                    adjacentPieces.Add(adjTile.occupyingPiece);
                }
            }
        }
        return adjacentPieces;
    }

    public List<Polyomino> GetAdjacentPolyominosToCoord(Coord coord)
    {
        List<Polyomino> adjacentPieces = new List<Polyomino>();

        foreach (Coord direction in Coord.Directions())
        {
            Coord adjacentCoord = coord.Add(direction);
            if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
            {
                MapTile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                if (adjTile.IsOccupied() && !adjacentPieces.Contains(adjTile.occupyingPiece) &&
                    adjTile.occupyingPiece != this)
                {
                    adjacentPieces.Add(adjTile.occupyingPiece);
                }
            }
        }
        return adjacentPieces;
    }

    public List<Coord> GetAdjacentCoords(Player player)
    {
        List<Coord> adjacentCoords = new List<Coord>();
        List<Polyomino> adjacentPieces = GetAdjacentPolyominos(owner);
        foreach (Polyomino piece in adjacentPieces)
        {
           // if(!adjacentCoords.Contains(piece))
        }
        return adjacentCoords;
    }

    public List<Polyomino> GetAdjacentPolyominos()
    {
        List<Polyomino> adjacentPieces = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            adjacentPieces.AddRange(GetAdjacentPolyominosToCoord(tile.coord)); 
        }

        return adjacentPieces;
    }

    protected bool IsConnected()
    {
        List<Polyomino> adjPieces = GetAdjacentPolyominos(owner);
        bool connectedToBase = false;
        for (int i = 0; i < adjPieces.Count; i++)
        {
            if (adjPieces[i].connected || adjPieces[i] is TechBuilding)
            {
                connectedToBase = true;
                break;
            }
        }
        return connectedToBase;
    }

    protected virtual List<Tile> GetIllegalTiles()
    {
        List<Tile> illegalTiles = new List<Tile>();
        foreach (Tile tile in tiles)
        {
            if (Services.MapManager.IsCoordContainedInMap(tile.coord))
            {
                MapTile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                if (mapTile.IsOccupied() &&  mapTile.occupyingPiece is TechBuilding)
                {
                    illegalTiles.Add(tile);
                }
            }
            else
            {
                illegalTiles.Add(tile);
            }              
        }

        return illegalTiles;
    }

    public virtual bool IsPlacementLegal()
    {
        return IsPlacementLegal(GetAdjacentPolyominos(owner), centerCoord);
    }

    public virtual bool IsPlacementLegal(Coord hypotheticalCoord, bool pretendAttackResource = false)
    {
        return IsPlacementLegal(GetAdjacentPolyominos(owner, hypotheticalCoord), hypotheticalCoord, pretendAttackResource);
    }

    public virtual bool IsPlacementLegal(List<Polyomino> adjacentPieces, Coord hypotheticalCoord, bool pretendAttackResource = false)
    {
        bool connectedToBase = false;
        for (int i = 0; i < adjacentPieces.Count; i++)
        {
            if (adjacentPieces[i].connected || adjacentPieces[i] is TechBuilding)
            {
                connectedToBase = true;
                break;
            }
        }
        if (!connectedToBase) return false;
        List<Coord> hypotheticalTileCoords = new List<Coord>();

        foreach (Tile tile in tiles)
        {
            hypotheticalTileCoords.Add(hypotheticalCoord.Add(tile.relativeCoord));
        }
        bool completelyCovered = true;
        foreach (Coord coord in hypotheticalTileCoords)
        {
            if (!Services.MapManager.IsCoordContainedInMap(coord)) return false;

            MapTile mapTile = Services.MapManager.Map[coord.x, coord.y];
            if (mapTile.IsOccupied() && !mapTile.occupyingPiece.destructible && !owner.crossSection) return false;
            if (mapTile.IsOccupied() && mapTile.occupyingPiece.destructible && mapTile.occupyingPiece.isTerrain &&
                owner.attackResources < 1 && !pretendAttackResource) return false;
            if ((mapTile.IsOccupied() && !owner.crossSection) &&
                ((mapTile.occupyingPiece.connected && owner.attackResources < 1 && !pretendAttackResource) ||
                (mapTile.occupyingPiece.connected && (mapTile.occupyingPiece.owner == owner)) ||
                mapTile.occupyingPiece is TechBuilding))
                return false;
            if (mapTile.IsOccupied() && 
                mapTile.occupyingPiece.shieldDurationRemaining > 0 && 
                !pretendAttackResource &&
                (!owner.crossSection || mapTile.occupyingPiece.owner == null ||
                mapTile.occupyingPiece.owner != owner))
                return false;
            if(owner.crossSection && (!mapTile.IsOccupied() || mapTile.occupyingPiece.owner != owner)) completelyCovered = false;
            if (owner.crossSection && mapTile.occupyingPiece != null &&
                mapTile.occupyingPiece.owner != null && mapTile.occupyingPiece.owner != owner && 
                mapTile.occupyingPiece is TechBuilding)
                return false;
            if (owner.crossSection && mapTile.occupyingPiece != null &&
                mapTile.occupyingPiece.owner != owner && mapTile.occupyingPiece.connected &&
                owner.attackResources < 1 && !pretendAttackResource)
                return false;
        }

        if (owner != null && owner.crossSection && completelyCovered) return false;

        return true;
    }

    public bool ShareTilesWith(Tile tile)
    {
        foreach (Tile myTiles in tiles)
        {
            if (myTiles == tile)
            {
                return true;
            }
        }

        return false;
    }

    public void SetGlowState(bool playAvailable)
    {
        if (playAvailable) SetGlow(new Color(1.3f, 1.3f, 0.9f));
        else
            TurnOffGlow();
    }

    public void TurnOffGlow()
    {
        foreach (Tile tile in tiles)
        {
            tile.SetHighlightStatus(false);
            tile.ToggleIllegalLocationIcon(false);
        }
        holder.legalityOverlay.enabled = false;
    }

    public void SetGlow(Color color)
    {
        foreach (Tile tile in tiles)
        {
            tile.SetHighlightColor(color);
            tile.SetHighlightAlpha(0.5f);
            tile.SetHighlightStatus(true);
        }
        if (!placed) holder.legalityOverlay.enabled = true;
    }

    public void SetTint(Color color, float tintProportion)
    {
        foreach (Tile tile in tiles)
        {
            tile.SetColor((baseColor * (1 - tintProportion)) + (color * tintProportion));
        }
    }

    public void SetAlpha(float alpha)
    {
        foreach(Tile tile in tiles)
        {
            tile.SetAlpha(alpha);
        }
    }

    public void ShiftColor(Color color)
    {
        foreach (Tile tile in tiles) tile.ShiftColor(color);
        holder.spriteBottom.color = color;
    }

    protected virtual void OnPlace()
    {
        piecesToRemove = new List<Polyomino>();
        bool removedOpposingPiece = false;
        Vector3 positionOfDestruction = Vector3.zero;
        foreach (Tile tile in tiles)
        {
            MapTile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if ( owner != null && 
                !owner.crossSection && 
                mapTile.occupyingPiece != null &&
                !piecesToRemove.Contains(mapTile.occupyingPiece))
            {
                piecesToRemove.Add(mapTile.occupyingPiece);
                if (mapTile.occupyingPiece.owner != owner && mapTile.occupyingPiece.connected)
                {
                    removedOpposingPiece = true;
                    positionOfDestruction = mapTile.transform.position;
                }

                if (mapTile.occupyingPiece.isTerrain && mapTile.occupyingPiece.destructible)
                {
                    removedOpposingPiece = true;
                    positionOfDestruction = mapTile.transform.position;
                }
            }

            

            if(owner != null &&
                owner.crossSection &&
                mapTile.occupyingPiece != null &&
                mapTile.occupyingPiece.owner != null &&
                mapTile.occupyingPiece.owner != owner &&
                !piecesToRemove.Contains(mapTile.occupyingPiece))
            {
                piecesToRemove.Add(mapTile.occupyingPiece);
                if (mapTile.occupyingPiece.owner != owner && mapTile.occupyingPiece.connected)
                {
                    removedOpposingPiece = true;
                    positionOfDestruction = mapTile.transform.position;
                }

                if (mapTile.occupyingPiece.isTerrain && mapTile.occupyingPiece.destructible)
                {
                    removedOpposingPiece = true;
                    positionOfDestruction = mapTile.transform.position;
                }
            }
        }

        if (piecesToRemove.Count > 0 &&
            (Services.GameManager.mode != TitleSceneScript.GameMode.Edit && Services.GameManager.mode != TitleSceneScript.GameMode.DungeonEdit))
            Services.AudioManager.RegisterSoundEffect(Services.Clips.PieceDestroyed);
        for (int i = piecesToRemove.Count - 1; i >= 0; i--)
        {
            piecesToRemove[i].Remove();
        }

        if (removedOpposingPiece)
        {
            owner.OnDestructionOfOpposingPiece(positionOfDestruction);
            Services.GameManager.Players[owner.playerNum % 2].OnDestructionOfPiece();
        }
    }

    public virtual List<Polyomino> GetPiecesInRange(bool includeMyTiles = true)
    {
        List<Polyomino> opposingPiecesInRange = new List<Polyomino>();
        List<Coord> myCoords = new List<Coord>();
        foreach(Tile tile in tiles)
        {
            myCoords.Add(tile.coord);
        }

        if (owner != null)
        {
            List<Polyomino> adjacentOpposingPieces =
                GetAdjacentPolyominos(Services.GameManager.Players[owner.playerNum % 2]);
            for (int i = 0; i < adjacentOpposingPieces.Count; i++)
            {
                Polyomino opposingPiece = adjacentOpposingPieces[i];
                if (!opposingPiecesInRange.Contains(opposingPiece) && !(opposingPiece is TechBuilding) &&
                    !opposingPiece.connected && opposingPiece.occupyingBlueprints.Count < 1)
                {
                    if (!includeMyTiles)
                    {
                        foreach (Tile tile in tiles)
                        {
                            if (!myCoords.Contains(opposingPiece.tiles[0].coord))
                            {
                                opposingPiecesInRange.Add(opposingPiece);
                                break;
                            }
                        }
                    }
                    else
                    {
                        opposingPiecesInRange.Add(opposingPiece);
                    }
                }
            }
        }
        return opposingPiecesInRange;
    }

    protected void MakeDustClouds()
    {
        foreach(Tile tile in tiles)
        {
            GameObject.Instantiate(Services.Prefabs.DustCloud, 
                tile.transform.position, Quaternion.identity);
        }
    }

    void CreateShield()
    {
        shieldDurationRemaining = ShieldedPieces.ShieldDuration;
        foreach (Tile tile in tiles)
        {
            tile.SetShieldStatus(true);
            tile.SetShieldAlpha(0.5f);
        }
    }

    void DecayShield()
    {
        shieldDurationRemaining -= Time.deltaTime;
        float shieldDecayed = ShieldedPieces.ShieldDuration - shieldDurationRemaining;
        foreach (Tile tile in tiles)
            tile.SetShieldAlpha(Mathf.Lerp(0.8f, 0, Mathf.Pow(EasingEquations.Easing
                .ExpoEaseIn(shieldDecayed / ShieldedPieces.ShieldDuration),2)));
        if (shieldDurationRemaining <= 0)
        {
            foreach (Tile tile in tiles) tile.SetShieldStatus(false);
        }
    }

    public virtual void Remove(bool replace = false, bool deathAnim = true)
    {
        Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);
        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        if (!replace)
        {
            for (int i = occupyingBlueprints.Count - 1; i >= 0; i--)
            {
                occupyingBlueprints[i].Remove();
            }

            if (deathAnim)
            {
                DeathAnimation die = new DeathAnimation(this);
                die.Then(new ActionTask(DestroyThis));
                Services.GameScene.tm.Do(die);
            }
            else
            {
                DestroyThis();
            }
            if((Services.GameManager.mode != TitleSceneScript.GameMode.Edit  || Services.GameManager.mode != TitleSceneScript.GameMode.DungeonEdit)
                && !isTerrain)
                Services.GameData.filledMapTiles[owner.playerNum - 1] -= tiles.Count;
            
        }
        else
        {
            DestroyThis();
        }

        foreach (Tile tile in tiles)
        {
            Services.MapManager.Map[tile.coord.x, tile.coord.y].SetOccupyingPiece(null);
            Services.MapManager.Map[tile.coord.x, tile.coord.y].SetOccupyingBlueprint(null);
            tile.OnRemove();

        }
        
        foreach(Polyomino piece in adjacentPieces)
        {
            piece.adjacentPieces.Remove(this);
        }

        

        if((Services.GameManager.mode != TitleSceneScript.GameMode.Edit || Services.GameManager.mode != TitleSceneScript.GameMode.DungeonEdit) 
            && !isTerrain)
            owner.OnPieceRemoved(this);
        dead = true;
    }

    public virtual void DestroyThis()
    {
        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);
        
        foreach (Tile tile in tiles) tile.OnRemove();
        GameObject.Destroy(holder.gameObject);
    }

    public void Reposition(Vector3 pos, bool centered = false)
    {
        if (centered) pos -= (GetCenterpoint() * UnselectedScale.x);
        holder.transform.position = pos;
    }

    public void ApproachHandPosition(Vector3 targetPos)
    {
        targetPos -= (GetCenterpoint() * UnselectedScale.x);
        holder.transform.position += (targetPos - holder.transform.position) * handPosApproachFactor;
    }

    public void SetBasePosition(IntVector2 pos)
    {
        centerCoord = new Coord(pos.x, pos.y);
        holder.transform.position = Services.MapManager.Map[centerCoord.x, centerCoord.y].transform.position;
    }

    public void SetTileCoords(Coord centerPos)
    {
        centerCoord = centerPos;

        foreach(Tile tile in tiles)
        {
            tile.SetCoord(tile.relativeCoord.Add(centerPos));
        }
    }

    public void AddOccupyingBlueprint(Blueprint blueprint)
    {
        if (!occupyingBlueprints.Contains(blueprint))
        {
            occupyingBlueprints.Add(blueprint);
        }
    }

    public void RemoveOccupyingBlueprint(Blueprint blueprint)
    {
        occupyingBlueprints.Remove(blueprint);
    }

    public virtual void MakePhysicalPiece()
    {
        holder = GameObject.Instantiate(Services.Prefabs.PieceHolder,
            Services.GameScene.transform).GetComponent<PieceHolder>();
        holder.Init(this);
        holder.gameObject.name = holderName;
        tooltips = new List<Tooltip>();
        adjacentPieces = new List<Polyomino>();
        previouslyHoveredMapTiles = new List<MapTile>();

        if (piece == null) return;

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    Tile newTile = MonoBehaviour.Instantiate(Services.Prefabs.Tile, holder.transform);

                    Coord myCoord = new Coord(-2 + x, -2 + y);
                    newTile.Init(myCoord, this);

                    string pieceName = newTile.name.Replace("(Clone)", "");
                    newTile.name = pieceName;
                    newTile.SetBaseTileColor(owner, buildingType);
                    tiles.Add(newTile);

                }
            }
        }
        SetSprites();
        if (buildingType != BuildingType.BASE && buildingType != BuildingType.EDITMODE)
        {
            EnterUnselectedState(false);
        }

        lastPositions = new Queue<Coord>();
    }

    protected virtual void SetIconSprite()
    {
        holder.icon.transform.localPosition = GetCenterpoint();
        holder.icon.enabled = false;
    }

    public Vector3 GetCenterpoint(bool centerTile = false)
    {
        Vector3 centerPos = Vector3.zero;
        foreach (Tile tile in tiles)
        {
            centerPos += tile.transform.localPosition;
        }
        centerPos /= tiles.Count;
        if (centerTile)
        {
            Tile closestTile = null;
            float closestDistance = Mathf.Infinity;
            foreach (Tile tile in tiles)
            {
                float dist = Vector3.Distance(tile.transform.localPosition, centerPos);
                if(dist < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = dist;
                }
            }
            return closestTile.transform.localPosition;
        }

        return centerPos;
    }

    protected void EnterUnselectedState(bool allowAIPlayers)
    {
        ListenForInput(allowAIPlayers);
        ScaleHolder(UnselectedScale);
    }

    protected void ListenForInput(bool allowAIPlayers)
    {
        if (!(owner is AIPlayer) || allowAIPlayers)
        {
            Services.GameEventManager.Register<TouchDown>(OnTouchDown);
            Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
            touchID = -1;
        }
    }

    protected void HideFromInput()
    {
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
    }

    protected virtual bool IsPointContainedWithinHolderArea(Vector3 point)
    {
        Debug.Assert(holder.holderSelectionArea != null);
        Vector3 extents;
        Vector3 centerPoint;
        if (!placed)
        {
            extents = holder.holderSelectionArea.bounds.extents;
            centerPoint = holder.transform.position;
        }
        else
        {
            extents = holder.spriteBottom.bounds.extents;
            centerPoint = holder.spriteBottom.transform.position;
        }
        return point.x >= centerPoint.x - extents.x && point.x <= centerPoint.x + extents.x &&
            point.y >= centerPoint.y - extents.y && point.y <= centerPoint.y + extents.y;
    }

    protected void OnTouchDown(TouchDown e)
    {
        Vector3 touchWorldPos = Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position);
        if (IsPointContainedWithinHolderArea(touchWorldPos) && touchID == -1
            && (owner == null || owner.selectedPiece == null))
        {
            touchID = e.touch.fingerId;
            OnInputDown(false);
        }
    }

    protected void OnMouseDownEvent(MouseDown e)
    {
        Vector3 mouseWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
        if (IsPointContainedWithinHolderArea(mouseWorldPos) && (owner == null || owner.selectedPiece == null))
        {
            OnInputDown(false);
        }
    }

    protected void OnTouchUp(TouchUp e)
    {
        if (e.touch.fingerId == touchID)
        {
            OnInputUp();
            touchID = -1;
        }
    }

    protected void OnMouseUpEvent(MouseUp e)
    {
        OnInputUp();
    }

    public virtual void OnInputDown(bool fromPlayTask)
    {
        if (!owner.gameOver && !placed)
        {
            lastPositions = new Queue<Coord>();
            ScaleHolder(Vector3.one);
            holder.transform.localPosition = new Vector3(holder.transform.position.x, holder.transform.position.y, -4);
            owner.OnPieceSelected(this);
            SortOnSelection(true);
            OnInputDrag(holder.transform.position);
            Services.AudioManager.RegisterSoundEffect(Services.Clips.PiecePicked);

            if (!(owner is AIPlayer))
            {
                Services.GameEventManager.Register<TouchUp>(OnTouchUp);
                Services.GameEventManager.Register<TouchMove>(OnTouchMove);
                Services.GameEventManager.Register<TouchDown>(CheckTouchForRotateInput);
                Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

                Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
                Services.GameEventManager.Register<MouseMove>(OnMouseMoveEvent);
                Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
            }
        }
    }

    protected void OnMouseMoveEvent(MouseMove e)
    {
        OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos));
    }

    protected void OnTouchMove(TouchMove e)
    {
        if (e.touch.fingerId == touchID)
        {
            OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position));
        }
    }

    public virtual void OnInputUp(bool forceCancel = false)
    {
        if (!placed)
        {
            if (lastPositions.Count > 0)
            {
                bool snapback = false;
                int sameCoordInARow = 1;
                Coord[] lastPositionsArray = lastPositions.ToArray();
                Coord lastCoord = lastPositionsArray[0];
                Coord coordToSnapbackTo = lastCoord;
                for (int i = 1; i < lastPositionsArray.Length; i++)
                {
                    if (lastPositionsArray[i].Equals(lastCoord))
                    {
                        sameCoordInARow += 1;
                        if (sameCoordInARow >= framesBeforeLockIn)
                        {
                            coordToSnapbackTo = lastCoord;
                            snapback = true;
                            break;
                        }
                    }
                    else
                    {
                        lastCoord = lastPositionsArray[i];
                        sameCoordInARow = 1;
                    }
                }
                if (snapback)
                {
                    SetTileCoords(coordToSnapbackTo);
                    Reposition(new Vector3(coordToSnapbackTo.x, coordToSnapbackTo.y,
                        holder.transform.position.z));
                    foreach (MapTile mapTile in previouslyHoveredMapTiles)
                    {
                        mapTile.SetMapSprite();
                    }
                }
            }
            if (!(owner is AIPlayer))
            {
                Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
                Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
                Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);

                Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
                Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
            }
            bool piecePlaced = false;
            if (IsPlacementLegal() && affordable && !owner.gameOver && !forceCancel)
            {
                PlaceAtCurrentLocation();
                piecePlaced = true;
            }
            else if(buildingType != BuildingType.EDITMODE)
            {
                owner.CancelSelectedPiece();
                EnterUnselectedState(false);
                piecePlaced = false;
            }
            CleanUpUI(piecePlaced);
            SortOnSelection(false);
            holder.transform.localPosition = new Vector3(holder.transform.position.x, holder.transform.position.y, 0);
        }
    }

    public virtual void OnInputDrag(Vector3 inputPos)
    {
        if (!placed && !owner.gameOver)
        {
            Vector3 screenInputPos = 
                Services.GameManager.MainCamera.WorldToScreenPoint(inputPos);
            Vector3 screenOffset;
            float mapEdgeScreenHeight = Services.CameraController.GetMapEdgeScreenHeight();
            if (owner.playerNum == 1)
            {
                screenOffset = baseDragOffset + 
                    (((mapEdgeScreenHeight - (Screen.height / 2) 
                        - baseDragOffset.y) / (Screen.height / 2))
                    * screenInputPos.y * Vector3.up);
            }
            else
            {
                screenOffset = -baseDragOffset + 
                    (((mapEdgeScreenHeight - (Screen.height / 2)
                        - baseDragOffset.y) / (Screen.height / 2))
                    * (Screen.height - screenInputPos.y) * Vector3.down);
            }
            Vector3 offsetInputPos = Services.GameManager.MainCamera.ScreenToWorldPoint(
                screenInputPos + screenOffset);
            if (owner is AIPlayer) offsetInputPos = inputPos;
            Coord roundedInputCoord = new Coord(
                Mathf.RoundToInt(offsetInputPos.x),
                Mathf.RoundToInt(offsetInputPos.y));
            Coord snappedCoord = roundedInputCoord;
            if(!IsPlacementLegal(roundedInputCoord, true))
            {
                List<Coord> nearbyCoords = new List<Coord>();
                foreach (Coord direction in Coord.Directions())
                {
                    Coord nearbyCoord = roundedInputCoord.Add(direction);
                    if (nearbyCoords.Count == 0) nearbyCoords.Add(nearbyCoord);
                    else
                    {
                        bool added = false;
                        for (int i = 0; i < nearbyCoords.Count; i++)
                        {
                            if(Vector2.Distance(new Vector2(nearbyCoord.x, nearbyCoord.y), 
                                offsetInputPos) <
                                Vector2.Distance(new Vector2(nearbyCoords[i].x, nearbyCoords[i].y),
                                offsetInputPos))
                            {
                                nearbyCoords.Insert(i, nearbyCoord);
                                added = true;
                                break;
                            }
                        }
                        if (!added) nearbyCoords.Add(nearbyCoord);
                    }
                }
                for (int i = 0; i < nearbyCoords.Count; i++)
                {
                    Coord nearbyCoord = nearbyCoords[i];
                    if (IsPlacementLegal(nearbyCoord, true))
                    {
                        snappedCoord = nearbyCoord;
                        break;
                    }
                }
            }
            SetTileCoords(snappedCoord);
            Reposition(new Vector3(
                snappedCoord.x,
                snappedCoord.y,
                holder.transform.position.z));
            QueuePosition(snappedCoord);
        }

        if(!Services.GameManager.disableUI) SetLegalityGlowStatus();
    }

    public void CheckTouchStatus()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) return;
        for (int i = 0; i < Input.touches.Length; i++)
        {
            if (Input.touches[i].fingerId == touchID) return;
        }
        OnInputUp(true);
    }

    protected virtual void CleanUpUI(bool piecePlaced)
    {
        UnhighlightPotentialStructureClaims();
        holder.SetEnergyDisplayStatus(false);
        holder.SetAttackDisplayStatus(false);
        if (!piecePlaced)
        {
            foreach (MapTile mapTile in previouslyHoveredMapTiles)
            {
                mapTile.SetMapSprite();
            }
        }
    }

    public virtual void SetLegalityGlowStatus()
    {
        bool isLegal = IsPlacementLegal();
        bool overDestructiblePiece = false;
        foreach(MapTile mapTile in previouslyHoveredMapTiles)
        {
            mapTile.SetMapSprite();
        }
        List<MapTile> hoveredMapTiles = new List<MapTile>();
        foreach (Tile tile in tiles)
        {
            tile.ToggleIllegalLocationIcon(false);
            Coord coord = tile.coord;
            if (Services.MapManager.IsCoordContainedInMap(coord)) {
                MapTile mapTile = Services.MapManager.Map[coord.x, coord.y];
                hoveredMapTiles.Add(mapTile);
                if(mapTile.occupyingPiece != null && !(mapTile.occupyingPiece is TechBuilding) &&
                    mapTile.occupyingPiece.owner != owner &&
                    (mapTile.occupyingPiece.connected || (mapTile.occupyingPiece.isTerrain && mapTile.occupyingPiece.destructible)))
                {
                    overDestructiblePiece = true;
                }
            }
        }
        previouslyHoveredMapTiles = hoveredMapTiles;
        if (!(this is Blueprint))
        {
            SetAffordableStatus(owner, true);
            holder.SetAttackDisplayStatus(overDestructiblePiece);
        }
        if (overDestructiblePiece)
        {
            if (owner.attackResources > 0) holder.SetAttackLevel(1);
            else holder.SetAttackLevel(owner.destructorDrawMeterFillAmt);
        }

        foreach (MapTile mapTile in hoveredMapTiles)
        {
            mapTile.SetMapSpriteHovered(isLegal);
        }

        if (isLegal && (affordable || this is Blueprint))
        {
            //SetGlow(Services.UIManager.legalGlowColor);
            holder.legalityOverlay.enabled = false;
        }
        else if (isLegal && !affordable && !(this is Blueprint))
        {
            //SetGlow(Color.yellow);
            holder.legalityOverlay.enabled = false;
        }
        else
        {
            //SetGlow(new Color(1, 0.2f, 0.2f));
            bool connected = IsConnected();
            if(!(this is Blueprint) && !connected)
            {
                holder.legalityOverlay.enabled = false;
            }
            else
            {
                holder.legalityOverlay.enabled = false;
                List<Tile> illegalTiles = GetIllegalTiles();
                foreach(Tile tile in tiles)
                {
                    //tile.ToggleIllegalLocationIcon(illegalTiles.Contains(tile));
                }
            }
        }
        UnhighlightPotentialStructureClaims();
        highlightedStructures = new List<TechBuilding>();
        List<TechBuilding> adjStructures = GetAdjacentStructures();
        if (isLegal)
        {
            foreach (TechBuilding structure in adjStructures)
            {
                if (structure.owner == null)
                {
                    structure.SetGlow(owner.ColorScheme[0]);
                    highlightedStructures.Add(structure);
                }
            }
        }
    }

    protected void UnhighlightPotentialStructureClaims()
    {
        if (highlightedStructures != null)
        {
            foreach (TechBuilding structure in highlightedStructures)
            {
                structure.TurnOffGlow();
            }
        }
    }

    protected virtual void SetTileSprites()
    {
        List<Coord> tileRelativeCoords = new List<Coord>();
        foreach (Tile tile in tiles) tileRelativeCoords.Add(tile.relativeCoord);

        foreach (Tile tile in tiles)
        {
            //int spriteIndex = 15;
            //Coord[] directions = Coord.Directions();
            //for (int i = 0; i < directions.Length; i++)
            //{
            //    Coord adjCoord = tile.relativeCoord.Add(directions[i]);
            //    if (tileRelativeCoords.Contains(adjCoord))
            //    {
            //        spriteIndex -= Mathf.RoundToInt(Mathf.Pow(2, i));
            //    }
            //}
            tile.SetSprite();
            SetRelativeSortingOrder(tile);
        }
    }

    private void SetRelativeSortingOrder(Tile tile)
    {
        //tile.SetSortingOrder(baseSortingOrder - tile.coord.x - (100 * tile.coord.y));
    }

    protected void CheckTouchForRotateInput(TouchDown e)
    {
        if (Services.GameManager.onIPhone ||
            (Services.GameManager.mode != TitleSceneScript.GameMode.TwoPlayers &&
            Services.GameManager.mode != TitleSceneScript.GameMode.HyperVS) ||
            (Vector2.Distance(
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position),
            Services.GameManager.MainCamera.ScreenToWorldPoint(Input.GetTouch(touchID).position))
            < rotationInputRadius) ||
            (e.touch.position.y < (Screen.height / 2 - rotationDeadZone) && owner.playerNum == 1 ||
            e.touch.position.y > (Screen.height / 2 + rotationDeadZone) && owner.playerNum == 2))
        {
            Rotate();
        }
    }

    public virtual void Rotate() { Rotate(true, false); }

    public virtual void Rotate(bool relocate, bool dataOnly)
    {
        float rotAngle = 90 * Mathf.Deg2Rad;
        foreach (Tile tile in tiles)
        {
            Coord prevRelCoord = tile.relativeCoord;
            int newXCoord = Mathf.RoundToInt(
                prevRelCoord.x * Mathf.Cos(rotAngle)
                - (prevRelCoord.y * Mathf.Sin(rotAngle)));
            int newYCoord = Mathf.RoundToInt(
                prevRelCoord.x * Mathf.Sin(rotAngle)
                + (prevRelCoord.y * Mathf.Cos(rotAngle)));
            tile.relativeCoord = new Coord(newXCoord, newYCoord);
        }
        SetTileCoords(centerCoord);
        numRotations = (numRotations + 1) % 4;
        if ((this == owner.selectedPiece || placed) && owner is AIPlayer)
        {
            Debug.Log("rotating while selected or placed");
        }        
        if (!dataOnly)
        {
            SetTileSprites();

            if (relocate)
            {
                foreach (Tile tile in tiles)
                {
                    tile.transform.localPosition = 
                        new Vector3(tile.relativeCoord.x, tile.relativeCoord.y);
                }
            }

            SetIconSprite();
            if(!Services.GameManager.disableUI) SetLegalityGlowStatus();
            SetOverlaySprite();
            holder.UpdateEnergyDisplayPos();
            Services.GameEventManager.Fire(new RotationEvent());
            Services.AudioManager.RegisterSoundEffectReverb(Services.Clips.PieceRotated, 1.0f, Clock.BeatValue.Sixteenth);
        }
    }

    public virtual void PlaceAtLocation(Coord centerCoordLocation)
    {
        PlaceAtLocation(centerCoordLocation, false);
    }

    public virtual void PlaceAtLocation(Coord centerCoordLocation, bool replace, bool terrain = false)
    {
        SetTileCoords(centerCoordLocation);
        Reposition(new Vector3(
            centerCoordLocation.x,
            centerCoordLocation.y,
            holder.transform.position.z));
        if (!terrain) PlaceAtCurrentLocation(replace);
        else PlaceTerrainAtCurrentLocation();
    }

    public void PlaceTerrainAtCurrentLocation(bool replace)
    {
        placed = true;
        OnPlace();

        foreach (Tile tile in tiles)
        {
            MapTile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            mapTile.SetOccupyingPiece(this);
            tile.OnPlace();
        }
        adjacentPieces = new List<Polyomino>();
        SortOverlay();
        SetOverlaySprite();
        SetTileSprites();
    }

    public virtual void Update()
    {
        if (shieldDurationRemaining >= 0) DecayShield();
    }

    public void ScaleHolder(Vector3 scale)
    {
        holder.transform.localScale = scale;
    }

    public virtual void TogglePieceConnectedness(bool connected_)
    {
        if (connected && !connected_) owner.OnPieceDisconnected(this);

        if (!(this is TechBuilding) && !(this is Blueprint))
        {
            if (connected_)
            {
                ShiftColor(owner.ColorScheme[0]);
            }
            else
            {
                ShiftColor(owner.ColorScheme[1]);
            }
            foreach (Tile tile in tiles) tile.ToggleConnectedness(connected_);
        }
        connected = connected_;
        for (int i = 0; i < occupyingBlueprints.Count; i++)
        {
            occupyingBlueprints[i].TogglePieceConnectedness(connected_);
        }

    }

    protected virtual void SortOnSelection(bool selected)
    {
        foreach(Tile tile in tiles)
        {
            tile.SortOnSelection(selected);
        }
        if (selected)
        {
            holder.icon.sortingLayerName = "SelectedPieceOverlay";
            holder.spriteBottom.sortingLayerName = "SelectedPieceOverlay";
        }
        else
        {
            holder.icon.sortingLayerName = "Overlay";
            holder.spriteBottom.sortingLayerName = "Overlay";
        }
    }

    protected virtual void SetOverlaySprite()
    {
        Color overlayColor;
        if (owner != null)
        {
            overlayColor = owner.ColorScheme[0];
        }
        else
        {
            overlayColor = Services.GameManager.NeutralColor;
        }
        holder.spriteBottom.color = overlayColor;
        holder.spriteBottom.transform.localPosition = GetCenterpoint();
    }

    protected virtual void SetSprites()
    {
        SetIconSprite();
        SetOverlaySprite();
        SetTileSprites();
    }

    public virtual string GetName()
    {
        return "";
    }

    public virtual string GetDescription()
    {
        return "";
    }

    protected void DestroyTooltips()
    {
        Services.UIManager.OnTooltipDestroyed(touchID);
        for (int i = tooltips.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(tooltips[i].gameObject);
            tooltips.Remove(tooltips[i]);
        }
        if (placed)
        {
            Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
            Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);

            Services.GameEventManager.Register<TouchDown>(OnTouchDown);
            Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        }
    }

    public void BurnFromHand()
    {
        HideFromInput();
        SortOnSelection(true);
        BurnPiece burnTask = new BurnPiece(this);
        burnTask.Then(new ActionTask(DestroyThis));
        Services.GameScene.tm.Do(burnTask);
        burningFromHand = true;
        foreach (Tile tile in tiles) tile.OnRemove();
    }

    public void Lock()
    {
        HideFromInput();
    }

    public void Unlock()
    {
        ListenForInput(false);
    }

    protected void QueuePosition(Coord pos)
    {
        if(lastPositions.Count >= (framesBeforeLockIn + leniencyFrames))
        {
            lastPositions.Dequeue();
        }
        lastPositions.Enqueue(pos);
    }

    protected void SortOverlay()
    {
        //holder.spriteBottom.sortingOrder = (-centerCoord.x * 10) - (centerCoord.y * 1000);
        //holder.dropShadow.sortingOrder = holder.spriteBottom.sortingOrder + 1;
        //holder.icon.sortingOrder = holder.spriteBottom.sortingOrder + 2;
    }

    public virtual void PathHighlight(float delay)
    {
        foreach(Tile tile in tiles)
        {
            tile.StartPathHighlight(delay);
        }
    }
}

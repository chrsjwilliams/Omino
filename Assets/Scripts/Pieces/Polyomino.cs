using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{ BASE,FACTORY, MINE, STRUCTURE,NONE, BOMBFACTORY, MININGDRILL, ASSEMBLYLINE, FORTIFIEDSTEEL,
    BIGGERBRICKS, BIGGERBOMBS, SPLASHDAMAGE, SHIELDEDPIECES}


public class Polyomino : IVertex
{
    public List<Tile> tiles = new List<Tile>();
    public List<Coord> pieceCoords = new List<Coord>();
    //public Dictionary<Tile, Coord> tileRelativeCoords { get; protected set; }
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
    protected int touchID;
    private readonly Vector3 baseDragOffset = 5f * Vector3.up;
    public static Vector3 unselectedScale = 0.5f * Vector3.one;
    public static Vector3 queueScale = 0.2f * Vector3.one;
    public const float drawAnimDur = 0.5f;
    public const float deathAnimDur = 0.5f;
    public const float deathAnimScaleUp = 1.5f;
    public const float resourceGainAnimDur = 0.5f;
    public const float resourceGainAnimStaggerTime = 0.06f;
    public const float resourceGainAnimNoiseSpeed = 1;
    public const float resourceGainAnimNoiseMag = 4;
    private const float handPosApproachFactor = 0.25f;
    public const float burnPieceDuration = 0.5f;
    public static Vector3 burnPieceOffset = new Vector3(2, 0, 0);
    private const float alphaWhileUnaffordable = 0.3f;
    private const float alphaWhileAffordable = 0.8f;
    private List<Structure> highlightedStructures;

    public List<Blueprint> occupyingBlueprints { get; protected set; }
    public int cost { get; protected set; }
    private bool affordable;
    public bool connected { get; protected set; }
    public bool dead { get; private set; }
    public float shieldDurationRemaining { get; private set; }
    protected List<Tooltip> tooltips;
    private int baseSortingOrder;
    protected int numRotations;
    private Queue<Coord> lastPositions;
    private const int framesBeforeLockIn = 10;
    private const int leniencyFrames = 5;
    public bool burningFromHand { get; private set; }

    public List<Polyomino> adjacentPieces;

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
                {0,0,1,0,0 },
                {0,0,1,0,0 },
                {0,0,1,0,0 },
                {0,0,1,0,0 },
                {0,0,1,0,0 }
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


    public string ownerName;
    public Polyomino(int _units, int _index, Player _player)
    {
        index = _index;
        units = _units;
        owner = _player;

        occupyingBlueprints = new List<Blueprint>();
        //cost = units;
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
                if (owner != null) holderName = "Player " + owner.playerNum + " Base";
                else holderName = "Neutral Base";
                buildingType = BuildingType.BASE;
                piece = playerBase;
                break;
            default:
                break;
        }
    }

    public void SetAffordableStatus(Player player)
    {
        affordable = player.resources >= cost;
        if (affordable)
        {
            SetTint(new Color(baseColor.r, baseColor.g, baseColor.b, alphaWhileAffordable), 1);
            foreach (Tile tile in tiles)
            {
                tile.SetFilledUIFillAmount(1);
            }
        }
        else
        {
            SetTint(new Color(baseColor.r, baseColor.g, baseColor.b, alphaWhileUnaffordable), 1);
            int tileCompletionProportion = 
                Mathf.FloorToInt(player.resourceMeterFillAmt * tiles.Count);
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];
                if (i <= tileCompletionProportion)
                {
                    //tile.SetFilledUIStatus(true);
                    if (i == tileCompletionProportion)
                    {
                        tile.SetFilledUIFillAmount(
                            (player.resourceMeterFillAmt - ((float)tileCompletionProportion / tiles.Count))
                            * tiles.Count);
                    }
                    else
                    {
                        tile.SetFilledUIFillAmount(1);
                    }
                }
                else tile.SetFilledUIFillAmount(0);
            }
        }
    }

    public void QueueUp()
    {
        HideFromInput();
        foreach(Tile tile in tiles)
        {
            //tile.mask.enabled = true;
            tile.SetHighlightColor(new Color(0, 0, 0, 0));
        }
        SetTint(new Color(baseColor.r, baseColor.g, baseColor.b, 0.75f), 1);
        ScaleHolder(queueScale);
        Vector3 centerpoint = GetCenterpoint() * queueScale.x;
        //Debug.Log("base pos: " + holder.transform.position);
        //Debug.Log("piece centerpoint: " + centerpoint);
        //Debug.Log("adjusted pos: " + (holder.transform.position - centerpoint));
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
        if (owner.shieldedPieces) CreateShield();
    }

    protected virtual Polyomino CreateSubPiece()
    {
        Polyomino monomino = new Polyomino(1, 0, owner);
        monomino.MakePhysicalPiece();
        return monomino;
    }

    public virtual void PlaceAtCurrentLocation(bool replace)
    {
        //place the piece on the board where it's being hovered now
        OnPlace();
        DestroyThis();
        List<Polyomino> monominos = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            Polyomino monomino = CreateSubPiece();
            monominos.Add(monomino);
        }
        //ConstructionTask construct = new ConstructionTask(this, monominos);
        //Services.GameScene.tm.Do(construct);
        for (int i = 0; i < monominos.Count; i++)
        {
            monominos[i].AssignLocation(tiles[i].coord);
            //monominos[i].tiles[0].StartScaling(i * Tile.scaleUpStaggerTime);
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
        for (int i = 0; i < 5; i++)
        {
            if (distanceLevels[i].Count == 0) break;
            List<Polyomino> nextDistanceLevel = new List<Polyomino>();
            foreach(Polyomino monomino in tempMonominos)
            {
                foreach(Polyomino neighbor in monomino.adjacentPieces)
                {
                    if (distanceLevels[i].Contains(neighbor))
                    {
                        nextDistanceLevel.Add(monomino);
                        break;
                    }
                }
            }
            foreach(Polyomino monomino in nextDistanceLevel)
            {
                tempMonominos.Remove(monomino);
            }
            distanceLevels.Add(nextDistanceLevel);
        }
        for (int i = 0; i < distanceLevels.Count; i++)
        {
            foreach(Polyomino monomino in distanceLevels[i])
            {
                monomino.tiles[0].StartScaling(Tile.scaleUpStaggerTime * i);
            }
        }
        owner.OnPiecePlaced(this, monominos);
        Services.AudioManager.CreateTempAudio(Services.Clips.PiecePlaced, 0.5f);
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
                    Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.occupyingPiece == null)
                    {
                        adjacentEmptyTiles.Add(adjTile.coord);
                    }
                }
            }
        }

        return adjacentEmptyTiles;
    }

    public virtual List<Structure> GetAdjacentStructures()
    {
        List<Structure> adjacentStructures = new List<Structure>();

        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
                {
                    Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.IsOccupied() && adjTile.occupyingPiece is Structure &&
                        !adjacentStructures.Contains((Structure)adjTile.occupyingPiece))
                    {
                        adjacentStructures.Add((Structure)adjTile.occupyingPiece);
                    }
                }
            }
        }

        return adjacentStructures;
    }

    public List<Polyomino> GetAdjacentPolyominos(Player player)
    {
        List<Polyomino> adjacentPieces = new List<Polyomino>();
        List<Coord> coordsChecked = new List<Coord>();
        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (!coordsChecked.Contains(adjacentCoord))
                {
                    coordsChecked.Add(adjacentCoord);
                    if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
                    {
                        Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
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
                Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
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
                Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
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
            if (adjPieces[i].connected || adjPieces[i] is Structure)
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
                Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
                if (mapTile.IsOccupied() && (mapTile.occupyingPiece.connected || mapTile.occupyingPiece is Structure))
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
        return IsPlacementLegal(GetAdjacentPolyominos(owner));
    }

    public virtual bool IsPlacementLegal(List<Polyomino> adjacentPieces)
    {
        //determine if the pieces current location is a legal placement
        //CONDITIONS:
        //is contiguous with a structure connected to either the base or a fortification
        //doesn't overlap with any existing pieces or is a destructor\
        bool connectedToBase = false;
        for (int i = 0; i < adjacentPieces.Count; i++)
        {
            if (adjacentPieces[i].connected || adjacentPieces[i] is Structure)
            {
                connectedToBase = true;
                break;
            }
        }
        if (!connectedToBase) return false;
        foreach (Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.IsOccupied() && (mapTile.occupyingPiece.connected || mapTile.occupyingPiece is Structure))
                return false;
        }
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
        //if (placed && isFortified) return;
        foreach (Tile tile in tiles)
        {
            //tile.SetMaskSrAlpha(0);
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
            //tile.SetGlowOutLine(10);
            //tile.SetGlowColor(color);
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
        List<Polyomino> piecesToRemove = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.occupyingPiece != null && !piecesToRemove.Contains(mapTile.occupyingPiece))
            {
                piecesToRemove.Add(mapTile.occupyingPiece);
            }
        }
        if (piecesToRemove.Count > 0)
            Services.AudioManager.CreateTempAudio(Services.Clips.PieceDestroyed, 1);
        for (int i = piecesToRemove.Count - 1; i >= 0; i--)
        {
            piecesToRemove[i].Remove();
        }
    }

    protected void MakeDustClouds()
    {
        foreach(Tile tile in tiles)
        {
            GameObject.Instantiate(Services.Prefabs.DustCloud, 
                tile.transform.position, Quaternion.identity);
        }
    }

    public void FortifyPiece()
    {
        foreach (Tile tile in tiles)
        {
            Polyomino monomino = new Polyomino(1, 0, owner);
            //monomino.ToggleAltColor(true);
            monomino.MakePhysicalPiece();
            monomino.PlaceAtLocation(tile.coord);
        }
    }

    void CreateShield()
    {
        shieldDurationRemaining = ShieldedPieces.ShieldDuration;
        //shield = GameObject.Instantiate(Services.Prefabs.Shield, 
        //    GetCenterpoint(), Quaternion.identity);
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
        //shield.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero,
        //    Mathf.Pow(EasingEquations.Easing.ExpoEaseIn(
        //        shieldDecayed / ShieldedPieces.ShieldDuration), 2));
        foreach (Tile tile in tiles)
            tile.SetShieldAlpha(Mathf.Lerp(0.8f, 0, Mathf.Pow(EasingEquations.Easing
                .ExpoEaseIn(shieldDecayed / ShieldedPieces.ShieldDuration),2)));
        if (shieldDurationRemaining <= 0)
        {
            //GameObject.Destroy(shield);
            foreach (Tile tile in tiles) tile.SetShieldStatus(false);
        }
    }

    public virtual void Remove()
    {
        Remove(false);
    }

    public virtual void Remove(bool replace)
    {
        Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);
        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        if (!replace)
        {
            for (int i = occupyingBlueprints.Count - 1; i >= 0; i--)
            {
                occupyingBlueprints[i].Remove();
            }

            //List<Structure> adjStructures = GetAdjacentStructures();
            //foreach(Structure structure in adjStructures)
            //{
            //    structure.ToggleStructureActivation(owner);
            //}

            DeathAnimation die = new DeathAnimation(this);
            die.Then(new ActionTask(DestroyThis));
            Services.GameScene.tm.Do(die);
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
        if (centered) pos -= (GetCenterpoint() * unselectedScale.x);
        holder.transform.position = pos;
    }

    public void ApproachHandPosition(Vector3 targetPos)
    {
        targetPos -= (GetCenterpoint() * unselectedScale.x);
        holder.transform.position += (targetPos - holder.transform.position) * handPosApproachFactor;
    }

    public void SetBasePosition(IntVector2 pos)
    {
        centerCoord = new Coord(pos.x, pos.y);
        holder.transform.position = Services.MapManager.Map[centerCoord.x, centerCoord.y].transform.position;
    }

    public void SetTileCoords(Coord centerPos)
    {
        //Coord oldCenter = centerCoord;
        centerCoord = centerPos;

        foreach(Tile tile in tiles)
        {
            tile.SetCoord(tile.relativeCoord.Add(centerPos));
        }

        //foreach (KeyValuePair<Tile, Coord> tileCoord in tileRelativeCoords)
        //{
        //    tileCoord.Key.SetCoord(tileCoord.Value.Add(centerPos));
        //}
        //foreach(Tile tile in tiles)
        //{
        //    tile.SetCoord(centerCoord.Add(tile.coord.Subtract(oldCenter)));
        //}
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
        holder.gameObject.name = holderName;
        holder.legalityOverlay.enabled = false;
        holder.icon.enabled = false;
        tooltips = new List<Tooltip>();
        adjacentPieces = new List<Polyomino>();

        if (piece == null) return;
        //tileRelativeCoords = new Dictionary<Tile, Coord>();

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    Tile newTile = MonoBehaviour.Instantiate(Services.Prefabs.Tile, holder.transform);

                    Coord myCoord = new Coord(-2 + x, -2 + y);
                    newTile.Init(myCoord, this);
                    //tileRelativeCoords[newTile] = myCoord;

                    string pieceName = newTile.name.Replace("(Clone)", "");
                    newTile.name = pieceName;
                    newTile.SetBaseTileColor(owner, buildingType);

                    tiles.Add(newTile);
                }
            }
        }
        SetSprites();
        if (buildingType != BuildingType.BASE)
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

    public Vector3 GetCenterpoint(bool centerTile)
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

    public Vector3 GetCenterpoint()
    {
        return GetCenterpoint(false);
    }

    protected void EnterUnselectedState(bool allowAIPlayers)
    {
        ListenForInput(allowAIPlayers);
        ScaleHolder(unselectedScale);
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
        Vector3 touchWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position);
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
            //IncrementSortingOrder(30000);
            SortOnSelection(true);
            //CreateRotationUI();
            OnInputDrag(holder.transform.position);
            Services.AudioManager.CreateTempAudio(Services.Clips.PiecePicked, 1);

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

    public virtual void OnInputUp()
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
          
            if (IsPlacementLegal() && affordable && !owner.gameOver)
            {
                PlaceAtCurrentLocation();
            }
            else
            {
                owner.CancelSelectedPiece();
                EnterUnselectedState(false);
                //ToggleCostUIStatus(true);
            }
            CleanUpUI();
            //DestroyRotationUI();
            //IncrementSortingOrder(-30000);
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
            //RepositionRotationUI(screenInputPos);
            Vector3 screenOffset;
            if (owner.playerNum == 1)
            {
                screenOffset = baseDragOffset + ((1 - (2 * baseDragOffset.y / Screen.height))
                    * screenInputPos.y * Vector3.up);
            }
            else
            {
                screenOffset = -baseDragOffset + ((1 - (2 * baseDragOffset.y / Screen.height)) 
                    * (Screen.height - screenInputPos.y) * Vector3.down);
            }
            Vector3 offsetInputPos = Services.GameManager.MainCamera.ScreenToWorldPoint(
                screenInputPos + screenOffset);
            if (owner is AIPlayer) offsetInputPos = inputPos;
            Coord roundedInputCoord = new Coord(
                Mathf.RoundToInt(offsetInputPos.x),
                Mathf.RoundToInt(offsetInputPos.y));
            SetTileCoords(roundedInputCoord);
            Reposition(new Vector3(
                roundedInputCoord.x,
                roundedInputCoord.y,
                holder.transform.position.z));
            QueuePosition(roundedInputCoord);
        }

        if(!Services.Main.disableUI) SetLegalityGlowStatus();
    }

    protected virtual void CleanUpUI()
    {
        UnhighlightPotentialStructureClaims();
    }

    public virtual void SetLegalityGlowStatus()
    {
        //if (affordable || this is Blueprint)
        //{
        if (!(this is Blueprint)) SetAffordableStatus(owner);
        bool isLegal = IsPlacementLegal();
        foreach (Tile tile in tiles)
        {
            tile.ToggleIllegalLocationIcon(false);
        }
        if (isLegal && (affordable || this is Blueprint))
        {
            SetGlow(Services.UIManager.legalGlowColor);
            holder.legalityOverlay.enabled = false;
        }
        else if (isLegal && !affordable && !(this is Blueprint))
        {
            SetGlow(Color.yellow);
            //TurnOffGlow();
            //legalityOverlay.SetStatus(true);
            //legalityOverlay.SetSprite(Services.UIManager.notEnoughResourcesIcon);
            holder.legalityOverlay.enabled = false;
        }
        else
        {
            SetGlow(new Color(1, 0.2f, 0.2f));
            //TurnOffGlow();
            bool connected = IsConnected();
            if(!(this is Blueprint) && !connected)
            {
                //legalityOverlay.SetStatus(true);
                holder.legalityOverlay.enabled = false;
                //legalityOverlay.SetSprite(Services.UIManager.notConnectedIcon);
            }
            else
            {
                holder.legalityOverlay.enabled = false;
                List<Tile> illegalTiles = GetIllegalTiles();
                foreach(Tile tile in tiles)
                {
                    tile.ToggleIllegalLocationIcon(illegalTiles.Contains(tile));
                }
            }
        }
        UnhighlightPotentialStructureClaims();
        highlightedStructures = new List<Structure>();
        List<Structure> adjStructures = GetAdjacentStructures();
        if (isLegal)
        {
            foreach (Structure structure in adjStructures)
            {
                if (structure.owner == null)
                {
                    structure.SetGlow(owner.ColorScheme[0]);
                    highlightedStructures.Add(structure);
                }
            }
        }
        //}
    }

    protected void UnhighlightPotentialStructureClaims()
    {
        if (highlightedStructures != null)
        {
            foreach (Structure structure in highlightedStructures)
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
            int spriteIndex = 15;
            Coord[] directions = Coord.Directions();
            for (int i = 0; i < directions.Length; i++)
            {
                Coord adjCoord = tile.relativeCoord.Add(directions[i]);
                if (tileRelativeCoords.Contains(adjCoord))
                {
                    spriteIndex -= Mathf.RoundToInt(Mathf.Pow(2, i));
                }
            }
            tile.SetSprite(spriteIndex);
            SetRelativeSortingOrder(tile);
        }
    }

    private void SetRelativeSortingOrder(Tile tile)
    {
        tile.SetSortingOrder(baseSortingOrder - tile.coord.x - (100 * tile.coord.y));
    }

    protected void CheckTouchForRotateInput(TouchDown e)
    {
        if (Vector2.Distance(
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position),
            Services.GameManager.MainCamera.ScreenToWorldPoint(Input.GetTouch(touchID).position))
            < rotationInputRadius)
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
            //Coord prevRelCoord = tileRelativeCoords[tile];
            Coord prevRelCoord = tile.relativeCoord;
            int newXCoord = Mathf.RoundToInt(
                prevRelCoord.x * Mathf.Cos(rotAngle)
                - (prevRelCoord.y * Mathf.Sin(rotAngle)));
            int newYCoord = Mathf.RoundToInt(
                prevRelCoord.x * Mathf.Sin(rotAngle)
                + (prevRelCoord.y * Mathf.Cos(rotAngle)));
            //tileRelativeCoords[tile] = new Coord(newXCoord, newYCoord);
            tile.relativeCoord = new Coord(newXCoord, newYCoord);
        }
        SetTileCoords(centerCoord);
        numRotations = (numRotations + 1) % 4;
        if ((this == owner.selectedPiece || placed) && owner is AIPlayer)
        {
            Debug.Log("rotating while selected or placed");
            //Debug.Break();
        }        
        if (!dataOnly)
        {
            SetTileSprites();

            if (relocate)
            {
                foreach (Tile tile in tiles)
                {
                    //tile.transform.localPosition = 
                    //new Vector3(tileRelativeCoords[tile].x, tileRelativeCoords[tile].y);
                    tile.transform.localPosition = 
                        new Vector3(tile.relativeCoord.x, tile.relativeCoord.y);
                }
            }

            SetIconSprite();
            if(!Services.Main.disableUI) SetLegalityGlowStatus();
            //Quaternion prevRotation = spriteOverlay.transform.localRotation;
            //spriteOverlay.transform.localRotation = Quaternion.Euler(prevRotation.eulerAngles.x,
            //    prevRotation.eulerAngles.y, prevRotation.eulerAngles.z + 90);
            SetOverlaySprite();
        }
    }

    public virtual void PlaceAtLocation(Coord centerCoordLocation)
    {
        PlaceAtLocation(centerCoordLocation, false);
    }

    public virtual void PlaceAtLocation(Coord centerCoordLocation, bool replace)
    {
        SetTileCoords(centerCoordLocation);
        Reposition(new Vector3(
            centerCoordLocation.x,
            centerCoordLocation.y,
            holder.transform.position.z));
        PlaceAtCurrentLocation(replace);
    }
    
    public virtual void Update()
    {
        if (shieldDurationRemaining >= 0) DecayShield();
    }

    public void ScaleHolder(Vector3 scale)
    {
        holder.transform.localScale = scale;
    }

    //protected void UpdateDrawMeter()
    //{
    //    drawMeter += drawRate * Time.deltaTime;
    //    if (drawMeter >= 1)
    //    {
    //        OnDraw();
    //        Services.AudioManager.CreateTempAudio(Services.Clips.PlayAvailable[0], 1);
    //        drawMeter -= 1;
    //    }
    //}

    protected virtual void OnDraw()
    {
        owner.DrawPieces(1, holder.transform.position);
    }

    //protected void UpdateResourceMeter()
    //{
    //    resourceGainMeter += resourceIncrementRate * Time.deltaTime;
    //    if (resourceGainMeter >= 1)
    //    {
    //        int resourcesGained = owner.GainResources(resourcesPerIncrement);
    //        resourceGainMeter -= 1;
    //        Services.GeneralTaskManager.Do(new FloatText("+" + resourcesGained,
    //            GetCenterpoint(), owner, 3, 0.75f));
    //        Services.GeneralTaskManager.Do(new ResourceGainAnimation(resourcesGained, 
    //            holder.transform.position, owner.playerNum));
    //    }
    //}

    public virtual void TogglePieceConnectedness(bool connected_)
    {
        if (!(this is Structure) && !(this is Blueprint))
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

    protected void SetSprites()
    {
        SetIconSprite();
        SetOverlaySprite();
        SetTileSprites();
    }

    protected virtual string GetName()
    {
        return "";
    }

    protected virtual string GetDescription()
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
        //IncrementSortingOrder(10000);
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

    void QueuePosition(Coord pos)
    {
        if(lastPositions.Count >= (framesBeforeLockIn + leniencyFrames))
        {
            lastPositions.Dequeue();
        }
        lastPositions.Enqueue(pos);
    }

    protected void SortOverlay()
    {
        holder.spriteBottom.sortingOrder = (-centerCoord.x * 10) - (centerCoord.y * 1000);
        holder.dropShadow.sortingOrder = holder.spriteBottom.sortingOrder + 1;
        holder.icon.sortingOrder = holder.spriteBottom.sortingOrder + 2;
    }
}

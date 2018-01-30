using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BuildingType
{ BASE,FACTORY, MINE, STRUCTURE,NONE, BOMBFACTORY, MININGDRILL, ASSEMBLYLINE, FORTIFIEDSTEEL,
    BIGGERBRICKS, BIGGERBOMBS, SPLASHDAMAGE, SHIELDEDPIECES}


public class Polyomino
{
    public List<Tile> tiles = new List<Tile>();
    protected Dictionary<Tile, Coord> tileRelativeCoords;
    protected SpriteRenderer holderSr;
    protected SpriteRenderer iconSr;
    protected SpriteRenderer spriteOverlay;
    protected string holderName;
    public Transform holder { get; protected set; }
    public BuildingType buildingType { get; protected set; }

    public int index { get; protected set; }
    public int units { get; protected set; }
    protected int variations;
    protected int[,,] piece;
    public Player owner { get; protected set; }
    public Color baseColor { get; private set; }
    public Coord centerCoord;
    protected bool placed;
    private const float rotationInputRadius = 8f;
    protected int touchID;
    private readonly Vector3 baseDragOffset = 5f * Vector3.right;
    private readonly Vector3 unselectedScale = 0.5f * Vector3.one;
    public const float drawAnimDur = 0.5f;
    public const float deathAnimDur = 0.5f;
    public const float deathAnimScaleUp = 1.5f;
    public const float resourceGainAnimDur = 0.5f;
    public const float resourceGainAnimStaggerTime = 0.06f;
    public const float resourceGainAnimNoiseSpeed = 1;
    public const float resourceGainAnimNoiseMag = 4;
    private const float handPosApproachFactor = 0.25f;
    public const float burnPieceDuration = 0.5f;
    public static Vector3 burnPieceOffset = new Vector3(3, 0, 0);


    public bool isFortified;
    public List<Blueprint> occupyingBlueprints { get; protected set; }
    protected float resourceGainRateBonus;
    protected float normalDrawRateBonus;
    protected float destructorDrawRateBonus;
    public int cost { get; protected set; }
    protected TextMesh costText;
    private bool affordable;
    private bool isVisible;
    public bool connected { get; private set; }
    public bool dead { get; private set; }
    public float shieldDurationRemaining { get; private set; }
    private GameObject shield;
    protected List<Tooltip> tooltips;

    protected readonly IntVector2 Center = new IntVector2(2, 2);

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

    protected static int[,,] triomino = new int[3, 5, 5]
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
            },
            //  #
            // ##
            {

                { 0,0,0,0,0 },
                { 0,0,0,0,0 },
                { 0,0,1,0,0 },
                { 0,1,1,0,0 },
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
        isFortified = false;
        cost = units * 10;
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
            costText.color = (Color.green + Color.black) / 2;
            SetTint(new Color(baseColor.r, baseColor.g, baseColor.b, 1), 1);
            //if (isVisible) EnterUnselectedState();
        }
        else
        {
            costText.color = (Color.red + Color.black) / 2;
            SetTint(new Color(baseColor.r, baseColor.g, baseColor.b, 0.7f), 1);
            //HideFromInput();
        }
        SetGlowState(affordable);
    }

    protected void ToggleCostUIStatus(bool status)
    {
        costText.gameObject.SetActive(status);
    }

    public void SetVisible(bool isVisible_)
    {
        isVisible = isVisible_;
        if (!placed)
        {
            holder.gameObject.SetActive(isVisible);
            foreach (Tile tile in tiles)
            {
                tile.enabled = isVisible;
            }
            if (isVisible && owner != null && (affordable || this is Blueprint))
            {
                EnterUnselectedState();
            }
            else
            {
                EnterUnselectedState();
                HideFromInput();
            }
        }
    }

    public bool IsWithinBounds()
    {
        bool withinBounds = false;
        foreach (Tile tile in tiles)
        {
            if (0 <= tile.coord.x && tile.coord.x < Services.MapManager.MapWidth &&
               0 <= tile.coord.y && tile.coord.y < Services.MapManager.MapLength)
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

    public virtual void PlaceAtCurrentLocation(bool replace)
    {
        //place the piece on the board where it's being hovered now
        placed = true;
        OnPlace();
        foreach (Tile tile in tiles)
        {
            Coord tileCoord = tile.coord;
            Services.MapManager.Map[tileCoord.x, tileCoord.y].SetOccupyingPiece(this);
        }

        if (owner != null)
        {
            bool autoFortify = owner.autoFortify;
            owner.OnPiecePlaced(this);
            if (autoFortify && !isFortified)
                Services.MapManager.FortifyPiece(this);
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
        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
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

        return adjacentPieces;
    }

    public List<Polyomino> GetAdjacentPolyominos()
    {
        List<Polyomino> adjacentPieces = new List<Polyomino>();
        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
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
        }

        return adjacentPieces;
    }

    public virtual bool CheckForFortification(bool isBeingDestroyed)
    {
        List<Tile> emptyAdjacentTiles = new List<Tile>();

        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
                {
                    Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                    if (!adjTile.IsOccupied() && !emptyAdjacentTiles.Contains(adjTile))
                    {
                        emptyAdjacentTiles.Add(adjTile);
                    }
                }
            }
        }

        return Services.MapManager.CheckForFortification(this, emptyAdjacentTiles, isBeingDestroyed);
    }

    public virtual bool IsPlacementLegal()
    {
        //determine if the pieces current location is a legal placement
        //CONDITIONS:
        //is contiguous with a structure connected to either the base or a fortification
        //doesn't overlap with any existing pieces or is a destructor\
        List<Polyomino> adjacentPieces = GetAdjacentPolyominos(owner);
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
            //if (!Services.MapManager.ConnectedToBase(this, new List<Polyomino>())) return false;
            if (Services.MapManager.Map[tile.coord.x, tile.coord.y].IsOccupied()) return false;
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
        else TurnOffGlow();
    }

    public void TurnOffGlow()
    {
        //if (placed && isFortified) return;
        foreach (Tile tile in tiles)
        {
            tile.SetMaskSrAlpha(0);
        }
    }

    public void SetGlow(Color color)
    {
        foreach (Tile tile in tiles)
        {
            tile.SetMaskColor(color);
            tile.SetMaskSrAlpha(0.5f);
            //tile.SetGlowOutLine(10);
            //tile.SetGlowColor(color);
        }
    }


    public void SetTint(Color color, float tintProportion)
    {
        foreach (Tile tile in tiles)
        {
            tile.SetColor((baseColor * (1 - tintProportion)) + (color * tintProportion));
        }
    }

    public void ToggleAltColor(bool useAlt)
    {
        foreach (Tile tile in tiles)
        {
            tile.ToggleAltColor(useAlt);
        }
    }

    public void ShiftColor(Color color)
    {
        foreach (Tile tile in tiles) tile.ShiftColor(color);
    }

    protected virtual void OnPlace()
    {
        //do whatever special stuff this piece does when you place it 
        //(e.g. destroy overlapping pieces for a destructor)
        foreach (Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.HasResource())
            {
                owner.AddPieceToHand(mapTile.occupyingResource);
            }
        }
        Services.AudioManager.CreateTempAudio(Services.Clips.PiecePlaced, 1);
        ToggleCostUIStatus(false);
        holder.localScale = Vector3.one;
        if (owner.shieldedPieces) CreateShield();
    }

    //  Have a fortification method
    //  When I am fortified, replace me with monominos at my tiles locations, then delete me

    public void FortifyPiece()
    {
        foreach (Tile tile in tiles)
        {
            Polyomino monomino = new Polyomino(1, 0, owner);
            //monomino.ToggleAltColor(true);
            monomino.MakePhysicalPiece(true);
            monomino.PlaceAtLocation(tile.coord);
        }
    }

    void CreateShield()
    {
        shieldDurationRemaining = ShieldedPieces.ShieldDuration;
        shield = GameObject.Instantiate(Services.Prefabs.Shield, 
            GetCenterpoint(), Quaternion.identity);
    }

    void DecayShield()
    {
        shieldDurationRemaining -= Time.deltaTime;
        float shieldDecayed = ShieldedPieces.ShieldDuration - shieldDurationRemaining;
        shield.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero,
            Mathf.Pow(EasingEquations.Easing.ExpoEaseIn(
                shieldDecayed / ShieldedPieces.ShieldDuration), 2));
        if (shieldDurationRemaining <= 0) GameObject.Destroy(shield);
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
            Services.GeneralTaskManager.Do(die);
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

        //CheckForFortification(true);

        //if (ringTimer != null) RemoveTimerUI();
        if (shield != null) GameObject.Destroy(shield);
        owner.OnPieceRemoved(this);
        dead = true;
    }

    void DestroyThis()
    {
        GameObject.Destroy(holder.gameObject);
    }

    public void Reposition(Vector3 pos)
    {
        holder.position = pos;
        //change localposition of the piece container in player UI to value
    }

    public void ApproachHandPosition(Vector3 targetPos)
    {
        holder.position += (targetPos - holder.position) * handPosApproachFactor;
    }

    public void SetBasePosition(IntVector2 pos)
    {
        centerCoord = new Coord(pos.x, pos.y);
        holder.position = Services.MapManager.Map[centerCoord.x, centerCoord.y].transform.position;
    }

    public void SetTileCoords(Coord centerPos)
    {
        centerCoord = centerPos;
        foreach (KeyValuePair<Tile, Coord> tileCoord in tileRelativeCoords)
        {
            tileCoord.Key.SetCoord(tileCoord.Value.Add(centerPos));
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

    public virtual void MakePhysicalPiece(bool isViewable)
    {
        holder = GameObject.Instantiate(Services.Prefabs.PieceHolder,
            Services.GameScene.transform).transform;
        holder.gameObject.name = holderName;
        holderSr = holder.gameObject.GetComponent<SpriteRenderer>();
        spriteOverlay = holder.GetComponentsInChildren<SpriteRenderer>()[2];
        costText = holder.gameObject.GetComponentInChildren<TextMesh>();
        costText.text = cost.ToString();
        tooltips = new List<Tooltip>();
        if (owner != null)
        {
            Quaternion rot = owner.playerNum == 1 ?
                Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, 90);
            costText.transform.localRotation = rot;
            Vector3 localPos = costText.transform.localPosition;
            if (owner.playerNum != 1)
            {
                costText.transform.localPosition =
                    new Vector3(-localPos.x, localPos.y, localPos.z);
            }
        }

        if (piece == null) return;
        tileRelativeCoords = new Dictionary<Tile, Coord>();

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    Tile newpiece = MonoBehaviour.Instantiate(Services.Prefabs.Tile, holder);

                    Coord myCoord = new Coord(-2 + x, -2 + y);
                    newpiece.Init(myCoord, this);
                    tileRelativeCoords[newpiece] = myCoord;

                    string pieceName = newpiece.name.Replace("(Clone)", "");
                    newpiece.name = pieceName;
                    newpiece.ActivateTile(owner, buildingType);

                    tiles.Add(newpiece);
                }
            }
        }
        SetSprites();
        if (buildingType != BuildingType.BASE)
        {
            //SetVisible(isViewable);
            EnterUnselectedState();
        }
    }

    protected virtual void SetIconSprite()
    {
        iconSr = holder.gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
        iconSr.transform.position = GetCenterpoint();
        if (buildingType == BuildingType.BASE) iconSr.sprite = Services.UIManager.baseIcon;
        else iconSr.enabled = false;
    }

    protected Vector3 GetCenterpoint(bool centerTile)
    {
        Vector3 centerPos = Vector3.zero;
        foreach (Tile tile in tiles)
        {
            centerPos += tile.transform.position;
        }
        centerPos /= tiles.Count;
        if (centerTile)
        {
            Tile closestTile = null;
            float closestDistance = Mathf.Infinity;
            foreach (Tile tile in tiles)
            {
                float dist = Vector3.Distance(tile.transform.position, centerPos);
                if(dist < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = dist;
                }
            }
            return closestTile.transform.position;
        }

        return centerPos;
    }

    protected Vector3 GetCenterpoint()
    {
        return GetCenterpoint(false);
    }

    public void SetPieceState(bool playAvailable)
    {
        if (playAvailable) EnterUnselectedState();
        else HideFromInput();
        SetGlowState(playAvailable);
    }

    protected void EnterUnselectedState()
    {
        ListenForInput();
        holder.localScale = unselectedScale;
    }

    protected void ListenForInput()
    {
        if (!(owner is AIPlayer))
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
        Debug.Assert(holderSr != null);
        Vector3 extents;
        Vector3 centerPoint;
        if (!placed)
        {
            extents = holderSr.bounds.extents;
            centerPoint = holder.position;
        }
        else
        {
            extents = spriteOverlay.bounds.extents;
            centerPoint = spriteOverlay.transform.position;
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
            OnInputDown();
        }
    }

    protected void OnMouseDownEvent(MouseDown e)
    {
        Vector3 mouseWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
        if (IsPointContainedWithinHolderArea(mouseWorldPos) && (owner == null || owner.selectedPiece == null))
        {
            OnInputDown();
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

    public virtual void OnInputDown()
    {
        if (!owner.gameOver && !placed)
        {
            holder.localScale = Vector3.one;
            holder.localPosition = new Vector3(holder.transform.position.x, holder.transform.position.y, -4);
            owner.OnPieceSelected(this);
            IncrementSortingOrder(20);
            OnInputDrag(holder.position);
            ToggleCostUIStatus(false);
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
                EnterUnselectedState();
                ToggleCostUIStatus(true);
                CleanUpUI();
            }
            IncrementSortingOrder(-20);
            holder.localPosition = new Vector3(holder.transform.position.x, holder.transform.position.y, 0);
        }
    }

    public virtual void OnInputDrag(Vector3 inputPos)
    {
        if (!placed && !owner.gameOver)
        {
            Vector3 screenInputPos = 
                Services.GameManager.MainCamera.WorldToScreenPoint(inputPos);
            Vector3 screenOffset;
            if (owner is AIPlayer) screenOffset = Vector3.zero;
            else if (owner.playerNum == 1)
            {
                screenOffset = baseDragOffset + ((1 - (2 * baseDragOffset.x / Screen.width))
                    * screenInputPos.x * Vector3.right);
            }
            else
            {
                screenOffset = -baseDragOffset + ((1 - (2 * baseDragOffset.x / Screen.width)) 
                    * (Screen.width - screenInputPos.x) * Vector3.left);
            }
            Vector3 offsetInputPos = Services.GameManager.MainCamera.ScreenToWorldPoint(
                screenInputPos + screenOffset);
            Coord roundedInputCoord = new Coord(
                Mathf.RoundToInt(offsetInputPos.x),
                Mathf.RoundToInt(offsetInputPos.y));
            SetTileCoords(roundedInputCoord);
            Reposition(new Vector3(
                roundedInputCoord.x,
                roundedInputCoord.y,
                holder.position.z));
        }

        SetLegalityGlowStatus();
    }

    protected virtual void CleanUpUI() { }

    public virtual void SetLegalityGlowStatus()
    {
        //if (affordable || this is Blueprint)
        //{
        if(!(this is Blueprint)) SetAffordableStatus(owner);
        bool isLegal = IsPlacementLegal();
            if (isLegal && (affordable || this is Blueprint))
            {
                SetGlow(new Color(0.2f, 1.5f, 0.2f));
            }
            else
            {
                SetGlow(new Color(1.5f, 0.2f, 0.2f));
            }
        //}
    }

    protected void SetTileSprites()
    {
        foreach (Tile tile in tiles)
        {
            int spriteIndex = 15;
            Coord[] directions = Coord.Directions();
            for (int i = 0; i < directions.Length; i++)
            {
                Coord adjCoord = tileRelativeCoords[tile].Add(directions[i]);
                if (tileRelativeCoords.ContainsValue(adjCoord))
                {
                    spriteIndex -= Mathf.RoundToInt(Mathf.Pow(2, i));
                }
            }
            tile.SetSprite(spriteIndex);
        }
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

    public virtual void Rotate()
    {
        float rotAngle = 90 * Mathf.Deg2Rad;
        foreach (Tile tile in tiles)
        {
            Coord prevRelCoord = tileRelativeCoords[tile];
            int newXCoord = Mathf.RoundToInt(
                prevRelCoord.x * Mathf.Cos(rotAngle)
                - (prevRelCoord.y * Mathf.Sin(rotAngle)));
            int newYCoord = Mathf.RoundToInt(
                prevRelCoord.x * Mathf.Sin(rotAngle)
                + (prevRelCoord.y * Mathf.Cos(rotAngle)));
            tileRelativeCoords[tile] = new Coord(newXCoord, newYCoord);
        }
        SetTileCoords(centerCoord);
        SetTileSprites();
        foreach (Tile tile in tiles)
        {
            tile.transform.position = new Vector3(tile.coord.x, tile.coord.y);
        }
        SetIconSprite();
        SetLegalityGlowStatus();
        Quaternion prevRotation = spriteOverlay.transform.localRotation;
        spriteOverlay.transform.localRotation = Quaternion.Euler(prevRotation.eulerAngles.x,
            prevRotation.eulerAngles.y, prevRotation.eulerAngles.z + 90);
        SetOverlaySprite();
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
            holder.position.z));
        PlaceAtCurrentLocation(replace);
    }

    //protected void CreateTimerUI()
    //{
    //    GameObject timerObj = GameObject.Instantiate(Services.Prefabs.RingTimer, 
    //        Services.UIManager.canvas);
    //    ringTimer = timerObj.GetComponentsInChildren<Image>()[1];
    //    ringTimer.fillAmount = 0;
    //    timerObj.transform.position =
    //        Services.GameManager.MainCamera.WorldToScreenPoint(GetCenterpoint());
    //}

    //protected void RemoveTimerUI()
    //{
    //    GameObject.Destroy(ringTimer.transform.parent.gameObject);
    //}

    public virtual void Update()
    {
        if (shield != null) DecayShield();
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

    public void TogglePieceConnectedness(bool connected_)
    {
        if (connected_)
        {
            ShiftColor(owner.ColorScheme[0]);
        }
        else
        {
            ShiftColor(owner.ColorScheme[1]);
        }
        connected = connected_;
        for (int i = 0; i < occupyingBlueprints.Count; i++)
        {
            occupyingBlueprints[i].TogglePieceConnectedness(connected_);
        }
    }

    protected void IncrementSortingOrder(int increment)
    {
        foreach (Tile tile in tiles) tile.IncrementSortingOrder(increment);
        iconSr.sortingOrder += increment;
        spriteOverlay.sortingOrder += increment;
    }

    protected virtual void SetOverlaySprite()
    {
        Color overlayColor;
        if(owner != null)
        {
            overlayColor = (owner.ColorScheme[0] + Color.black) / 2;
        }
        else
        {
            overlayColor = (Services.GameManager.NeutralColor + Color.black) / 2;
        }
        spriteOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0.75f);
        spriteOverlay.transform.position = GetCenterpoint();
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
            Services.GameEventManager.Register<TouchDown>(OnTouchDown);

            Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
            Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        }
    }

    public void BurnFromHand()
    {
        HideFromInput();
        IncrementSortingOrder(20);
        BurnPiece burnTask = new BurnPiece(this);
        burnTask.Then(new ActionTask(DestroyThis));
        Services.GeneralTaskManager.Do(burnTask);
    }
}

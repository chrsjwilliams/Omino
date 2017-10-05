using System.Collections.Generic;
using UnityEngine;

public class Polyomino
{

    public bool isBase;
    public List<Tile> tiles = new List<Tile>();
    private Dictionary<Coord, Tile> tileRelativeCoords;
    public GameObject holder { get; protected set; }
    private string holderName;

    protected int index;
    protected int variations;
    protected int[,,] piece;
    protected Player owner;
    public Coord centerCoord;

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

    protected static int[,,] tetromino = new int[5, 5, 5]
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

    public Polyomino(int _units, int _index, Player _player)
    {
        index = _index;
        owner = _player;

        isBase = false;

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
                holderName = "Player " + owner.playerNum + " Base";
                isBase = true;
                piece = playerBase;
                break;
            default:
                break;
        }
    }

    ~Polyomino()
    {
        Services.GameEventManager.Unregister<PlacePieceEvent>(OnPlacePiece);
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

    protected void OnPlacePiece(PlacePieceEvent e)
    {
        //  This stops the pieces from being connected to the player
        //  Check if all pieces are on the map
        //  Debug.Log("Bounds: " + IsWithinBounds() + "| Legal: " + IsPlacementLegal() + "| Base: " + isBase);
        if (IsWithinBounds() && IsPlacementLegal() && !isBase)
        {
            //  Will put the logic below in a method to set occupancy
            foreach(Tile tile in tiles)
            {
                Services.MapManager.Map[tile.coord.x, tile.coord.y].SetOccupyingPiece(this);
            }
            //  Will put logic above in a method to set occupancy

            if (e.player.playerNum == 1)
            {
                holder.transform.parent = Services.GameManager.ownedByPlayer1.transform;
            }
            else if (e.player.playerNum == 2)
            {
                holder.transform.parent = Services.GameManager.ownedByPlayer2.transform;
            }
            Services.GameEventManager.Unregister<PlacePieceEvent>(OnPlacePiece);
        }
        else
        {
            //  Some sort of negative feedback
        }
    }

    private void GivePlayerOwnershipOfBase()
    {
        if (owner.playerNum == 1)
        {
            holder.transform.parent = Services.GameManager.ownedByPlayer1.transform;
        }
        else if (owner.playerNum == 2)
        {
            holder.transform.parent = Services.GameManager.ownedByPlayer2.transform;
        }
        Services.GameEventManager.Unregister<PlacePieceEvent>(OnPlacePiece);
    }

    public void PlaceAtCurrentLocation()
    {
        //place the piece on the board where it's being hovered now
        OnPlace();
        holder.transform.parent = Services.MapManager.Map[centerCoord.x, centerCoord.y].transform;
    }

    public virtual bool IsPlacementLegal()
    {
        //determine if the pieces current location is a legal placement
        //CONDITIONS:
        //is contiguous with a structure connected to either the base or a fortification
        //doesn't overlap with any existing pieces or is a destructor\
        bool isLegal = false;
        Debug.Log("tile count: " + tiles.Count);
        foreach(Tile tile in tiles)
        {
            if (!isLegal && Services.MapManager.ValidateTile(tile))
                isLegal = true;

            if (Services.MapManager.Map[tile.coord.x, tile.coord.y].IsOccupied())
            {
                Debug.Log("space occupied");
                return false;
            }
        }

        return isLegal;
    }

    protected virtual void OnPlace()
    {
        //do whatever special stuff this piece does when you place it 
        //(e.g. destroy overlapping pieces for a destructor)
    }

    public void Reposition(Vector3 pos)
    {
        holder.transform.localPosition = pos;
        //change localposition of the piece container in player UI to value
    }

    public void SetBasePosition(IntVector2 pos)
    {
        float worldSpaceOffset = 2.0f;
        centerCoord = new Coord(pos.x, pos.y);
        Vector3 tilePos = Services.MapManager.Map[centerCoord.x, centerCoord.y].transform.position;
        tilePos = new Vector3(tilePos.x, tilePos.y , tilePos.z - worldSpaceOffset);
        holder.transform.position = tilePos;
    }

    public void SetTileCoords(Coord centerPos)
    {
        centerCoord = centerPos;
        foreach(KeyValuePair<Coord,Tile> tileCoord in tileRelativeCoords)
        {
            tileCoord.Value.SetCoord(tileCoord.Key.Add(centerPos));
        }
    }

    public void MakePhysicalPiece()
    {
        holder = new GameObject();
        holder.transform.position = Vector3.zero;

        holder.name = holderName;
        Services.GameEventManager.Register<PlacePieceEvent>(OnPlacePiece);

        if (piece == null) return;
        if (isBase)
            GivePlayerOwnershipOfBase();
        else
        {
            holder.transform.parent = owner.uiArea;
            holder.transform.localPosition = new Vector3(holder.transform.position.x, holder.transform.position.y, 0);
        }

        tileRelativeCoords = new Dictionary<Coord, Tile>();

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    Tile newpiece = MonoBehaviour.Instantiate(Services.Prefabs.Tile);

                    Coord myCoord = new Coord(-2 + x, -2 + y);
                    newpiece.transform.parent = holder.transform;
                    newpiece.Init(myCoord, owner.playerNum);
                    tileRelativeCoords[myCoord] = newpiece;

                    string pieceName = newpiece.name.Replace("(Clone)", "");
                    newpiece.name = pieceName;


                    newpiece.ActivateTile(owner);
                    tiles.Add(newpiece);
                }
            }
        }
    }
}

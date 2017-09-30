using System.Collections.Generic;
using UnityEngine;

public class Polyomino
{

    public List<Tile> tiles = new List<Tile>();
    public GameObject holder { get; protected set; }

    protected int index;
    protected int variations;
    protected int width;
    protected int length;
    protected int[,,] piece;
    protected Player owner;

    protected static int[,,] monomino = new int[1, 1, 1]
    {   
            //  These hashes represent what the piece will look like
            //  #
            {
                { 1}
            }
    };

    protected static int[,,] domino = new int[1, 1, 2]
        { 
            //  ##
            {
                { 1 ,  1}
            }
        };

    protected static int[,,] triomino = new int[2, 3, 3]
        { 
            //  ###
            {
                {1,1,1 },
                {0,0,0 },
                {0,0,0 }
            },
            //  #
            //  ##
            {

                {1,0,0 },
                {1,1,0 },
                {0,0,0 }
            }
        };

    protected static int[,,] tetromino = new int[5, 4, 4]
        { 
            //  ####
            {
                {1,1,1,1 },
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 }
            },
            //  #
            //  #
            //  ##
            {
                {1,0,0,0 },
                {1,0,0,0 },
                {1,0,0,0 },
                {1,1,0,0 }
            },
            //  #
            //  ##
            //  #
            {
                {1,0,0,0 },
                {1,1,0,0 },
                {1,0,0,0 },
                {0,0,0,0 }
            },
            //  ##
            //   ##
            {
                {1,1,0,0 },
                {0,1,1,0 },
                {0,0,0,0 },
                {0,0,0,0 }
            },
            //  ##
            //  ##
            {
                {1,1,0,0 },
                {1,1,0,0 },
                {0,0,0,0 },
                {0,0,0,0 }
            }
        };

    protected static int[,,] pentomino = new int[12, 5, 5]
        { 
            //  F Shape
            //   ##
            //  ##
            //   #
            {
                {0,1,1,0,0 },
                {1,1,0,0,0 },
                {0,1,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  I Shape
            //  #
            //  #
            //  #
            //  #
            //  #
            {
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,0,0,0,0 }
            },
            //  L Shape
            //  #
            //  #
            //  #
            //  ##
            {
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,1,0,0,0 },
                {0,0,0,0,0 }
            },
            //  N Shape
            //  ###
            //    ##
            {
                {1,1,1,0,0 },
                {0,0,1,1,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  P Shape
            //  ##
            //  ##
            //  #
            {
                {1,1,0,0,0 },
                {1,1,0,0,0 },
                {1,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  T Shape
            //  ###
            //   #
            //   #
            {
                {1,1,1,0,0 },
                {0,1,0,0,0 },
                {0,1,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  U Shape
            //  # #
            //  ###
            {
                {1,0,1,0,0 },
                {1,1,1,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  V Shape
            //  #
            //  #
            //  ###
            {
                {1,0,0,0,0 },
                {1,0,0,0,0 },
                {1,1,1,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  W Shape
            //  #
            //  ##
            //   ##
            {
                {1,0,0,0,0 },
                {1,1,0,0,0 },
                {0,1,1,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  X Shape
            //   #
            //  ###
            //   #
            {
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            },
            //  Y Shape
            //   #
            //  ##
            //   #
            //   #
            {
                {0,1,0,0,0 },
                {1,1,0,0,0 },
                {0,1,0,0,0 },
                {0,1,0,0,0 },
                {0,0,0,0,0 }
            },
            //  Z Shape
            //  ##
            //   #
            //   ##
            {
                {1,1,0,0,0 },
                {0,1,0,0,0 },
                {0,1,1,0,0 },
                {0,0,0,0,0 },
                {0,0,0,0,0 }
            }
        };

    protected static int[,,] playerBase = new int[1, 3, 3]
    {   
            //  These hashes represent what the piece will look like
            //  ###
            //  ###
            //  ###
            {
                {1, 1, 1},
                {1, 1, 1},
                {1, 1, 1 }
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

        holder = new GameObject();

        string holderName = "";

        index = _index;
        owner = _player;

        switch (_units)
        {
            case 1:
                holderName = "MonominoHolder";
                piece = monomino;
                width = 1;
                length = 1;
                break;
            case 2:
                holderName = "DominoHolder";
                piece = domino;
                width = 1;
                length = 2;
                break;
            case 3:
                holderName = "TriominoHolder";
                piece = triomino;
                width = 3;
                length = 3;
                break;
            case 4:
                holderName = "TetraominoHolder";
                piece = tetromino;
                width = 4;
                length = 4;
                break;
            case 5:
                holderName = "PentominoHolder";
                piece = pentomino;
                width = 5;
                length = 5;
                break;
            case 9:
                holderName = "Player " + owner.playerNum + " Base";
                piece = playerBase;
                width = 3;
                length = 3;
                break;
            default:
                break;
        }

        holder.name = holderName;
    }

    public bool WithinBounds()
    {
        bool withinBounds = false;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (0 < tiles[i].coord.x && tiles[i].coord.x < Services.MapManager.MapWidth - 1 &&
               0 < tiles[i].coord.y && tiles[i].coord.y < Services.MapManager.MapLength - 1)
            {
                withinBounds = true;
            }
        }

        return withinBounds;
    }

    protected void OnPlacePiece(PlacePieceEvent e)
    {
        //  This stops the pieces from being connected to the player
        //  Check if all pieces are on the map

        if (WithinBounds())
        {
            if (e.player.playerNum == 1)
            {
                holder.transform.parent = Services.GameManager.ownedByPlayer1.transform;
            }
            else if (e.player.playerNum == 2)
            {
                holder.transform.parent = Services.GameManager.ownedByPlayer2.transform;
            }
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
    }

    public void PlaceAtCurrentLocation()
    {
        //place the piece on the board where it's being hovered now
        OnPlace();
    }

    public virtual bool IsPlacementLegal()
    {
        //determine if the pieces current location is a legal placement
        //CONDITIONS:
        //is contiguous with a structure connected to either the base or a fortification
        //doesn't overlap with any existing pieces or is a destructor
        foreach(Tile tile in tiles)
        {

        }

        return false;
    }

    protected virtual void OnPlace()
    {
        //do whatever special stuff this piece does when you place it 
        //(e.g. destroy overlapping pieces for a destructor)
    }

    public void Reposition(Vector3 pos)
    {
        //change localposition of the piece container in player UI to value
    }

    public void SetPosition(IntVector2 pos)
    {
        float worldSpaceOffset = 1.0f;
        Coord myCoord = new Coord(pos.x, pos.y);
        Vector3 tilePos = Services.MapManager.Map[pos.x, pos.y].transform.position;
        tilePos = new Vector3(tilePos.x, tilePos.y , tilePos.z - worldSpaceOffset);
        holder.transform.position = tilePos;
    }

    public void MakePhysicalPiece()
    {
        //create all the tile gameobjects for the physical representation of the piece
        //put them inside a container to move around 
        if (piece == null) return;
        //  Parents the polyomino to the player so the player
        //  can move around the board with the piece
        if (holder.name.Contains("Base"))
            GivePlayerOwnershipOfBase();
        else
            holder.transform.parent = owner.cursor;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    //  A pieces is created like this to prevent ArrayOutOfBounds Exceptions 
                    //  This way the player cursor can move to any space on the map with any
                    //  any piece
                    Tile newpiece = MonoBehaviour.Instantiate(Services.Prefabs.Tile);

                    Coord myCoord;
                    if (holder.name.Contains("Base"))
                    {
                        myCoord = new Coord(x, y);
                    }
                    else
                    {
                        myCoord = new Coord(owner.cursorPos.X + x, owner.cursorPos.Y + y);
                    }

                    newpiece.Init(myCoord, owner.playerNum);

                    string pieceName = newpiece.name.Replace("(Clone)", "") + "[X: " + (owner.cursorPos.X + x) + ", Y: " + (owner.cursorPos.Y + y) + "]";
                    newpiece.name = pieceName;

                    newpiece.transform.parent = holder.transform;
                    Services.MapManager.Map[x, y].SetOccupyingPiece(this);

                    newpiece.ActivateTile(owner);
                    tiles.Add(newpiece);
                }
            }
        }
    }
    
}

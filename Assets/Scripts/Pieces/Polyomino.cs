using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{ BASE,FACTORY, MINE, NONE }


public class Polyomino
{
    public List<Tile> tiles = new List<Tile>();
    protected Dictionary<Tile, Coord> tileRelativeCoords;
    public GameObject holder { get; protected set; }
    protected string holderName;
    public BuildingType buildingType { get; protected set; }

    protected int index;
    protected int variations;
    protected int[,,] piece;
    public Player owner { get; protected set; }
    public Coord centerCoord;
    public bool selected { get; protected set; }
    protected bool placed;
    private const float rotationInputRadius = 8f;
	public int touchID;
	private readonly Vector3 dragOffset = 2f * Vector3.up;
    private readonly Vector3 unselectedScale = 0.675f * Vector3.one;

    public List<Blueprint> occupyingStructures { get; protected set; }

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

        occupyingStructures = new List<Blueprint>();

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
                holderName = "Player " + owner.playerNum + " Base";
                buildingType = BuildingType.BASE;
                piece = playerBase;
                break;
            default:
                break;
        }
    }

    public void SetPlaced(bool isPlaceed)
    {
        placed = isPlaceed;
    }

    public void SetVisible(bool isVisible)
    {
        if (!placed)
        {
            holder.SetActive(isVisible);
            foreach (Tile tile in tiles)
            {
                tile.enabled = isVisible;
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
        //place the piece on the board where it's being hovered now
        placed = true;
        OnPlace();
        foreach(Tile tile in tiles)
        {
            Coord tileCoord = tile.coord;
            Services.MapManager.Map[tileCoord.x, tileCoord.y].SetOccupyingPiece(this);
        }
    }

    public virtual bool IsPlacementLegal()
    {
        //determine if the pieces current location is a legal placement
        //CONDITIONS:
        //is contiguous with a structure connected to either the base or a fortification
        //doesn't overlap with any existing pieces or is a destructor\
        bool isLegal = false;
        foreach(Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            if (Services.MapManager.ValidateTile(tile, owner)) isLegal = true;
            if (Services.MapManager.Map[tile.coord.x, tile.coord.y].IsOccupied())
            {
                return false;
            }
        }

        return isLegal;
    }

    public void TurnOffGlow()
    {
        foreach (Tile tile in tiles)
        {
            tile.SetGlowOutLine(0);
        }
    }

    public void SetGlow(Color color)
    {
        foreach (Tile tile in tiles)
        {
            tile.SetGlowOutLine(10);
            tile.SetGlowColor(color);
        }
    }

    protected virtual void OnPlace()
    {
        //do whatever special stuff this piece does when you place it 
        //(e.g. destroy overlapping pieces for a destructor)
    }

    public virtual void Remove()
    {
		Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);
		Services.GameEventManager.Unregister<TouchMove> (OnTouchMove);
		for (int i = occupyingStructures.Count -1; i >= 0; i--)
        {
            occupyingStructures[i].Remove();
        }
        foreach (Tile tile in tiles)
        {
            Services.MapManager.Map[tile.coord.x, tile.coord.y].SetOccupyingPiece(null);
			tile.OnRemove ();
        }
        GameObject.Destroy(holder);
    }

    public void Reposition(Vector3 pos)
    {
        holder.transform.position = pos;
        //change localposition of the piece container in player UI to value
    }

    public void SetBasePosition(IntVector2 pos)
    {
        centerCoord = new Coord(pos.x, pos.y);
        holder.transform.position = Services.MapManager.Map[centerCoord.x, centerCoord.y].transform.position;
    }

    public void SetTileCoords(Coord centerPos)
    {
        centerCoord = centerPos;
        foreach(KeyValuePair<Tile,Coord> tileCoord in tileRelativeCoords)
        {
            tileCoord.Key.SetCoord(tileCoord.Value.Add(centerPos));
        }
    }

    public void AddOccupyingStructure(Blueprint blueprint)
    {
        occupyingStructures.Add(blueprint);
    }

    public void RemoveOccupyingStructure(Blueprint blueprint)
    {
        occupyingStructures.Remove(blueprint);
    }

    public virtual void MakePhysicalPiece(bool isViewable)
    {
        holder = GameObject.Instantiate(Services.Prefabs.PieceHolder, 
            Services.GameScene.transform);
        holder.name = holderName;

        if (piece == null) return;
        tileRelativeCoords = new Dictionary<Tile, Coord>();

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (piece[index, x, y] == 1)
                {
                    Tile newpiece = MonoBehaviour.Instantiate(Services.Prefabs.Tile, 
                        holder.transform);

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
        SetTileSprites();
        SetVisible(isViewable);

        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);

        if(buildingType != BuildingType.BASE) holder.transform.localScale = unselectedScale;
    }

    bool IsPointContainedWithinHolderArea(Vector3 point)
    {
        Vector3 extents = holder.GetComponent<SpriteRenderer>().bounds.extents;
        Vector3 centerPoint = holder.transform.position;
        return point.x >= centerPoint.x - extents.x && point.x <= centerPoint.x + extents.x &&
            point.y >= centerPoint.y - extents.y && point.y <= centerPoint.y + extents.y;
    }

    void OnTouchDown(TouchDown e)
    {
        Vector3 touchWorldPos = 
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position);
        Vector3 projectedTouchPos = 
            new Vector3(touchWorldPos.x, touchWorldPos.y, holder.transform.position.z);
        if (IsPointContainedWithinHolderArea(projectedTouchPos) && touchID == -1)
        {
            touchID = e.touch.fingerId;
            OnInputDown();
        }
    }

    void OnMouseDownEvent(MouseDown e)
    {
        Vector3 mouseWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
        Vector3 projectedMousePos =
            new Vector3(mouseWorldPos.x, mouseWorldPos.y, holder.transform.position.z);
        if (IsPointContainedWithinHolderArea(projectedMousePos))
        {
            OnInputDown();
        }
    }

    void OnTouchUp(TouchUp e)
    {
        if (e.touch.fingerId == touchID)
        {
            touchID = -1;
            OnInputUp();
        }
    }

    void OnMouseUpEvent(MouseUp e)
    {
        OnInputUp();
    }

    public virtual void OnInputDown()
    {
		if (!selected && !owner.gameOver && owner.selectedPiece == null)
        {
            selected = true;
            holder.transform.localScale = Vector3.one;
			if (!placed) {
				owner.OnPieceSelected (this);
				OnInputDrag (holder.transform.position);

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

    void OnMouseMoveEvent(MouseMove e)
    {
        OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos));
    }

	protected void OnTouchMove(TouchMove e){
		if (e.touch.fingerId == touchID) {
			OnInputDrag (Services.GameManager.MainCamera.ScreenToWorldPoint (e.touch.position));
		}
	}

    public virtual void OnInputUp()
    {
        if (selected)
        {
            selected = false;
            if (!placed)
            {
                Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
                Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);
                Services.GameEventManager.Unregister<TouchDown>(CheckTouchForRotateInput);

                Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
                Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
                if (IsPlacementLegal() && owner.placementAvailable && !owner.gameOver)
                {
                    PlaceAtCurrentLocation();
                    owner.OnPiecePlaced(this);
                }
                else
                {
                    owner.CancelSelectedPiece();
                    Services.GameEventManager.Register<TouchDown>(OnTouchDown);
                    Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
                    holder.transform.localScale = unselectedScale;
                }
            }
        }
    }

    public void OnInputDrag(Vector3 inputPos)
    {
        if (!placed && !owner.gameOver)
        {
			Vector3 offsetInputPos = inputPos + dragOffset;
			Coord roundedInputCoord = new Coord(
				Mathf.RoundToInt(offsetInputPos.x),
				Mathf.RoundToInt(offsetInputPos.y));
            SetTileCoords(roundedInputCoord);
            Reposition(new Vector3(
                roundedInputCoord.x,
                roundedInputCoord.y,
                holder.transform.position.z));
        }

        if (owner.placementAvailable)
        {
            if (IsPlacementLegal())
            {
                SetGlow(new Color(0.2f, 1.5f, 0.2f));
            }
            else
            {
                SetGlow(new Color(1.5f, 0.2f, 0.2f));
            }
        }
    }

    protected void SetTileSprites()
    {
        foreach(Tile tile in tiles)
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
        if(Vector2.Distance(
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position), 
            holder.transform.position) < rotationInputRadius)
        {
            Rotate();
        }
    }

    public void Rotate()
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
    }
}

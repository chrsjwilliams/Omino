using System.Collections.Generic;
using UnityEngine;

public enum BuildingType
{ BASE,FACTORY, MINE, NONE }


public class Polyomino
{
    public List<Tile> tiles = new List<Tile>();
    protected Dictionary<Tile, Coord> tileRelativeCoords;
    protected Transform holder;
    protected SpriteRenderer holderSr;
    protected SpriteRenderer iconSr;
    protected string holderName;
    public BuildingType buildingType { get; protected set; }

    public int index { get; protected set; }
    public int units { get; protected set; }
    protected int variations;
    protected int[,,] piece;
    public Player owner { get; protected set; }
    public Coord centerCoord;
    protected bool placed;
    private const float rotationInputRadius = 8f;
	private int touchID;
	private readonly Vector3 dragOffset = 2f * Vector3.up;
    private readonly Vector3 unselectedScale = 0.675f * Vector3.one;

    public bool connectedToBase;
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
        units = _units;
        owner = _player;

        occupyingStructures = new List<Blueprint>();
        connectedToBase = false;

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
                connectedToBase = true;
                piece = playerBase;
                break;
            default:
                break;
        }
    }

    public void SetPlaced(bool isPlaced)
    {
        placed = isPlaced;
    }

    public void SetVisible(bool isVisible)
    {
        if (!placed)
        {
            holder.gameObject.SetActive(isVisible);
            foreach (Tile tile in tiles)
            {
                tile.enabled = isVisible;
            }
            if (isVisible)
            {
                EnterUnselectedState();
            }
            else
            {
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
        //place the piece on the board where it's being hovered now
        placed = true;
        OnPlace();
        foreach(Tile tile in tiles)
        {
            Coord tileCoord = tile.coord;
            Services.MapManager.Map[tileCoord.x, tileCoord.y].SetOccupyingPiece(this);
        }

        List<Tile> emptyAdjacentTiles = new List<Tile>();

        foreach (Tile tile in tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (Services.MapManager.IsCoordContainedInMap(adjacentCoord))
                {
                    Tile adjTile = Services.MapManager.Map[adjacentCoord.x, adjacentCoord.y];
                    if(!adjTile.IsOccupied() && !emptyAdjacentTiles.Contains(adjTile))
                    {
                        emptyAdjacentTiles.Add(adjTile);
                    }
                }
            }
        }

        Services.MapManager.CheckForFortification(this, emptyAdjacentTiles);
    }

    public virtual bool IsPlacementLegal()
    {
        //determine if the pieces current location is a legal placement
        //CONDITIONS:
        //is contiguous with a structure connected to either the base or a fortification
        //doesn't overlap with any existing pieces or is a destructor\
        foreach(Tile tile in tiles)
        {
            if (!Services.MapManager.IsCoordContainedInMap(tile.coord)) return false;
            if (!Services.MapManager.ConnectedToBase(this, new List<Polyomino>(), 0)) return false;
            if (Services.MapManager.Map[tile.coord.x, tile.coord.y].IsOccupied()) return false;
        }
        return true;
    }

    public bool ShareTilesWith(Tile tile)
    {
        foreach(Tile myTiles in tiles)
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

    protected void TurnOffGlow()
    {
        foreach (Tile tile in tiles)
        {
            tile.SetGlowOutLine(0);
        }
    }

    protected void SetGlow(Color color)
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
        foreach(Tile tile in tiles)
        {
            Tile mapTile = Services.MapManager.Map[tile.coord.x, tile.coord.y];
            if (mapTile.HasResource())
            {
                owner.AddPieceToHand(mapTile.occupyingResource);
            }
        }
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
        GameObject.Destroy(holder.gameObject);
    }

    public void Reposition(Vector3 pos)
    {
        holder.position = pos;
        //change localposition of the piece container in player UI to value
    }

    public void SetBasePosition(IntVector2 pos)
    {
        centerCoord = new Coord(pos.x, pos.y);
        holder.position = Services.MapManager.Map[centerCoord.x, centerCoord.y].transform.position;
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
            Services.GameScene.transform).transform;
        holder.gameObject.name = holderName;
        holderSr = holder.gameObject.GetComponent<SpriteRenderer>();

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
        SetTileSprites();
        SetIconSprite();
        if (buildingType != BuildingType.BASE) SetVisible(isViewable);
    }

    protected virtual void SetIconSprite()
    {
        iconSr = holder.gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
        Vector3 centerPos = Vector3.zero;
        foreach (Tile tile in tiles)
        {
            centerPos += tile.transform.position;
        }
        centerPos /= tiles.Count;
        iconSr.transform.position = centerPos;
        if (buildingType == BuildingType.BASE) iconSr.sprite = Services.UIManager.baseIcon;
        else iconSr.enabled = false;
    }

    public void SetPieceState(bool playAvailable)
    {
        if (playAvailable) EnterUnselectedState();
        else HideFromInput();
    }

	protected void EnterUnselectedState(){
		Services.GameEventManager.Register<TouchDown>(OnTouchDown);
		Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
		touchID = -1;
		holder.localScale = unselectedScale;
	}

    protected void HideFromInput()
    {
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
    }

    bool IsPointContainedWithinHolderArea(Vector3 point)
    {
        Debug.Assert(holderSr != null);
        Vector3 extents = holderSr.bounds.extents;
        Vector3 centerPoint = holder.position;
        return point.x >= centerPoint.x - extents.x && point.x <= centerPoint.x + extents.x &&
            point.y >= centerPoint.y - extents.y && point.y <= centerPoint.y + extents.y;
    }

    protected void OnTouchDown(TouchDown e)
    {
        Vector3 touchWorldPos = 
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position);
		if (IsPointContainedWithinHolderArea(touchWorldPos) && touchID == -1 
            && owner.selectedPiece == null)
        {
            touchID = e.touch.fingerId;
            OnInputDown();
        }
    }

    protected void OnMouseDownEvent(MouseDown e)
    {
        Vector3 mouseWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
		if (IsPointContainedWithinHolderArea(mouseWorldPos) && owner.selectedPiece == null)
        {
            OnInputDown();
        }
    }

    protected void OnTouchUp(TouchUp e)
    {
        if (e.touch.fingerId == touchID)
        {
            touchID = -1;
            OnInputUp();
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
            owner.OnPieceSelected(this);
            OnInputDrag(holder.position);

            Services.GameEventManager.Register<TouchUp>(OnTouchUp);
            Services.GameEventManager.Register<TouchMove>(OnTouchMove);
            Services.GameEventManager.Register<TouchDown>(CheckTouchForRotateInput);
            Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

            Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
            Services.GameEventManager.Register<MouseMove>(OnMouseMoveEvent);
            Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);

        }
    }

    protected void OnMouseMoveEvent(MouseMove e)
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
		if (!placed) {
			Services.GameEventManager.Unregister<TouchMove> (OnTouchMove);
			Services.GameEventManager.Unregister<TouchUp> (OnTouchUp);
			Services.GameEventManager.Unregister<TouchDown> (CheckTouchForRotateInput);

			Services.GameEventManager.Unregister<MouseMove> (OnMouseMoveEvent);
			Services.GameEventManager.Unregister<MouseUp> (OnMouseUpEvent);
			if (IsPlacementLegal () && owner.placementAvailable && !owner.gameOver) {
				PlaceAtCurrentLocation ();
				owner.OnPiecePlaced (this);
			} else {
				owner.CancelSelectedPiece ();
				EnterUnselectedState ();
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
                holder.position.z));
        }

        if (owner.placementAvailable)
        {
            bool isLegal = IsPlacementLegal();
            if (isLegal)
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
            holder.position) < rotationInputRadius)
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
        SetIconSprite();
    }

    public virtual void PlaceAtLocation(Coord centerCoordLocation)
    {
        SetTileCoords(centerCoordLocation);
        Reposition(new Vector3(
            centerCoordLocation.x, 
            centerCoordLocation.y, 
            holder.position.z));
        PlaceAtCurrentLocation();
    }
}

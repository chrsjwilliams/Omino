using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    public bool isActive
    {
        get { return _isActive; }
    }


    public Coord coord { get; private set; }
    public BoxCollider boxCol { get; private set; }
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }
    public Polyomino pieceParent { get; private set; }
    private int touchID;

    public void Init(Coord coord_)
    {
        coord = coord_;
        transform.localPosition = new Vector3(coord.x, coord.y, 0);
        material = GetComponent<MeshRenderer>().material;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = Services.GameManager.MapColorScheme[0];
        }
        else
        {
            material.color = Services.GameManager.MapColorScheme[1];
        }
        boxCol = GetComponent<BoxCollider>();
        touchID = -1;
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<TouchUp>(OnTouchUp);
    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
    }

    public void Init(Coord coord_, Color color1, Color color2)
    {
        coord = coord_;
        transform.localPosition = new Vector3(coord.x, coord.y, 0);
        material = GetComponent<MeshRenderer>().material;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = color1;
        }
        else
        {
            material.color = color2;
        }
        boxCol = GetComponent<BoxCollider>();
    }

    public void Create()
    {
        material = GetComponent<MeshRenderer>().material;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = Services.GameManager.MapColorScheme[0];
        }
        else
        {
            material.color = Services.GameManager.MapColorScheme[1];
        }
        boxCol = GetComponent<BoxCollider>();
    }

    public void SetCoord(Coord newCoord)
    {
        coord = newCoord;
    }

    public void ActivateTile(Player player)
    {
        _isActive = true;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = player.ActiveTilePrimaryColors[0];
        }
        else
        {
            material.color = player.ActiveTilePrimaryColors[1];
        }
    }

    public void DeactivateTile()
    {
        _isActive = false;
        if ((coord.x + coord.y) % 2 == 0)
        {
            material.color = Services.GameManager.MapColorScheme[0];
        }
        else
        {
            material.color = Services.GameManager.MapColorScheme[1];
        }
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
    }

    public bool IsOccupied()
    {
        return occupyingPiece != null;
    }

    public Polyomino OccupiedBy()
    {
        if(IsOccupied())
        {
            return occupyingPiece;
        }
        else
        {
            return null;
        }
    }

    private void OnMouseDown()
    {
        OnInputDown();
    }

    private void OnMouseUp()
    {
        OnInputUp();
    }

    private void OnMouseDrag()
    {
        Vector3 inputPos = 
            Services.GameManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        OnInputDrag(inputPos);
    }

    private void Update()
    {
        if(touchID != -1)
        {
            Touch touch = Input.GetTouch(touchID);
            OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(touch.position));
        }
    }

    void OnTouchDown(TouchDown e)
    {
        if (boxCol.bounds.Contains(e.touch.position))
        {
            touchID = e.touch.fingerId;
            OnInputDown();
        }
    }

    void OnTouchUp(TouchUp e)
    {
        if(e.touch.fingerId == touchID)
        {
            touchID = -1;
            OnInputUp();
        }
    }

    void OnInputDown()
    {
        if (pieceParent != null) pieceParent.OnInputDown();
    }

    void OnInputUp()
    {
        if (pieceParent != null) pieceParent.OnInputUp();
    }

    void OnInputDrag(Vector3 inputPos)
    {
        if (pieceParent != null) pieceParent.OnInputDrag(inputPos);
    }
}

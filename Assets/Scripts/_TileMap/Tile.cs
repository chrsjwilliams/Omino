using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    public bool isActive
    {
        get { return _isActive; }
    }
    [SerializeField]
    private Sprite[] sprites;
    public Coord coord { get; private set; }
    public BoxCollider2D boxCol { get; private set; }
    private SpriteRenderer sr;
    private SpriteGlow glow;
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }
    public Polyomino pieceParent { get; private set; }
    public Blueprint occupyingStructure { get; private set; }
    private int touchID;

    public void Init(Coord coord_)
    {
        coord = coord_;
        boxCol = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        glow = GetComponent<SpriteGlow>();
        glow.OutlineWidth = 0;
        transform.position = new Vector3(coord.x, coord.y, 0);
        if ((coord.x + coord.y) % 2 == 0)
        {
            sr.color = Services.GameManager.MapColorScheme[0];
        }
        else
        {
            sr.color = Services.GameManager.MapColorScheme[1];
        }
        touchID = -1;
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<TouchUp>(OnTouchUp);
		Services.GameEventManager.Register<MouseDown> (OnMouseDownEvent);
		//if (pieceParent != null)
			//Debug.Log ("bounds from :" + boxCol.bounds.min + " to " + boxCol.bounds.max);
    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
        sr.sortingOrder += 5;
    }

	public void OnRemove(){
		Services.GameEventManager.Unregister<TouchDown> (OnTouchDown);
		Services.GameEventManager.Unregister<TouchUp> (OnTouchUp);
		Services.GameEventManager.Unregister<MouseDown> (OnMouseDownEvent);
	}

    public void SetCoord(Coord newCoord)
    {
        coord = newCoord;
    }

    public void SetColor(Color color)
    {
        sr.color = color;
    }

    public void SetGlowOutLine(int i)
    {
        glow.OutlineWidth = i;
    }

    public void SetGlowColor(Color color)
    {
        glow.GlowColor = color;
    }

    public void ActivateTile(Player player, BuildingType buildingType)
    {
        _isActive = true;
        if(buildingType == BuildingType.NONE) sr.color = player.ActiveTilePrimaryColors[0];
        else sr.color = player.ActiveTilePrimaryColors[1];
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
    }

    public void SetOccupyingStructure(Blueprint blueprint)
    {
        occupyingStructure = blueprint;
    }

    public bool IsOccupied()
    {
        return occupyingPiece != null;
    }

    public bool PartOfStructure()
    {
        return occupyingStructure != null;
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
            //OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(touch.position));
        }
    }

	void OnMouseDownEvent(MouseDown e){
		Vector3 touchWorldPos = Services.GameManager.MainCamera.ScreenToWorldPoint (e.mousePos);
		Vector3 projectedTouchPos = new Vector3 (touchWorldPos.x, touchWorldPos.y, transform.position.z);
		float dist = Vector3.Distance (projectedTouchPos, transform.position);
		if(dist < 0.5f) Debug.Log (dist);
	}

    void OnTouchDown(TouchDown e)
    {
		Vector3 touchWorldPos = Services.GameManager.MainCamera.ScreenToWorldPoint (e.touch.position);
		Vector3 projectedTouchPos = new Vector3 (touchWorldPos.x, touchWorldPos.y, transform.position.z);
		if (Vector3.Distance(projectedTouchPos, transform.position) < 0.5f && touchID == -1)
        {
            touchID = e.touch.fingerId;
			if (pieceParent != null)
				pieceParent.touchID = e.touch.fingerId;
            OnInputDown();
        }
    }

    void OnTouchUp(TouchUp e)
    {
        if(e.touch.fingerId == touchID)
        {
            touchID = -1;
			if (pieceParent != null)
				pieceParent.touchID = -1;
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


    public void SetSprite(int spriteIndex)
    {
        sr.sprite = sprites[spriteIndex];
    }

    public void SetAlpha(float alpha)
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    }

    public void IncrementSortingOrder(int inc)
	{
		sr.sortingOrder += inc;
	}
}

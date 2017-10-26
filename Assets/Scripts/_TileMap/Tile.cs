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
    public SuperDestructorResource occupyingResource { get; private set; }

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
    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
        sr.sortingOrder += 5;
    }

	public void OnRemove(){
	}

    public void SetCoord(Coord newCoord)
    {
        coord = newCoord;
    }

    public void SetColor(Color color)
    {
        sr.color = color;
    }

    public void ToggleAltColor(bool useAlt)
    {
        if (useAlt) sr.color = pieceParent.owner.ActiveTilePrimaryColors[1];
        else sr.color = pieceParent.owner.ActiveTilePrimaryColors[0];
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
        if (player == null) sr.color = Services.GameManager.SuperDestructorResourceColor;
        else if(buildingType == BuildingType.NONE) sr.color = player.ActiveTilePrimaryColors[0];
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

    public void SetOccupyingResource(SuperDestructorResource resource)
    {
        occupyingResource = resource;
    }

    public bool IsOccupied()
    {
        return occupyingPiece != null;
    }

    public bool PartOfStructure()
    {
        return occupyingStructure != null;
    }

    public bool HasResource()
    {
        return occupyingResource != null;
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

    private void Update()
    {

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

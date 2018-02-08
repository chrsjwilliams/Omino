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
    [SerializeField]
    private Sprite[] destructorSprites;
    public Coord coord { get; private set; }
    public BoxCollider2D boxCol { get; private set; }
    public SpriteRenderer sr { get; private set; }
    //private SpriteGlow glow;
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }
    public Polyomino pieceParent { get; private set; }
    public Blueprint occupyingBlueprint { get; private set; }
    public Structure occupyingStructure { get; private set; }
    public SuperDestructorResource occupyingResource { get; private set; }
    private Color prevColor;
    private Color targetColor_;
    private Color targetColor
    {
        get { return targetColor_; }
        set
        {
            prevColor = sr.color;
            targetColor_ = value;
            changingColor = true;
            colorChangeTimeElapsed = 0;
        }
    }
    private float colorChangeTimeElapsed;
    [SerializeField]
    private float colorChangeDuration;
    private bool changingColor;
    public SpriteRenderer maskSr { get; private set; }
    private SpriteRenderer bombOverlay;
    private SpriteRenderer moltenLines;
    private bool bombSettling;
    private float bombSettleTimeElapsed;
    private const float bombSettleDuration = 0.4f;
    private float currentBombAlpha;
    private float currentNormalAlpha;
    //public SpriteMask mask { get; private set; }

    public void Init(Coord coord_)
    {
        coord = coord_;
        boxCol = GetComponent<BoxCollider2D>();
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        sr = srs[0];
        bombOverlay = srs[2];
        moltenLines = srs[3];
        if (!(pieceParent != null && pieceParent is Destructor))
        {
            bombOverlay.enabled = false;
            moltenLines.enabled = false;
        }
        //glow = GetComponent<SpriteGlow>();
        //glow.OutlineWidth = 0;
        SpriteMask[] masks = GetComponentsInChildren<SpriteMask>();
        maskSr = masks[0].gameObject.GetComponent<SpriteRenderer>();
        //mask = masks[1];
        //mask.enabled = false;
        
        transform.position = new Vector3(coord.x, coord.y, 0);

        sr.color = Services.GameManager.MapColorScheme[0];

    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
        IncrementSortingOrder(5);
    }

	public void OnRemove(){
        //Destroy(this);
	}

    public void SetCoord(Coord newCoord)
    {
        coord = newCoord;
    }

    public void SetColor(Color color)
    {
        sr.color = color;
        bombOverlay.color = color;
    }

    public void ShiftColor(Color color)
    {
        targetColor = color;
    }

    public void SetGlowOutLine(int i)
    {
        //glow.OutlineWidth = i;
    }

    public void SetGlowColor(Color color)
    {
        //glow.GlowColor = color;
    }

    public void SetMaskSrAlpha(float alpha)
    {
        maskSr.color = new Color(maskSr.color.r, maskSr.color.g, maskSr.color.b, alpha);
    }

    public void SetMaskColor(Color color)
    {
        maskSr.color = color;
    }

    public void ActivateTile(Player player, BuildingType buildingType)
    {
        _isActive = true;
        if (player == null) sr.color = Services.GameManager.NeutralColor;
        //else if(buildingType == BuildingType.NONE)
         else sr.color = player.ColorScheme[0];
        //else sr.color = player.ColorScheme[1];
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
    }

    public void SetOccupyingResource(SuperDestructorResource resource)
    {
        occupyingResource = resource;
    }

    public void SetOccupyingBlueprint(Blueprint blueprint)
    {
        occupyingBlueprint = blueprint;
    }

    public void SetOccupyingStructure(Structure structure)
    {
        occupyingStructure = structure;
    }

    public bool IsOccupied()
    {
        return occupyingPiece != null;
    }

    public bool PartOfExistingBlueprint()
    {
        return occupyingBlueprint != null;
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
        if (changingColor) LerpToTargetColor();
        if (bombSettling) SettleToNormalSprite();
    }

    void LerpToTargetColor()
    {
        colorChangeTimeElapsed += Time.deltaTime;
        sr.color = Color.Lerp(prevColor, targetColor, 
            colorChangeTimeElapsed / colorChangeDuration);
        bombOverlay.color = new Color(sr.color.r, sr.color.g, sr.color.b, bombOverlay.color.a);
        if(colorChangeTimeElapsed >= colorChangeDuration)
        {
            changingColor = false;
        }
    }

    void SettleToNormalSprite()
    {
        bombSettleTimeElapsed += Time.deltaTime;
        sr.color = Color.Lerp(new Color(sr.color.r, sr.color.g, sr.color.b, 0),
            new Color(sr.color.r, sr.color.g, sr.color.b, 1),
            EasingEquations.Easing.QuadEaseOut(bombSettleTimeElapsed / bombSettleDuration));
        bombOverlay.color = Color.Lerp(new Color(sr.color.r, sr.color.g, sr.color.b, 1),
            new Color(sr.color.r, sr.color.g, sr.color.b, 0),
            EasingEquations.Easing.QuadEaseIn(bombSettleTimeElapsed / bombSettleDuration));
        moltenLines.color = Color.Lerp(new Color(sr.color.r, sr.color.g, sr.color.b, 1),
            new Color(sr.color.r, sr.color.g, sr.color.b, 0),
            EasingEquations.Easing.QuadEaseIn(bombSettleTimeElapsed / bombSettleDuration));
        if (bombSettleTimeElapsed >= bombSettleDuration) bombSettling = false;
    }


    public void SetSprite(int spriteIndex)
    {
        bombOverlay.sprite = destructorSprites[spriteIndex];
        sr.sprite = sprites[spriteIndex];
    }

    public void SetAlpha(float alpha)
    {
        targetColor = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    }

    public void IncrementSortingOrder(int inc)
	{
		sr.sortingOrder += inc;
        maskSr.sortingOrder += inc;
        bombOverlay.sortingOrder += inc;
        moltenLines.sortingOrder += inc;
	}

    public void SetSortingOrder(int sortingOrder)
    {
        int maskDiff = sr.sortingOrder - maskSr.sortingOrder;
        int bombDiff = sr.sortingOrder - bombOverlay.sortingOrder;
        int moltenDiff = sr.sortingOrder - moltenLines.sortingOrder;
        sr.sortingOrder = sortingOrder;
        maskSr.sortingOrder = sortingOrder - maskDiff;
        bombOverlay.sortingOrder = sortingOrder - bombDiff;
        moltenLines.sortingOrder = sortingOrder - moltenDiff;
    }

    public void PrintCoord()
    {
        Debug.Log("X: " + coord.x + ", Y: " + coord.y);
    }

    public void StartSettlingToNormalPiece()
    {
        bombSettling = true;
        bombSettleTimeElapsed = 0;
    }
}

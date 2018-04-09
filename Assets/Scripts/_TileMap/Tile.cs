using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Tile : MonoBehaviour, IVertex
{
    [SerializeField]
    private Sprite[] sprites;
    [SerializeField]
    private Sprite[] destructorSprites;
    [SerializeField]
    private Sprite[] shieldSprites;
    [SerializeField]
    private Sprite disconnectedSprite;
    public Coord coord { get; private set; }
    public Coord relativeCoord;
    public BoxCollider2D boxCol { get; private set; }
    public SpriteRenderer mainSr;
    [SerializeField]
    private SpriteRenderer fillOverlay;
    //private SpriteGlow glow;
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }
    public Polyomino pieceParent { get; private set; }
    public Blueprint occupyingBlueprint { get; private set; }
    public Structure occupyingStructure { get; private set; }
    private Color prevColor;
    private Color targetColor_;
    private Color targetColor
    {
        get { return targetColor_; }
        set
        {
            prevColor = mainSr.color;
            targetColor_ = value;
            baseColor = value;
            changingColor = true;
            colorChangeTimeElapsed = 0;
        }
    }
    private Color baseColor;
    private float colorChangeTimeElapsed;
    [SerializeField]
    private float colorChangeDuration;
    private bool changingColor;
    public SpriteRenderer highlightSr;
    //private SpriteRenderer bombOverlay;
    //private SpriteRenderer moltenLines;
    [SerializeField]
    private SpriteRenderer shieldSr;
    [SerializeField]
    private SpriteRenderer legalitySr;
    private bool bombSettling;
    private float bombSettleTimeElapsed;
    private const float bombSettleDuration = 0.4f;
    private float currentBombAlpha;
    private float currentNormalAlpha;
    private SortingGroup sortingGroup;
    private float redPulseTimer;
    private const float redPulsePeriod = 0.6f;
    private static Color redPulseColor = new Color(0.5f, 0, 0.5f);
    private bool toRed;
    //private Image uiTile;
    //private OverlayIcon illegalLocationIcon;
    //public SpriteMask mask { get; private set; }

    public void Init(Coord coord_)
    {
        coord = coord_;
        boxCol = GetComponent<BoxCollider2D>();
        sortingGroup = GetComponentInChildren<SortingGroup>();
        shieldSr.enabled = false;
        highlightSr.enabled = false;
        legalitySr.enabled = false;
        transform.position = new Vector3(coord.x, coord.y, 0);

        mainSr.color = Services.GameManager.MapColorScheme[0];
        baseColor = mainSr.color;
        toRed = true;
        if (pieceParent == null) IncrementSortingOrder(-5000);
    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
        relativeCoord = coord_;
    }

    public void ToggleIllegalLocationIcon(bool status)
    {
        legalitySr.enabled = status;
    }

    public void OnRemove(){

    }

    public void OnPlace()
    {
        ToggleIllegalLocationIcon(false);
        SetAlpha(0.1f);
        SetFilledUIFillAmount(0);
    }

    public void SetCoord(Coord newCoord)
    {
        coord = newCoord;
    }

    public void SetColor(Color color)
    {
        SetSrAndUIColor(color);
        baseColor = color;
        targetColor = color;
    }
    
    private void SetSrAndUIColor(Color color)
    {
        mainSr.color = color;
        fillOverlay.color = new Color(color.r, color.g, color.b, 1);
    }

    public Color GetColor() { return mainSr.color; }

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

    public void SetHighlightAlpha(float alpha)
    {
        highlightSr.color = new Color(highlightSr.color.r, highlightSr.color.g, highlightSr.color.b, alpha);
    }

    public void SetHighlightStatus(bool status)
    {
        highlightSr.enabled = status;
    }

    public void SetHighlightColor(Color color)
    {
        highlightSr.color = color;
    }

    public void SetBaseTileColor(Player player, BuildingType buildingType)
    {
        if (player == null) SetSrAndUIColor(Services.GameManager.NeutralColor);
        else SetSrAndUIColor(player.ColorScheme[0]);
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
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
    }

    void LerpToTargetColor()
    {
        colorChangeTimeElapsed += Time.deltaTime;
        SetSrAndUIColor(Color.Lerp(prevColor, targetColor,
            colorChangeTimeElapsed / colorChangeDuration));
        baseColor = mainSr.color;
        if(colorChangeTimeElapsed >= colorChangeDuration)
        {
            changingColor = false;
        }
    }

    public void SetSprite(int spriteIndex)
    {
        mainSr.sprite = sprites[spriteIndex];
        shieldSr.sprite = shieldSprites[spriteIndex];
        fillOverlay.sprite = mainSr.sprite;
    }

    public void ShiftAlpha(float alpha)
    {
        targetColor = new Color(mainSr.color.r, mainSr.color.g, mainSr.color.b, alpha);
    }

    public void SetAlpha(float alpha)
    {
        mainSr.color = new Color(mainSr.color.r, mainSr.color.g, mainSr.color.b, alpha);
        targetColor = mainSr.color;
    }

    public void IncrementSortingOrder(int inc)
	{
		highlightSr.sortingOrder += inc;
        sortingGroup.sortingOrder += inc;
        shieldSr.sortingOrder += inc;
        legalitySr.sortingOrder += inc;
	}

    public void SetSortingOrder(int sortingOrder)
    {
        sortingGroup.sortingOrder = sortingOrder;
    }

    public void SortOnSelection(bool selected)
    {
        if (selected) sortingGroup.sortingLayerName = "SelectedPiece";
        else sortingGroup.sortingLayerName = "Default";
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

    public void SetShieldStatus(bool status)
    {
        shieldSr.enabled = status;
    }

    public void SetShieldAlpha(float alpha)
    {
        shieldSr.color = new Color(shieldSr.color.r, shieldSr.color.g, shieldSr.color.b,
            alpha);
    }

    public void SetFilledUIFillAmount(float fillProportion)
    {
        fillOverlay.material.SetFloat("_Cutoff", 1 - fillProportion);
    }

    public void ToggleConnectedness(bool connected)
    {
        if (connected) mainSr.sprite = sprites[0];
        else mainSr.sprite = disconnectedSprite;
    }
}

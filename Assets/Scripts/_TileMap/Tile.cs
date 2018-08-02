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
    public SpriteRenderer topSr;
    [SerializeField]
    private SpriteRenderer fillOverlay;
    [SerializeField]
    private SpriteRenderer fillOverlayTop;
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }
    public Polyomino pieceParent { get; private set; }
    public Blueprint occupyingBlueprint { get; private set; }
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
    [SerializeField]
    private SpriteRenderer shieldSr;
    public bool shielded { get; private set; }
    [SerializeField]
    private SpriteRenderer legalitySr;
    [SerializeField]
    private SpriteRenderer bpAssistHighlightSr;
    private readonly Color bpAssistColor = new Color(1, 1, 1);
    private bool bombSettling;
    private float bombSettleTimeElapsed;
    private const float bombSettleDuration = 0.4f;
    private float currentBombAlpha;
    private float currentNormalAlpha;
    private SortingGroup sortingGroup;
    private float scaleUpTimeElapsed;
    private float scaleUpDelayTime;
    private const float scaleUpDuration = 0.3f;
    public const float scaleUpStaggerTime = 0.1f;
    private bool scaling;
    private bool rippleEmitted;

    private float pathHighlightTime;
    private bool highlightingPath;
    private const float pathHighlightUpDuration = 0.1f;
    private const float pathHighlightDownDuration = 0.6f;
    private float pathHighlightDelay;

    public void Init(Coord coord_)
    {
        coord = coord_;
        boxCol = GetComponent<BoxCollider2D>();
        sortingGroup = GetComponentInChildren<SortingGroup>();
        shieldSr.enabled = false;
        highlightSr.enabled = false;
        legalitySr.enabled = false;
        topSr.enabled = false;

        transform.position = new Vector3(coord.x, coord.y, 0);

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.HyperSOLO:
            case TitleSceneScript.GameMode.HyperVS:
                mainSr.color = Services.GameManager.MapColorScheme[1];
                break;
            default:
                mainSr.color = Services.GameManager.MapColorScheme[0];
                break;
        }
        
        baseColor = mainSr.color;
        bpAssistHighlightSr.gameObject.SetActive(false);
        if (pieceParent == null) IncrementSortingOrder(-5000);
    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
        relativeCoord = coord_;
        topSr.enabled = true;
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
        //SetFilledUIFillAmount(0);
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
        topSr.color = new Color(1, 1, 1, color.a);
        //fillOverlay.color = new Color(color.r, color.g, color.b, 1);
    }

    public Color GetColor() { return mainSr.color; }

    public void ShiftColor(Color color)
    {
        targetColor = color;
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
        if (scaling) ScaleUp();
        if (pathHighlightDelay > 0) TickDownHighlightDelay();
        if (highlightingPath) PathHighlight();
    }

    public void StartPathHighlight(float delay)
    {
        pathHighlightDelay = delay;
        pathHighlightTime = 0;
    }

    private void TickDownHighlightDelay()
    {
        pathHighlightDelay -= Time.deltaTime;
        if (pathHighlightDelay <= 0) highlightingPath = true;
    }

    private void PathHighlight()
    {
        if (!pieceParent.connected)
        {
            highlightingPath = false;
            SetBpAssistAlpha(0);
            return;
        }
        pathHighlightTime += Time.deltaTime;
        float alpha;
        if (pathHighlightTime < pathHighlightUpDuration)
        {
            alpha = Mathf.Lerp(0, 1,
                EasingEquations.Easing.QuadEaseOut(
                   pathHighlightTime /pathHighlightUpDuration));
        }
        else
        {
            alpha = Mathf.Lerp(1, 0,
                EasingEquations.Easing.QuadEaseOut(
                    (pathHighlightTime- pathHighlightUpDuration) 
                    / pathHighlightDownDuration));
        }
        if (pathHighlightTime >= pathHighlightUpDuration + pathHighlightDownDuration)
        {
            alpha = 0;
            highlightingPath = false;
        }
        SetBpAssistAlpha(alpha);
    }

    private void ScaleUp()
    {
        scaleUpTimeElapsed += Time.deltaTime;

        if (scaleUpTimeElapsed > scaleUpDelayTime)
        {
            if (!rippleEmitted)
            {
                GameObject ripple = Instantiate(Services.Prefabs.RippleEffect, Services.GameScene.transform);
                ripple.transform.position = transform.position;
                ParticleSystem ripplePs = ripple.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = ripplePs.main;
                main.startColor = pieceParent.owner.ColorScheme[0];
                rippleEmitted = true;
            }
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
                EasingEquations.Easing.QuadEaseOut(
                    Mathf.Min(1,
                    (scaleUpTimeElapsed - scaleUpDelayTime) / scaleUpDuration)));
        }
        if(scaleUpTimeElapsed > scaleUpDelayTime + scaleUpDuration)
        {
            scaling = false;
        }
    }

    public void StartScaling(float delay)
    {
        scaling = true;
        scaleUpDelayTime = delay;
        scaleUpTimeElapsed = 0;
        transform.localScale = Vector3.zero;
    }

    void LerpToTargetColor()
    {
        colorChangeTimeElapsed += Time.deltaTime;
        SetSrAndUIColor(Color.Lerp(prevColor, targetColor,
            colorChangeTimeElapsed / colorChangeDuration));
        baseColor = mainSr.color;
        topSr.color = new Color(1, 1, 1, mainSr.color.a);
        if(colorChangeTimeElapsed >= colorChangeDuration)
        {
            changingColor = false;
        }
    }

    public void SetSprite(int spriteIndex)
    {
        mainSr.sprite = sprites[spriteIndex];
        shieldSr.sprite = shieldSprites[spriteIndex];
        //fillOverlay.sprite = mainSr.sprite;
        //fillOverlayTop.sprite = topSr.sprite;
    }

    public void ShiftAlpha(float alpha)
    {
        targetColor = new Color(mainSr.color.r, mainSr.color.g, mainSr.color.b, alpha);
    }

    public void SetAlpha(float alpha)
    {
        mainSr.color = new Color(mainSr.color.r, mainSr.color.g, mainSr.color.b, alpha);
        targetColor = mainSr.color;
        topSr.color = new Color(1, 1, 1, alpha);
        //fillOverlay.color = mainSr.color;
        //fillOverlayTop.color = new Color(1, 1, 1, alpha);
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

    public void SetShieldStatus(bool status)
    {
        shielded = status;
        shieldSr.enabled = status;
    }

    public void SetShieldAlpha(float alpha)
    {
        shieldSr.color = new Color(shieldSr.color.r, shieldSr.color.g, shieldSr.color.b,
            alpha);
    }

    public void SetFilledUIFillAmount(float fillProportion)
    {
        //fillOverlay.material.SetFloat("_Cutoff", 1 - fillProportion);
        //fillOverlayTop.material.SetFloat("_Cutoff", 1 - fillProportion);
    }

    public void ToggleConnectedness(bool connected)
    {
        if (connected) mainSr.sprite = sprites[0];
        else mainSr.sprite = disconnectedSprite;
    }

    public void SetBpAssistAlpha(float alpha)
    {
        if(alpha == 0)
        {
            bpAssistHighlightSr.gameObject.SetActive(false);
        }
        else if (!bpAssistHighlightSr.gameObject.activeInHierarchy)
        {
            bpAssistHighlightSr.gameObject.SetActive(true);
        }
        bpAssistHighlightSr.color = new Color(bpAssistColor.r, bpAssistColor.g, bpAssistColor.b, alpha);
    }
}

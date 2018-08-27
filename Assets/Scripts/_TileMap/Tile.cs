using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Tile : MonoBehaviour, IVertex
{
    #region TileSkin Items
    [SerializeField]
    private Sprite[] handSprites;
    public Sprite[] HandSprites
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].handSprites;
            }
            else
                return handSprites;
        }
    }
    [SerializeField]
    private Sprite[] placedSprites;
    public Sprite[] PlacedSprites
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].placedSprites;
            }
            else
                return placedSprites;
        }
    }

    [SerializeField]
    private Sprite[] placedSecondarySprites;
    public Sprite[] PlacedSecondarySprites
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].placedSecondarySprites;
            }
            else
                return placedSecondarySprites;
        }
    }

    [SerializeField]
    private Sprite[] shieldSprites;
    public Sprite[] ShieldSprites
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].shieldSprites;
            }
            else
                return shieldSprites;
        }
    }

    [SerializeField]
    private Sprite[] disconnectedSprites;
    public Sprite[] DisconnectedSprites
    {
        get
        {
            if (Services.GameManager.currentTileSkins[0] != null)
            {
                return Services.GameManager.currentTileSkins[0].disconnectedSprites;
            }
            else
                return disconnectedSprites;
        }
    }
    #endregion

    public bool impassable { get; private set; }
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

    public Polyomino pieceParent { get; private set; }
    
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
    [SerializeField]
    private SpriteRenderer crackSr;
    [SerializeField]
    private SpriteRenderer[] gridEdges;
    //private readonly Color bpAssistColor = new Color(1, 1, 1);
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

    private float entranceTime;
    private float entranceDelayTime;
    private const float entranceTotalDuration = 0.4f;
    private const float blackEntranceTotalDuration = 0.8f;
    public const float entranceStaggerTime = 0.1f;
    private const float entranceOuterDelay = 0f;
    private const float entranceInnerDelay = 0.075f;
    private bool entering;
    private float blackEntranceTime;
    private bool blackEntering;



    public void Init(Coord coord_, bool impassable_ = false)
    {
        coord = coord_;
        boxCol = GetComponent<BoxCollider2D>();
        impassable = impassable_;
        sortingGroup = GetComponentInChildren<SortingGroup>();
        shieldSr.enabled = false;
        highlightSr.enabled = false;
        legalitySr.enabled = false;
        topSr.enabled = false;
        crackSr.enabled = false;

        transform.position = new Vector3(coord.x, coord.y, 0);
        
        baseColor = mainSr.color;

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.HyperSOLO:
            case TitleSceneScript.GameMode.HyperVS:
                mainSr.color = Services.GameManager.MapColorScheme[1];
                break;
            default:
                mainSr.color = impassable ? Color.black: Services.GameManager.MapColorScheme[0];
                break;
        }

        if (impassable) Debug.Log(coord.ToString());
        bpAssistHighlightSr.gameObject.SetActive(false);
        //for (int i = 0; i < gridEdges.Length; i++)
        //{
        //    SpriteRenderer edgeSr = gridEdges[i];

        //    edgeSr.enabled = false;
        //}
        //if (pieceParent == null)
        //{
        //    IncrementSortingOrder(-5000);
        //    if (coord.x == 0) gridEdges[1].enabled = true;
        //    if (coord.x == Services.MapManager.MapWidth - 1) gridEdges[2].enabled = true;
        //    if (coord.y == 0) gridEdges[0].enabled = true;
        //    if (coord.y == Services.MapManager.MapHeight - 1) gridEdges[3].enabled = true;
        //}
    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
        relativeCoord = coord_;
        topSr.enabled = false;
    }

    public void ToggleIllegalLocationIcon(bool status)
    {
        legalitySr.enabled = status;
    }

    public void OnRemove()
    {
        
    }

    public void OnPlace()
    {
        ToggleIllegalLocationIcon(false);
        //SetFilledUIFillAmount(0);
        topSr.enabled = true;
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

   

    private void Update()
    {
        //if (changingColor) LerpToTargetColor();
        if (scaling) ScaleUp();
        if (pathHighlightDelay > 0) TickDownHighlightDelay();
        if (highlightingPath) PathHighlight();
        if (entranceDelayTime > 0) TickDownEntranceDelay();
        if (entering) EntranceAnimation();
        if (blackEntering) BlackEntrance();
    }

    private void TickDownEntranceDelay()
    {
        entranceDelayTime -= Time.deltaTime;
        if (entranceDelayTime <= 0) entering = true;
    }

    private void BlackEntrance()
    {
        blackEntranceTime += Time.deltaTime;
        float rawDuration = Mathf.Min(1, blackEntranceTime / blackEntranceTotalDuration);
        MapTile mapTile = Services.MapManager.Map[coord.x, coord.y];
        mapTile.FadeToFull(Easing.SineEaseOut(rawDuration));
        if (blackEntranceTime >= blackEntranceTotalDuration) blackEntering = false;
    }

    public void StartEntrance(float delay)
    {
        entranceDelayTime = delay;
        entranceTime = 0;
        Color zeroAlpha = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
        mainSr.color = zeroAlpha;
        topSr.color = zeroAlpha;
        Services.MapManager.Map[coord.x, coord.y].SetMapSprite(true);
        blackEntranceTime = 0;
        blackEntering = true;
    }


    private void EntranceAnimation()
    {
        entranceTime += Time.deltaTime;
        float rawDuration = Mathf.Min(1, entranceTime / entranceTotalDuration);
        float rawOuterDuration = Mathf.Min(1, (entranceTime-entranceOuterDelay) / 
            (entranceTotalDuration-entranceOuterDelay));
        float rawInnerDuration = Mathf.Min(1, (entranceTime - entranceInnerDelay - entranceOuterDelay) /
            (entranceTotalDuration - entranceInnerDelay-entranceOuterDelay));
        //MapTile mapTile = Services.MapManager.Map[coord.x, coord.y];
        //mapTile.SetMapSprite();
        //mapTile.FadeToFull(Easing.ExpoEaseOut(rawDuration));
        Color zeroAlpha = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
        if (entranceTime >= entranceOuterDelay)
        {
            mainSr.color = Color.Lerp(zeroAlpha, baseColor,
                Easing.SineEaseOut(rawOuterDuration));
        }
        if(entranceTime >= entranceInnerDelay)
        {
            topSr.color = Color.Lerp(zeroAlpha, baseColor,
                Easing.SineEaseOut(rawInnerDuration));
        }
        if(entranceTime >= entranceTotalDuration)
        {
            entering = false;
        }
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

    public void SetSprite()
    {
        int ownerIndex = pieceParent.owner.playerNum - 1;
        mainSr.sprite = pieceParent.placed ? 
            PlacedSprites[ownerIndex] : HandSprites[ownerIndex];
        shieldSr.sprite = ShieldSprites[ownerIndex];
        topSr.sprite = PlacedSecondarySprites[ownerIndex];
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
        if (connected)
        {
            //mainSr.sprite = Sprites[0];
            crackSr.enabled = false;
        }
        else
        {
            //mainSr.sprite = DisconnectedSprite;
            crackSr.enabled = true;
        }
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
        bpAssistHighlightSr.color = new Color(bpAssistHighlightSr.color.r, bpAssistHighlightSr.color.g, bpAssistHighlightSr.color.b, alpha);
    }

    public void SetBpAssistColor(Color color)
    {
        bpAssistHighlightSr.color = color;
    }
}

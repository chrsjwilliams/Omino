using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceHolder : MonoBehaviour {

    public Polyomino piece;
    public SpriteRenderer holderSelectionArea;
    public SpriteRenderer spriteBottom;
    public SpriteRenderer dropShadow;
    public SpriteRenderer icon;
    public SpriteRenderer legalityOverlay;
    public SpriteRenderer energyLevelBack;
    public SpriteRenderer energyLevelFront;
    public SpriteRenderer attackLevelBack;
    public SpriteRenderer attackLevelFront;
    public SpriteRenderer glower;
    public Color legalGlowColor;
    public Color notLegalGlowColor;
    private Color prevColor;
    private Color targetColor_;
    private Color targetColor
    {
        get { return targetColor_; }
        set
        {
            prevColor = spriteBottom.color;
            targetColor_ = value;
            changingColor = true;
            colorChangeTimeElapsed = 0;
        }
    }
    private float colorChangeTimeElapsed;
    private bool changingColor;
    public float colorChangeDuration;
    private const float energyCutoffMax = 0.8f;
    private const float energyCutoffMin = 0.18f;
    private const float energyDisplayOffset = 1.5f;

    private float pathHighlightTime;
    private bool highlightingPath;
    private const float pathHighlightUpDuration = 0.1f;
    private const float pathHighlightDownDuration = 0.6f;
    private float pathHighlightDelay;
    private Color baseColor;

    private bool beingClaimed;
    private float claimAnimDuration;
    private float claimIconChangeDuration;
    private float claimTimeElapsed;
    private Color iconPrevColor;

    public void Init(Polyomino piece)
    {
        this.piece = piece;
        legalityOverlay.enabled = false;
        icon.enabled = false;
        SetEnergyDisplayStatus(false);
        SetAttackDisplayStatus(false);
    }

    public void Init(Blueprint piece)
    {
        Polyomino polyomino = piece;
        Init(polyomino);
        spriteBottom.sortingOrder = 0;
    }


    // Use this for initialization
    void Start () {
        claimAnimDuration = Services.Clock.QuarterLength();
        claimIconChangeDuration = Services.Clock.QuarterLength();
	}
	
	// Update is called once per frame
	void Update () {
        if (changingColor) LerpToTargetColor(spriteBottom, prevColor, targetColor);
        if (pathHighlightDelay > 0) TickDownHighlightDelay();
        if (highlightingPath) PathHighlight();
        if (beingClaimed) AnimateClaim();
    }

    private void AnimateClaim()
    {
        claimTimeElapsed += Time.deltaTime;
        if (claimTimeElapsed <= claimAnimDuration)
        {
            float progress = Easing.SineEaseIn(Mathf.Min(1, claimTimeElapsed / claimAnimDuration));
            spriteBottom.material.SetFloat("_Cutoff", 1 - progress);
        }
        else
        {
            spriteBottom.material.SetFloat("_Cutoff", 0);
            icon.color = Color.Lerp(iconPrevColor, piece.owner.ColorScheme[0],
                Easing.QuadEaseOut(Mathf.Min(1, 
                (claimTimeElapsed - claimAnimDuration) /
                claimIconChangeDuration)));
        }
        if (claimTimeElapsed >= claimAnimDuration + claimIconChangeDuration)
            beingClaimed = false;
    }

    public void ShiftColor(Color color)
    {
        targetColor = color;
    }

    public void SetBaseColor(Color color)
    {
        spriteBottom.color = color;
        icon.color = color;
        baseColor = color;
    }

    private void LerpToTargetColor(SpriteRenderer sr, Color start, 
        Color target)
    {
        colorChangeTimeElapsed += Time.deltaTime;
        sr.color =  Color.Lerp(start, target,
            colorChangeTimeElapsed / colorChangeDuration);
        if (colorChangeTimeElapsed >= colorChangeDuration)
        {
            changingColor = false;
        }
    }

    public void SetEnergyDisplayStatus(bool status)
    {
        energyLevelBack.enabled = status;
        energyLevelFront.enabled = status;
        if (status)
        {
            UpdateEnergyDisplayPos();
            if (piece.owner.playerNum == 2)
                energyLevelBack.transform.localRotation = Quaternion.Euler(0, 0, 180);
        }
    }

    public void SetAttackDisplayStatus(bool status)
    {
        attackLevelBack.enabled = status;
        attackLevelFront.enabled = status;
        if (status)
        {
            UpdateAttackDisplayPos();
            if (piece.owner.playerNum == 2)
                attackLevelBack.transform.localRotation = Quaternion.Euler(0, 0, 180);
        }
    }

    public void SetEnergyLevel(float prop)
    {
        SetEnergyFillAmount(Mathf.Min(1, prop));
        if (prop >= 1)
        {
            energyLevelFront.color = legalGlowColor;
            energyLevelBack.color = legalGlowColor;
        }
        else
        {
            energyLevelFront.color = notLegalGlowColor;
            energyLevelBack.color = notLegalGlowColor;
        }
    }

    public void SetAttackLevel(float prop)
    {
        SetAttackFillAmount(Mathf.Min(1, prop));
        if (prop >= 1)
        {
            attackLevelFront.color = legalGlowColor;
            attackLevelBack.color = legalGlowColor;
        }
        else
        {
            attackLevelFront.color = notLegalGlowColor;
            attackLevelBack.color = notLegalGlowColor;
        }
    }

    private void SetEnergyFillAmount(float amount)
    {
        float cutoff = ((1 - amount) * (energyCutoffMax-energyCutoffMin)) + energyCutoffMin;
        if (amount >= 1) cutoff = 0;
        energyLevelFront.material.SetFloat("_Cutoff", cutoff);
    }

    private void SetAttackFillAmount(float amount)
    {
        float cutoff = ((1 - amount) * (energyCutoffMax - energyCutoffMin)) + energyCutoffMin;
        if (amount >= 1) cutoff = 0;
        attackLevelFront.material.SetFloat("_Cutoff", cutoff);
    }

    public void UpdateEnergyDisplayPos()
    {
        Tile leftmostTile = piece.tiles[0];
        int modifier = piece.owner.playerNum == 1 ? -1 : 1;
        foreach (Tile tile in piece.tiles)
        {
            if (modifier == -1 && tile.transform.localPosition.x < leftmostTile.transform.localPosition.x ||
                modifier == 1 && tile.transform.localPosition.x > leftmostTile.transform.localPosition.x)
            {
                leftmostTile = tile;
            }
        }
        Vector3 position = (leftmostTile.transform.localPosition.x + (energyDisplayOffset * modifier))
            * Vector3.right;
        energyLevelBack.transform.localPosition = position;
    }

    public void UpdateAttackDisplayPos()
    {
        Tile rightmostTile = piece.tiles[0];
        int modifier = piece.owner.playerNum == 1 ? 1 : -1;
        foreach (Tile tile in piece.tiles)
        {
            if (modifier == -1 && tile.transform.localPosition.x < rightmostTile.transform.localPosition.x ||
                modifier == 1 && tile.transform.localPosition.x > rightmostTile.transform.localPosition.x)
            {
                rightmostTile = tile;
            }
        }
        Vector3 position = (rightmostTile.transform.localPosition.x + (energyDisplayOffset * modifier))
            * Vector3.right;
        attackLevelBack.transform.localPosition = position;
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
        if ((!piece.connected && !(piece is TechBuilding)) ||
            ((piece is TechBuilding) && piece.owner == null))
        {
            highlightingPath = false;
            if (piece.owner == null)
            {
                TechBuilding techBuilding = piece as TechBuilding;
                techBuilding.SetNeutralVisualStatus();
            }
            return;
        }
        pathHighlightTime += Time.deltaTime;
        Color color;
        if (pathHighlightTime < pathHighlightUpDuration)
        {
            color = Color.Lerp(baseColor, Color.white,
                EasingEquations.Easing.QuadEaseOut(
                   pathHighlightTime / pathHighlightUpDuration));
        }
        else
        {
            color = Color.Lerp(Color.white, baseColor,
                EasingEquations.Easing.QuadEaseOut(
                    (pathHighlightTime - pathHighlightUpDuration)
                    / pathHighlightDownDuration));
        }
        if (pathHighlightTime >= pathHighlightUpDuration + pathHighlightDownDuration)
        {
            color = baseColor;
            highlightingPath = false;
        }
        spriteBottom.color = color;
    }

    public void SetTechStatus(Player owner, bool initialBase = false)
    {
        if (owner == null)
        {
            spriteBottom.enabled = false;
            icon.color = Services.GameManager.NeutralColor;
        }
        else
        {
            if (!initialBase)
            {
                Services.Clock.SyncFunction(StartClaimAnimation, BeatManagement.Clock.BeatValue.Eighth);
            }
            else
            {
                beingClaimed = false;
                spriteBottom.material.SetFloat("_Cutoff", 0);
                spriteBottom.enabled = true;
            }
            //SetBaseColor(owner.ColorScheme[0]);
        }
        iconPrevColor = icon.color;
    }

    private void StartClaimAnimation()
    {
        spriteBottom.material.SetFloat("_Cutoff", 1);
        spriteBottom.enabled = true;
        beingClaimed = true;
        claimTimeElapsed = 0;
    }
}

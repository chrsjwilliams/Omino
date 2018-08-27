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
    private const float claimAnimDuration = 0.8f;
    private float claimTimeElapsed;


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (changingColor) LerpToTargetColor();
        if (pathHighlightDelay > 0) TickDownHighlightDelay();
        if (highlightingPath) PathHighlight();
        if (beingClaimed) AnimateClaim();
    }

    private void AnimateClaim()
    {
        claimTimeElapsed += Time.deltaTime;
        float progress = Easing.SineEaseIn(Mathf.Min(1,claimTimeElapsed / claimAnimDuration));
        spriteBottom.material.SetFloat("_Cutoff", 1 - progress);
        if (claimTimeElapsed >= claimAnimDuration) beingClaimed = false;
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

    private void LerpToTargetColor()
    {
        colorChangeTimeElapsed += Time.deltaTime;
        spriteBottom.color =  Color.Lerp(prevColor, targetColor,
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
            energyLevelFront.color = Services.UIManager.legalGlowColor;
            energyLevelBack.color = Services.UIManager.legalGlowColor;
        }
        else
        {
            energyLevelFront.color = Services.UIManager.notLegalGlowColor;
            energyLevelBack.color = Services.UIManager.notLegalGlowColor;
        }
    }

    public void SetAttackLevel(float prop)
    {
        SetAttackFillAmount(Mathf.Min(1, prop));
        if (prop >= 1)
        {
            attackLevelFront.color = Services.UIManager.legalGlowColor;
            attackLevelBack.color = Services.UIManager.legalGlowColor;
        }
        else
        {
            attackLevelFront.color = Services.UIManager.notLegalGlowColor;
            attackLevelBack.color = Services.UIManager.notLegalGlowColor;
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
        if(owner == null)
        {
            spriteBottom.enabled = false;
            icon.color = Services.GameManager.NeutralColor;
        }
        else
        {
            if (!initialBase)
            {
                spriteBottom.material.SetFloat("_Cutoff", 1);
                beingClaimed = true;
                claimTimeElapsed = 0;
            }
            else
            {
                beingClaimed = false;
                spriteBottom.material.SetFloat("_Cutoff", 0);
            }
            spriteBottom.enabled = true;
            SetBaseColor(owner.ColorScheme[0]);
        }
    }
}

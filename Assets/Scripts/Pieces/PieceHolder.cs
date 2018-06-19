﻿using System.Collections;
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
    private const float energyCutoffMin = 0.1f;
    private const float energyDisplayOffset = 1.5f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (changingColor) LerpToTargetColor();
    }

    public void ShiftColor(Color color)
    {
        targetColor = color;
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

    private void SetEnergyFillAmount(float amount)
    {
        float fillAmt = ((1 - amount) * (energyCutoffMax-energyCutoffMin)) + energyCutoffMin;
        energyLevelFront.material.SetFloat("_Cutoff", fillAmt);
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
}

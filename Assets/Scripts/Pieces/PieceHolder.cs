using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceHolder : MonoBehaviour {

    public SpriteRenderer holderSelectionArea;
    public SpriteRenderer spriteBottom;
    public SpriteRenderer dropShadow;
    public SpriteRenderer icon;
    public SpriteRenderer legalityOverlay;
    public SpriteRenderer energyLevel;
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
}

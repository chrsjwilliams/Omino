using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorShifter : MonoBehaviour {

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
    private SpriteRenderer sr;
    private bool changingColor;
    private float colorChangeTimeElapsed;
    public float colorChangeDuration;

    // Use this for initialization
    void Start () {
        sr = GetComponent<SpriteRenderer>();
        prevColor = sr.color;
	}

    // Update is called once per frame
    void Update()
    {
        if (changingColor) LerpToTargetColor();
    }

    void LerpToTargetColor()
    {
        colorChangeTimeElapsed += Time.deltaTime;
        sr.color = Color.Lerp(prevColor, targetColor, colorChangeTimeElapsed / colorChangeDuration);
        if (colorChangeTimeElapsed >= colorChangeDuration)
        {
            changingColor = false;
        }
    }
    public void ShiftColor(Color target)
    {
        targetColor = target;
    }

}

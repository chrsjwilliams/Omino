using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandicapSystem : MonoBehaviour
{

    public Slider typeSlider;
    public Slider handicapSlider;

    public bool useBlueprintHandicap;
    public float currentTypeValue;
    public float typeMaxValue;
    public float typeMinValue;

    [SerializeField]
    private float lerpSpeed;

    public float handicapValue;

	// Use this for initialization
	void Start () {
        typeMaxValue = typeSlider.maxValue;
        typeMinValue = typeSlider.minValue;
        currentTypeValue = typeMinValue;

        lerpSpeed = 18.0f;
	}

    public void UpdateTypeSlider()
    {
        //if (currentTypeValue == 0)
        //{
        //    useBlueprintHandicap = true;
        //    currentTypeValue = 1;
        //}
        //else
        //{
        //    useBlueprintHandicap = false;
        //    currentTypeValue = 0;
        //}
    }

    public void UpdateHandicapValueSlider()
    {
        //(1+(1*.1)) - .1
        handicapValue = (1 + (handicapSlider.value * 0.1f)) - 0.1f;
    }
	
	// Update is called once per frame
	void Update () {

        typeSlider.value = Mathf.Lerp(typeSlider.value, currentTypeValue, Time.deltaTime * lerpSpeed);
        UpdateHandicapValueSlider();
	}
}

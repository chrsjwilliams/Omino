using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public Color onColor;
    public Color offColor;
    public Image toggleImage;
    public bool useExpansions;
	// Use this for initialization
	void Start () {
        if(Services.GameManager.levelSelected.isNewEditLevel())
            SetToggleImageColor(false);
	}

    public void OnPointerDown(PointerEventData eventData)
    {
        ((EditSceneScript)Services.GameScene).ToggleExpansions();
        useExpansions = !useExpansions;
        SetToggleImageColor(useExpansions);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

    public void SetToggleImageColor(bool b)
    {
        useExpansions = b;
        if (useExpansions)
        {
            toggleImage.color = onColor;
        }
        else
        {
            toggleImage.color = offColor;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}

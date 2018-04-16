using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialTooltip : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI textComponent;
    [SerializeField]
    private TextMeshProUGUI dismissText;
    [SerializeField]
    private Image arrow;
    [SerializeField]
    private float scaleDuration;
    private float timeElapsed;
    private bool scalingUp;
    private bool scalingDown;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (scalingUp)
        {
            timeElapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
                EasingEquations.Easing.QuadEaseOut(timeElapsed / scaleDuration));
            if (timeElapsed >= scaleDuration) scalingUp = false;
        }
        else if (scalingDown)
        {
            timeElapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero,
                EasingEquations.Easing.QuadEaseOut(timeElapsed / scaleDuration));
            if (timeElapsed >= scaleDuration)
            {
                scalingDown = false;
                Destroy(gameObject);
            }
        }
    }

    public void Init(TooltipInfo info)
    {
        GetComponent<RectTransform>().anchoredPosition = info.location;
        textComponent.text = info.text;
        GetComponent<Button>().enabled = info.dismissable;
        dismissText.enabled = info.dismissable;
        if (info.arrowLocation == Vector2.zero) arrow.enabled = false;
        else
        {
            arrow.GetComponent<RectTransform>().anchoredPosition = info.arrowLocation;
            arrow.transform.localRotation = Quaternion.Euler(0, 0, info.arrowRotation);
        }
        transform.localScale = Vector3.zero;
        scalingUp = true;
        RectTransform windowRect = 
            Services.TutorialManager.backDim.GetComponent<RectTransform>();
        windowRect.sizeDelta = info.windowSize;
        windowRect.anchoredPosition = info.windowLocation;
    }

    public void Dismiss()
    {
        //Destroy(gameObject);
        Services.TutorialManager.OnDismiss();
        scalingDown = true;
        timeElapsed = 0;
    }

}

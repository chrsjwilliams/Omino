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
    private Image image;
    [SerializeField]
    private Image subImage;
    [SerializeField]
    private Image subImage2;
    [SerializeField]
    private float scaleDuration;
    [SerializeField]
    private Animator imageAnim;
    private float timeElapsed;
    private bool scalingUp;
    private bool scalingDown;
    private string currentAnimation;

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

        image.rectTransform.anchoredPosition = info.imageLocation;
        image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.imageRotation);
        image.color = info.imageColor;
        imageAnim = image.GetComponent<Animator>();
        
        subImage.rectTransform.anchoredPosition = info.subImageLocation;
        subImage.rectTransform.localRotation = Quaternion.Euler(0, 0, info.subImageRotation);
        subImage.color = info.subImageColor;

        subImage2.rectTransform.anchoredPosition = info.subImageLocation;
        subImage2.color = subImage.color;

        if (info.arrowLocation == Vector2.zero) arrow.enabled = false;
        else
        {
            arrow.GetComponent<RectTransform>().anchoredPosition = info.arrowLocation;
            arrow.transform.localRotation = Quaternion.Euler(0, 0, info.arrowRotation);
        }
        if(!info.haveImage)
        {
            TurnOffAnimation();
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

    public void ToggleImageAnimation(string animationBool, bool b)
    {
        if(currentAnimation == "")
        {
            currentAnimation = animationBool;
        }
        else if(currentAnimation != animationBool && b)
        {
            imageAnim.SetBool(currentAnimation, false);
            currentAnimation = animationBool;
        }
        imageAnim.SetBool(currentAnimation, b);
    }

    public void TurnOffAnimation()
    {
        image.color = Services.GameManager.AdjustColorAlpha(image.color, 0);
        subImage.enabled = false;
        subImage2.enabled = false;

        foreach (AnimatorControllerParameter parameter in imageAnim.parameters)
        {
            imageAnim.SetBool(parameter.name, false);
        }
    }

}

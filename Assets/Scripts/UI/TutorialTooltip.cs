﻿
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TutorialTooltip : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private int touchID;

    public string label { get; private set; }

    [SerializeField]
    private TextMeshProUGUI textComponent;
    [SerializeField]
    private TextMeshProUGUI dismissText;
    public bool dismissible { get; private set; }
    [SerializeField]
    private Image arrow;
    
    public Image textBox;
    private bool lerps;
    [SerializeField]
    private Image image;
    private Vector3 imagePrimaryPosition;
    private Vector3 imageSecondaryPosition;
    private Vector3 imageSecondaryTempPosition;
    [SerializeField]
    private Image subImage;
    private Vector3 subImagePrimaryPosition;
    [SerializeField]
    private Image subImage2;
    [SerializeField]
    private float scaleDuration;
    [SerializeField]
    private Animator imageAnim;
    private float timeElapsed;
    private float timeDisplayed;
    private bool scalingUp;
    private bool scalingDown;
    private string currentAnimation;

    private float dismissTimer;

    private bool haveSelectedPiece = false;

    private const float MIN_TIME_ANIMATION_DISPLAYED = 1;

	// Use this for initialization
	void Start () {
        textBox = GetComponent<Image>();
        textComponent.richText = true;
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
        else
        {
            dismissTimer -= Time.deltaTime;
            if(dismissible && dismissTimer <= 0)
            {
                dismissText.color = Color.Lerp(dismissText.color, Color.red, Time.deltaTime * 10);
            }
        }

        if(label == "Rotate")
        {

            RotationSpecificToolTipUpdates();
        }

        if (lerps)
        {
            image.rectTransform.localPosition = Vector3.Lerp(image.rectTransform.localPosition, imageSecondaryPosition, EasingEquations.Easing.QuadEaseOut(Time.deltaTime * 1f));

            if (label == "Place Piece" && image.rectTransform.localPosition.x > imageSecondaryPosition.x - 1)
            {

                image.rectTransform.localPosition = imagePrimaryPosition;
            }
        }
    }

    void RotationSpecificToolTipUpdates()
    {
        if (Services.GameManager.Players[0].selectedPiece == null )
        {
            TurnOffAnimation();
            ToggleImageAnimation("Place Piece");
            imageSecondaryPosition = imageSecondaryTempPosition;
        }
        else
        {


                TurnOffAnimation();
                imageSecondaryPosition = new Vector2(-200, -250);
                image.rectTransform.localPosition = imageSecondaryPosition;
            

            if(Services.GameManager.CurrentDevice == DEVICE.IPHONE ||
                    Services.GameManager.CurrentDevice == DEVICE.IPHONE_X)
            {
                TurnOffAnimation();
                imageSecondaryPosition = new Vector2(-200, -385);
                image.rectTransform.localPosition = imageSecondaryPosition;
            }

            ToggleImageAnimation("Rotate");
            
        }
    }

    public void Init(TooltipInfo info)
    {
        RectTransform tooltipTransform = GetComponent<RectTransform>();
        RectTransform windowRect =
            Services.TutorialManager.backDim.GetComponent<RectTransform>();
        RectTransform secondWindow =
            Services.TutorialManager.secondWindow.GetComponent<RectTransform>();
        touchID = -1;
        label = info.label;

        if(label.Contains("Show Me"))
        {
            dismissText.text = "show me";
        }

        if(label.Contains("Complete"))
        {
            dismissText.text = "tutorial complete!";
        }

        if(label.Contains("Rotate"))
        {
            Services.GameEventManager.Register<RotationEvent>(OnRotation);
        }

        if (Services.GameManager.CurrentDevice == DEVICE.IPHONE)
        {

            // TOOLTIP
            tooltipTransform.anchoredPosition = info.iPhoneLocation;
            tooltipTransform.sizeDelta = info.iPhoneToolTipSize;

            // ARROW
            if (info.arrowLocation == Vector2.zero) arrow.enabled = false;
            else
            {
                arrow.GetComponent<RectTransform>().anchoredPosition = info.iPhoneArrowLocation;
                arrow.transform.localRotation = Quaternion.Euler(0, 0, info.iPhoneArrowRotation);
                arrow.transform.localScale = info.iPhoneArrowScale;
            }

            // WINDOW
            windowRect.anchoredPosition = info.iPhoneWindowLocation;
            windowRect.sizeDelta = info.iPhoneWindowSize;
            secondWindow.anchoredPosition = info.iPhoneSecondWindowLocation;
            secondWindow.sizeDelta = info.iPhoneSecondWindowSize;

            // IMAGE
            imagePrimaryPosition = info.iPhoneImageLocation;
            image.rectTransform.localPosition = imagePrimaryPosition;
            imageSecondaryPosition = info.iPhoneSecondaryImageLocation;
            imageSecondaryTempPosition = info.iPhoneSecondaryImageLocation;
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.iPhoneImageRotation);
            image.rectTransform.localScale = info.iPhoneImageScale;

            //  Font Size
            textComponent.fontSize = 60;
        }
        else if (Services.GameManager.CurrentDevice == DEVICE.IPHONE_X)
        {
            // TOOLTIP
            tooltipTransform.anchoredPosition = info.iPhoneXLocation;
            tooltipTransform.sizeDelta = info.iPhoneXToolTipSize;

            // ARROW
            if (info.arrowLocation == Vector2.zero) arrow.enabled = false;
            else
            {
                arrow.GetComponent<RectTransform>().anchoredPosition = info.iPhoneXArrowLocation;
                arrow.transform.localRotation = Quaternion.Euler(0, 0, info.iPhoneXArrowRotation);
                arrow.transform.localScale = info.iPhoneXArrowScale;
            }

            // WINDOW
            windowRect.anchoredPosition = info.iPhoneXWindowLocation;
            windowRect.sizeDelta = info.iPhoneXWindowSize;
            secondWindow.anchoredPosition = info.iPhoneXSecondWindowLocation;
            secondWindow.sizeDelta = info.iPhoneXSecondWindowSize;

            // IMAGE
            imagePrimaryPosition = info.iPhoneXImageLocation;
            image.rectTransform.localPosition = imagePrimaryPosition;
            imageSecondaryPosition = info.iPhoneXSecondaryImageLocation;
            imageSecondaryTempPosition = info.iPhoneXSecondaryImageLocation;
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.iPhoneXImageRotation);
            image.rectTransform.localScale = info.iPhoneXImageScale;
        }
        else if(Services.GameManager.CurrentDevice == DEVICE.IPAD_PRO)
        {
            // TOOLTIP
            tooltipTransform.anchoredPosition = info.iPadProLocation;
            tooltipTransform.sizeDelta = info.iPadProToolTipSize;

            // ARROW
            if (info.arrowLocation == Vector2.zero) arrow.enabled = false;
            else
            {
                arrow.GetComponent<RectTransform>().anchoredPosition = info.iPadProArrowLocation;
                arrow.transform.localRotation = Quaternion.Euler(0, 0, info.iPadProArrowRotation);
                arrow.transform.localScale = info.iPadProArrowScale;
            }

            // WINDOW
            windowRect.anchoredPosition = info.iPadProWindowLocation;
            windowRect.sizeDelta = info.iPadProWindowSize;
            secondWindow.anchoredPosition = info.iPadProSecondWindowLocation;
            secondWindow.sizeDelta = info.iPadProSecondWindowSize;

            // IMAGE
            imagePrimaryPosition = info.iPadProImageLocation;
            image.rectTransform.localPosition = imagePrimaryPosition;
            imageSecondaryPosition = info.iPadProSecondaryImageLocation;
            imageSecondaryTempPosition = info.iPadProSecondaryImageLocation;
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.iPadProImageRotation);
            image.rectTransform.localScale = info.iPadProImageScale;
        }
        else if(Services.GameManager.CurrentDevice == DEVICE.IPAD_11INCH)
        {
            // TOOLTIP
            tooltipTransform.anchoredPosition = info.iPad11InchLocation;
            tooltipTransform.sizeDelta = info.iPad11InchToolTipSize;

            // ARROW
            if (info.arrowLocation == Vector2.zero) arrow.enabled = false;
            else
            {
                arrow.GetComponent<RectTransform>().anchoredPosition = info.iPad11InchArrowLocation;
                arrow.transform.localRotation = Quaternion.Euler(0, 0, info.iPad11InchArrowRotation);
                arrow.transform.localScale = info.iPad11InchArrowScale;
            }

            // WINDOW
            windowRect.anchoredPosition = info.iPad11InchWindowLocation;
            windowRect.sizeDelta = info.iPad11InchWindowSize;
            secondWindow.anchoredPosition = info.iPad11InchSecondWindowLocation;
            secondWindow.sizeDelta = info.iPad11InchSecondWindowSize;

            // IMAGE
            imagePrimaryPosition = info.iPad11InchImageLocation;
            image.rectTransform.localPosition = imagePrimaryPosition;
            imageSecondaryPosition = info.iPad11InchSecondaryImageLocation;
            imageSecondaryTempPosition = info.iPad11InchSecondaryImageLocation;
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.iPad11InchImageRotation);
            image.rectTransform.localScale = info.iPad11InchImageScale;
        }
        else
        {
            // TOOLTIP
            tooltipTransform.anchoredPosition = info.location;

            // ARROW
            if (info.arrowLocation == Vector2.zero) arrow.enabled = false;
            else
            {
                arrow.GetComponent<RectTransform>().anchoredPosition = info.arrowLocation;
                arrow.transform.localRotation = Quaternion.Euler(0, 0, info.arrowRotation);
            }

            // WINDOW
            windowRect.sizeDelta = info.windowSize;
            windowRect.anchoredPosition = info.windowLocation;
            secondWindow.anchoredPosition = info.secondWindowLocation;
            secondWindow.sizeDelta = info.secondWindowSize;

            // IMAGE
            imagePrimaryPosition = info.imageLocation;
            image.rectTransform.localPosition = imagePrimaryPosition;
            imageSecondaryPosition = info.secondaryImageLocation;
            imageSecondaryTempPosition = info.secondaryImageLocation;
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.imageRotation);
            image.rectTransform.localScale = info.imageScale;
        }

        textComponent.text = info.text.ToLower();

        dismissText.enabled = info.dismissable;

        dismissible = info.dismissable;

        lerps = info.imageLerps;

        image.color = info.imageColor;  

        imageAnim = image.GetComponent<Animator>();

        ToggleImageAnimation(label);
        if (label == "Rotate") TurnOffAnimation();

        transform.localScale = Vector3.zero;
        scalingUp = true;
       
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
    }

    public void Dismiss(bool displayNext = true)
    {
        if (label.Contains("Rotate"))
        {
            Services.GameEventManager.Unregister<RotationEvent>(OnRotation);
        }
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);

        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);

        Services.TutorialManager.OnDismiss(displayNext);
        
        
        scalingDown = true;
        timeElapsed = 0;    
    }

    public void ToggleImageAnimation(string animationParam)
    {
        if (HasParameter(animationParam))
        {
            imageAnim.SetBool(animationParam, true);
            if (dismissible)
            {
                int clipIndex = 0;
                foreach (AnimationClip anim in imageAnim.runtimeAnimatorController.animationClips)
                {
                    if (anim.name == animationParam)
                        break;
                    clipIndex++;
                }


                dismissTimer = imageAnim.runtimeAnimatorController.animationClips[clipIndex].length + 0.33f;
            }
        }
        else
        {
            dismissTimer = 0.5f;
            TurnOffAnimation();
        }

        
    }

    public void TurnOffAnimation()
    {

        image.color = Services.GameManager.AdjustColorAlpha(image.color, 0);
        subImage.enabled = false;
        subImage2.enabled = false;

        foreach (AnimatorControllerParameter parameter in imageAnim.parameters) //  here
        {
            imageAnim.SetBool(parameter.name, false);
        }
    }

    protected void OnRotation(RotationEvent e)
    {
        textComponent.text = "well done! now place the piece on the board.";
    }

    protected void OnTouchDown(TouchDown e)
    {
        Vector2 touchWorldPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(textBox.rectTransform, Input.mousePosition, null, out touchWorldPos);
        Vector2 normalizedPoint = Rect.PointToNormalized(textBox.rectTransform.rect, touchWorldPos);
        
        if (touchID == -1)
        {
            touchID = e.touch.fingerId;
            OnInputDown(normalizedPoint);
        }
    }

    protected void OnMouseDownEvent(MouseDown e)
    {
        Vector2 mouseWorldPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(textBox.rectTransform, Input.mousePosition, null, out mouseWorldPos);
        Vector2 normalizedPoint = Rect.PointToNormalized(textBox.rectTransform.rect, mouseWorldPos);
        OnInputDown(normalizedPoint);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (dismissible && dismissTimer <= 0)
        {
            Dismiss();
        }
    }

    public virtual void OnInputDown(Vector3 touchPos)
    {
        //if (dismissible)
        //{
        //    Dismiss();
        //}
    }

    protected void OnTouchUp(TouchUp e)
    {
        if (e.touch.fingerId == touchID)
        {
            OnInputUp();
            touchID = -1;
        }
    }

    protected void OnMouseUpEvent(MouseUp e)
    {
        OnInputUp();
    }

    public virtual void OnInputUp()
    {
    }

    public bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in imageAnim.parameters) // here
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}

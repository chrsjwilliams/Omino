using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TutorialTooltip : MonoBehaviour, IPointerDownHandler
{
    private bool canView = true;
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
    private Vector3 imageSecondaryPosition = Vector3.back;
    [SerializeField]
    private Image subImage;
    private Vector3 subImagePrimaryPosition;
    private Vector3 subImageSecondaryPosition = Vector3.back;
    [SerializeField]
    private Image subImage2;
    private Vector3 subImage2PrimaryPosition;
    private Vector3 subImage2SecondaryPosition = Vector3.back;
    [SerializeField]
    private float scaleDuration;
    [SerializeField]
    private Animator imageAnim;
    private float timeElapsed;
    private float timeDisplayed;
    private bool scalingUp;
    private bool scalingDown;
    private string currentAnimation;

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
            //timeDisplayed += Time.unscaledDeltaTime;
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
        if (Services.GameManager.Players[0].selectedPiece == null && !haveSelectedPiece)
        {
            textComponent.text = "First, drag a piece to the board...";
            ToggleImageAnimation("Place Piece");
        }
        else
        {

            if (!haveSelectedPiece)
            {
                TurnOffAnimation();
                imageSecondaryPosition = new Vector2(-200, -250);
                image.rectTransform.localPosition = imageSecondaryPosition;
            }

            if(!haveSelectedPiece && Services.GameManager.onIPhone)
            {
                TurnOffAnimation();
                imageSecondaryPosition = new Vector2(-200, -250);
                image.rectTransform.localPosition = imageSecondaryPosition;
            }

            haveSelectedPiece = true;
            textComponent.text = "Then tap on the screen with a different finger to <color=#6CAE75>ROTATE</color>.";
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
            dismissText.text = "Show Me";
        }

        if(label.Contains("Complete"))
        {
            dismissText.text = "Tutorial Complete!";
        }

        if (Services.GameManager.onIPhone)
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
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.iPhoneImageRotation);
            image.rectTransform.localScale = info.iPhoneImageScale;
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
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.imageRotation);
            image.rectTransform.localScale = info.imageScale;
        }

        textComponent.text = info.text;

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
        }
        else if (animationParam.Contains("Block Opponent Base"))
        {
            imageAnim.SetBool("Block Opponent Base", true);
        }
        else
        {
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
        if (dismissible)
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

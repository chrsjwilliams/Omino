using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TutorialTooltip : MonoBehaviour, IPointerDownHandler
{

    [SerializeField]
    private int touchID;

    [SerializeField]
    private string tag;

    [SerializeField]
    private TextMeshProUGUI textComponent;
    [SerializeField]
    private TextMeshProUGUI dismissText;
    private bool dismissible;
    [SerializeField]
    private Image arrow;
    
    public Image textBox;
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
    private bool scalingUp;
    private bool scalingDown;
    private string currentAnimation;

    private bool haveSelectedPiece = false;

	// Use this for initialization
	void Start () {
        textBox = GetComponent<Image>();
        textComponent.richText = true;
    }


    private void OnDestroy()
    {
        
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

        if(tag == "Rotate")
        {

            RotationSpecificToolTipUpdates();
        }
        
        if (imageSecondaryPosition.z != -1)
        {
            image.rectTransform.localPosition = Vector3.Lerp(image.rectTransform.localPosition, imageSecondaryPosition, EasingEquations.Easing.QuadEaseOut(Time.deltaTime));

            if (tag == "Place Piece" && image.rectTransform.localPosition.y > imageSecondaryPosition.y * 0.95f)
            {

                image.rectTransform.localPosition = imagePrimaryPosition;
            }
        }
    }

    void RotationSpecificToolTipUpdates()
    {
        

        if (Services.GameManager.Players[0].selectedPiece == null && !haveSelectedPiece)
        {
            textComponent.text = "First, drag a piece to the board";
            ToggleImageAnimation("Place Piece");
        }
        else
        {
            if (!haveSelectedPiece) TurnOffAnimation();
            haveSelectedPiece = true;
            textComponent.text = "While holding the piece, tap the screen with a second finger to rotate the piece";
            ToggleImageAnimation("Rotate");
        }
    }

    public void Init(TooltipInfo info)
    {
        touchID = -1;
        tag = info.tag;

        if(tag.Contains("Show Me"))
        {
            dismissText.text = "Show Me";
        }

        GetComponent<RectTransform>().anchoredPosition = info.location;
        textComponent.text = info.text;

        //GetComponent<Button>().enabled = info.dismissable;
        dismissText.enabled = info.dismissable;

        dismissible = info.dismissable;

        imagePrimaryPosition = info.imageLocation;
        image.rectTransform.localPosition = imagePrimaryPosition;
        imageSecondaryPosition = info.secondaryImageLocation;
        image.rectTransform.localRotation = Quaternion.Euler(0, 0, info.imageRotation);
        image.rectTransform.localScale = info.imageScale;
        image.color = info.imageColor;
        

        imageAnim = image.GetComponent<Animator>();

        subImagePrimaryPosition = info.subImageLocation;
        subImage.rectTransform.anchoredPosition = subImagePrimaryPosition;
        subImageSecondaryPosition = info.secondarySubImageLocation;
        subImage.rectTransform.localRotation = Quaternion.Euler(0, 0, info.subImageRotation);
        subImage.rectTransform.localScale = info.subImageScale;
        subImage.color = info.subImageColor;

        subImage2PrimaryPosition = info.subImage2Location;
        subImage2.rectTransform.anchoredPosition = subImage2PrimaryPosition;
        subImage2SecondaryPosition = info.secondarySubImage2Location;
        subImage2.rectTransform.localRotation = Quaternion.Euler(0, 0, info.subImage2Rotation);
        subImage2.rectTransform.localScale = info.subImage2Scale;
        subImage2.color = info.subImage2Color;

        if (info.arrowLocation == Vector2.zero) arrow.enabled = false;
        else
        {
            arrow.GetComponent<RectTransform>().anchoredPosition = info.arrowLocation;
            arrow.transform.localRotation = Quaternion.Euler(0, 0, info.arrowRotation);
        }

        ToggleImageAnimation(tag);
        if (tag == "Rotate") TurnOffAnimation();


        transform.localScale = Vector3.zero;
        scalingUp = true;
        RectTransform windowRect = 
            Services.TutorialManager.backDim.GetComponent<RectTransform>();
        windowRect.sizeDelta = info.windowSize;
        windowRect.anchoredPosition = info.windowLocation;

        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);

    }

    public void Dismiss()
    {
        //Destroy(gameObject);
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);

        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
        Services.TutorialManager.OnDismiss();
        

        scalingDown = true;
        timeElapsed = 0;

        
    }

    public void ToggleImageAnimation(string animationParam)
    {
        //if(currentAnimation == "")
        //{
        //    currentAnimation = animationParam;
        //}
        //else if(currentAnimation != animationParam && b)
        //{
        //    imageAnim.SetBool(currentAnimation, false);
        //    currentAnimation = animationParam;
        //}
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

        foreach (AnimatorControllerParameter parameter in imageAnim.parameters)
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


        if(IsPointContainedWithinHolderArea(touchPos) && dismissible)
        {
            Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
            Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);

        }

        //Services.GameEventManager.Register<TouchMove>(OnTouchMove);
        //Services.GameEventManager.Register<TouchUp>(OnTouchUp);

        //Services.GameEventManager.Register<MouseMove>(OnMouseMoveEvent);
        //Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
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
        //Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        //Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        //Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);

        //Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        //Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
        //Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
    }

    protected void OnMouseMoveEvent(MouseMove e)
    {
        OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos));
    }

    protected void OnTouchMove(TouchMove e)
    {
        if (e.touch.fingerId == touchID)
        {
            OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position));
        }
    }

    public virtual void OnInputDrag(Vector3 inputPos)
    {
    }

    public virtual bool IsPointContainedWithinHolderArea(Vector3 point)
    {
        //Debug.Assert(holder.holderSelectionArea != null);
        Vector3 extents;
        Vector3 centerPoint;

        extents = textBox.sprite.bounds.extents;
        centerPoint = Services.GameManager.MainCamera.ScreenToWorldPoint(transform.position);

        return  0.1f < point.x && point.x < 0.9f &&
                0.1f < point.y && point.y < 0.9f;
    }

    public bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in imageAnim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}

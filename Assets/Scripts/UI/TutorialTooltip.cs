using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialTooltip : MonoBehaviour {

    [SerializeField]
    private int touchID;

    [SerializeField]
    private TextMeshProUGUI textComponent;
    [SerializeField]
    private TextMeshProUGUI dismissText;
    [SerializeField]
    private Image arrow;
    
    public Image textBox;
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
        textBox = GetComponent<Image>();
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
    }

    public void Init(TooltipInfo info)
    {
        touchID = -1;
        GetComponent<RectTransform>().anchoredPosition = info.location;
        textComponent.text = info.text;
        //GetComponent<Button>().enabled = info.dismissable;
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

    public virtual void OnInputDown(Vector3 touchPos)
    {
        Debug.Log("Touch Down OUT");

        if(IsPointContainedWithinHolderArea(touchPos))
        {
            Debug.Log("Touch Down IN");

            Dismiss();
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
}

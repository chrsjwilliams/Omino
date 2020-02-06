using UnityEngine;

public class CanvasScroller : MonoBehaviour {

    private Vector2 holdPos;
    private int touchID;
    private bool held;
    private RectTransform rect;
    private Vector2 basePos;
    [HideInInspector]
    public float maxY;
    [HideInInspector]
    public float minY;

	// Use this for initialization
	void Start () {
        touchID = -1;
        Services.GlobalEventManager.Register<TouchDown>(OnTouchDown);
        Services.GlobalEventManager.Register<MouseDown>(OnMouseDown);
        rect = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTouchDown(TouchDown e)
    {
        if (touchID == -1)
        {
            touchID = e.touch.fingerId;
            OnInputDown(e.touch.position);
        }
    }

    private void OnMouseDown(MouseDown e)
    {
        OnInputDown(e.mousePos);
    }

    private void OnInputDown(Vector3 inputPos)
    {
        holdPos = inputPos;
        basePos = rect.anchoredPosition;

        Services.GlobalEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GlobalEventManager.Register<TouchMove>(OnTouchMove);
        Services.GlobalEventManager.Register<TouchUp>(OnTouchUp);

        Services.GlobalEventManager.Unregister<MouseDown>(OnMouseDown);
        Services.GlobalEventManager.Register<MouseMove>(OnMouseMove);
        Services.GlobalEventManager.Register<MouseUp>(OnMouseUp);
    }

    private void OnTouchUp(TouchUp e)
    {
        if(e.touch.fingerId == touchID)
        {
            OnInputUp();
        }
    }

    private void OnMouseUp(MouseUp e)
    {
        OnInputUp();
    }

    private void OnInputUp()
    {
        touchID = -1;

        Services.GlobalEventManager.Register<TouchDown>(OnTouchDown);
        Services.GlobalEventManager.Unregister<TouchUp>(OnTouchUp);

        Services.GlobalEventManager.Register<MouseDown>(OnMouseDown);
        Services.GlobalEventManager.Unregister<MouseMove>(OnMouseMove);
        Services.GlobalEventManager.Unregister<MouseUp>(OnMouseUp);
    }

    private void OnMouseMove(MouseMove e)
    {
        OnInputDrag(e.mousePos);
    }

    private void OnTouchMove(TouchMove e)
    {
        if (e.touch.fingerId == touchID)
        {
            OnInputDrag(e.touch.position);
        }
    }

    private void OnInputDrag(Vector2 inputPos)
    {
        float yPos = basePos.y + inputPos.y - holdPos.y;
        yPos = Mathf.Clamp(yPos, minY, maxY);
        rect.anchoredPosition = new Vector2(basePos.x, yPos);
    }

    private void OnDestroy()
    {
        Services.GlobalEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GlobalEventManager.Unregister<TouchMove>(OnTouchMove);
        Services.GlobalEventManager.Unregister<TouchUp>(OnTouchUp);

        Services.GlobalEventManager.Unregister<MouseDown>(OnMouseDown);
        Services.GlobalEventManager.Unregister<MouseMove>(OnMouseMove);
        Services.GlobalEventManager.Unregister<MouseUp>(OnMouseUp);
    }
}

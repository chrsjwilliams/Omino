using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectSnap : MonoBehaviour
{
    public enum DIRECTION { LEFT = -1, IDLE = 0, RIGHT = 1}

    protected int touchID;
    public bool dragging;
    public bool landscapeMode;
    public float swipeSpeed;
    public float dir = 1;
    public int currentIndex;
    public int selectedIndex;
    public int prevSelectedIndex;
    public int anticipatedIndex;
    public int imageDistance;
    public DIRECTION direction;
    public DIRECTION anticipatedDirection;
    public RectTransform panel;
    public Image[] images;
    public RectTransform center;
    public float moveBuffer;
    public float sensitivityThreshold;

    private bool levelSelected;
    private float[] distance;   
    private int minImageIndex;
    private float travelDistance;
    private Vector3 screenInputPos;
    private Vector3 initalTouchPos;
    private Vector3 prevTouchPos;
    private Vector3 maxSize = new Vector3(0.66f, 0.66f, 1.0f);
    private Vector3 minSize = new Vector3(0.33f, 0.33f, 1.0f);
    private int lastClickIndex = 0;

    public float t;

    // Use this for initialization
    public void Start ()
    {
        sensitivityThreshold = 0.01f;
        levelSelected = false;
        moveBuffer = 150;
        panel = GameObject.Find("ScrollPanel").GetComponent<RectTransform>();
        center = GameObject.Find("SelectedLevel").GetComponent<RectTransform>();
        int numOfLevels = panel.transform.childCount;
        images = new Image[numOfLevels];
        for (int i = 0; i < numOfLevels; i++)
        {
            images[i] = panel.transform.GetChild(i).GetComponent<Image>();
        }

        distance = new float[numOfLevels];

        imageDistance = (int)Mathf.Abs(images[2].GetComponent<RectTransform>().anchoredPosition.x - 
                                        images[1].GetComponent<RectTransform>().anchoredPosition.x);

        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);

        landscapeMode = true;

        swipeSpeed = 30.0f;
        direction = DIRECTION.IDLE;
        anticipatedDirection = DIRECTION.IDLE;
        minImageIndex = 0;
        touchID = -1;

        prevSelectedIndex = 0;
        anticipatedIndex = 0;
    }

    private void OnDestroy()
    {
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);

        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
    }

    // Update is called once per frame
    void Update ()
    {
        for (int i = 0; i < images.Length; i++)
        {
            distance[i] = Mathf.Abs(center.transform.position.x - images[i].transform.position.x);
        }

        float minDistance = Mathf.Min(distance);

        prevSelectedIndex = minImageIndex;

        for (int i = 0; i < images.Length; i++)
        {
            if (minDistance == distance[i])
            {
                currentIndex = i;
                
                if (currentIndex > 5)
                    currentIndex = 5;
                
                if (currentIndex != lastClickIndex)
                {
                    Services.AudioManager.CreateTempAudio(Services.Clips.UIClick, 1.0f);
                    lastClickIndex = currentIndex;
                }
            }
        }

        ScaleSelectedImage(anticipatedIndex, currentIndex, EasingEquations.Easing.Linear(t));

        if (!dragging)
        {
            LerpToImage(minImageIndex * -imageDistance);
        }

    }

    public void SelectLevel()
    {
        Debug.Log(t);
        if (t > sensitivityThreshold) return;
        levelSelected = true;
        LevelButton selectedLevel = images[selectedIndex].GetComponent<LevelButton>();
        ((GameOptionsSceneScript)Services.Scenes.CurrentScene).SelectLevel(selectedLevel);
    }

    void LerpToImage(int position)
    {
        selectedIndex = currentIndex;

        float posX = Mathf.Lerp(panel.anchoredPosition.x, position, Time.deltaTime * 4f);
        Vector2 newPos = new Vector2(posX, panel.anchoredPosition.y);

        panel.anchoredPosition = newPos;
    }

    void ScaleSelectedImage(int anticipatedIndex, int prevIndex, float duration)
    {
        if (levelSelected) return;
        if(anticipatedDirection == DIRECTION.LEFT)
        {
            if (anticipatedIndex > selectedIndex)
            {
                images[anticipatedIndex].transform.localScale = Vector3.Lerp(minSize, maxSize, duration);
                images[selectedIndex].transform.localScale = Vector3.Lerp(maxSize, minSize, duration);
            }
            else if (anticipatedIndex < selectedIndex)
            {
                images[anticipatedIndex].transform.localScale = Vector3.Lerp(minSize, maxSize, 1 - duration);
                images[selectedIndex].transform.localScale = Vector3.Lerp(maxSize, minSize, 1 - duration);
            }
        }
        else if(anticipatedDirection == DIRECTION.RIGHT)
        {
            if (anticipatedIndex < selectedIndex)
            {
                images[anticipatedIndex].transform.localScale = Vector3.Lerp(minSize, maxSize, 1 - duration);
                images[selectedIndex].transform.localScale = Vector3.Lerp(maxSize, minSize, 1 - duration);
            }
            else if (anticipatedIndex > selectedIndex)
            {
                images[anticipatedIndex].transform.localScale = Vector3.Lerp(minSize, maxSize, duration);
                images[selectedIndex].transform.localScale = Vector3.Lerp(maxSize, minSize, duration);
            }
        }

        if (!dragging)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (i != selectedIndex)
                {
                    images[i].transform.localScale = Vector3.Lerp(images[i].transform.localScale, minSize, duration);
                }
                else
                {
                    images[i].transform.localScale = Vector3.Lerp(images[i].transform.localScale, maxSize, duration);
                }
            }
        }
    }

    protected void OnTouchDown(TouchDown e)
    {
        Vector3 touchWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position);
        if (touchID == -1)
        {
            touchID = e.touch.fingerId;
            OnInputDown(touchWorldPos);
        }
    }

    protected void OnMouseDownEvent(MouseDown e)
    {
        Vector3 mouseWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
        OnInputDown(mouseWorldPos);
    }

    public virtual void OnInputDown(Vector3 touchPos)
    {
        initalTouchPos = Services.GameManager.MainCamera.WorldToScreenPoint(touchPos);
        selectedIndex = currentIndex;

        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<TouchMove>(OnTouchMove);
        Services.GameEventManager.Register<TouchUp>(OnTouchUp);

        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Register<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
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
        dragging = false;
        switch (direction)
        {
            case DIRECTION.RIGHT:
                minImageIndex--;
                if (minImageIndex < 0) minImageIndex = 0;
                break;
            case DIRECTION.LEFT:
                minImageIndex++;
                if (minImageIndex > images.Length - 1) minImageIndex = images.Length - 1;
                break;
            default:
                break;
        }

        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);

        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);

        selectedIndex = currentIndex;
        direction = DIRECTION.IDLE;
        anticipatedDirection = DIRECTION.IDLE;
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
        
        dragging = true;
        screenInputPos = Services.GameManager.MainCamera.WorldToScreenPoint(inputPos);

        if (landscapeMode)
        {
            if (prevTouchPos.x > screenInputPos.x)
            {
                anticipatedDirection = DIRECTION.LEFT;
            }
            else if (prevTouchPos.x < screenInputPos.x)
            {
                anticipatedDirection = DIRECTION.RIGHT;
            }

            prevTouchPos = screenInputPos;

            if (screenInputPos.x > initalTouchPos.x)
            {
                dir = 1;

                anticipatedIndex = selectedIndex - 1;
                if (anticipatedIndex < 0) anticipatedIndex = 0;
            }
            else
            {
                dir = -1;

                anticipatedIndex = selectedIndex + 1;
                if (anticipatedIndex > images.Length - 1) anticipatedIndex = images.Length - 1;
            }

            if (screenInputPos.x  > initalTouchPos.x + moveBuffer)
            {
                direction = DIRECTION.RIGHT;

            }
            else if (screenInputPos.x < initalTouchPos.x - moveBuffer)
            {
                direction = DIRECTION.LEFT;
            }
            else
            {
                direction = DIRECTION.IDLE;
            }

            travelDistance = Mathf.Abs(initalTouchPos.x - screenInputPos.x);
        }
        else
        {
            if (prevTouchPos.y > screenInputPos.y)
            {
                anticipatedDirection = DIRECTION.RIGHT;
            }
            else if (prevTouchPos.y < screenInputPos.y)
            {
                anticipatedDirection = DIRECTION.LEFT;
            }

            if (screenInputPos.y > initalTouchPos.y)
            {
                dir = 1;

                anticipatedIndex = selectedIndex - 1;
                if (anticipatedIndex < 0) anticipatedIndex = 0;
            }
            else
            {
                dir = -1;

                anticipatedIndex = selectedIndex + 1;
                if (anticipatedIndex > images.Length - 1) anticipatedIndex = images.Length - 1;
            }

            if (screenInputPos.y > initalTouchPos.y) dir = 1;
            else dir = -1;

            if (screenInputPos.y < initalTouchPos.y - moveBuffer)
            {
                direction = DIRECTION.LEFT;
            }
            else if (screenInputPos.y > initalTouchPos.y + moveBuffer)
            {
                direction = DIRECTION.RIGHT;
            }
            else
            {
                direction = DIRECTION.IDLE;
            }

            travelDistance = Mathf.Abs(initalTouchPos.y - screenInputPos.y);
        }

        prevTouchPos = screenInputPos;

        if (travelDistance > imageDistance) travelDistance = imageDistance;

        Vector3 newPanelPos = new Vector3(travelDistance * dir + ((minImageIndex * -imageDistance)), 0, 0);
       
        panel.anchoredPosition = Vector3.Lerp(panel.anchoredPosition, newPanelPos, Time.deltaTime * swipeSpeed);


        float directionLimit = ((anticipatedIndex * imageDistance));
        float anchorPos = selectedIndex * imageDistance;

        float min = anchorPos < directionLimit ? anchorPos : directionLimit;

        t = (Mathf.Abs(panel.anchoredPosition.x) - min) / imageDistance;

    }
}

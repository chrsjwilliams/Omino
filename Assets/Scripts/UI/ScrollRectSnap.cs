using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectSnap : MonoBehaviour
{
    public enum DIRECTION { LEFT = -1, IDLE = 0, RIGHT = 1}

    protected int touchID;
    public bool dragging;
    public bool twoPlayers;
    public float swipeSpeed;
    public int selectedIndex;
    public int prevSelectedIndex;
    public DIRECTION direction;
    public RectTransform panel;
    public Image[] images;
    public RectTransform center;

    
    private float[] distance;
    public int imageDistance; 
    private int minImageIndex;
    private float travelDistance;
    private Vector3 initalTouchPos;
    private Vector3 maxSize = new Vector3(0.5f, 0.5f, 1.0f);
    private Vector3 minSize = new Vector3(0.3f, 0.3f, 1.0f);

    // Use this for initialization
    public void Start ()
    {
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

        twoPlayers = ((GameOptionsSceneScript)Services.Scenes.CurrentScene).humanPlayers[0] ==
            ((GameOptionsSceneScript)Services.Scenes.CurrentScene).humanPlayers[1];

        swipeSpeed = 30.0f;
        direction = DIRECTION.IDLE;
        minImageIndex = 0;
        touchID = -1;
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

        prevSelectedIndex = selectedIndex;
        for (int i = 0; i < images.Length; i++)
        {
            if (minDistance == distance[i])
            {
                selectedIndex = i;
                if (selectedIndex > 5)
                    selectedIndex = 5;
            }
        }

        ScaleSelectedImage(selectedIndex, prevSelectedIndex, travelDistance / imageDistance);
        if (!dragging)
        {
            LerpToImage(minImageIndex * -imageDistance);

        }
    }

    public void SelectLevel()
    {
        LevelButton selectedLevel = images[selectedIndex].GetComponent<LevelButton>();
        ((GameOptionsSceneScript)Services.Scenes.CurrentScene).SelectLevel(selectedLevel);
    }

    void LerpToImage(int position)
    {
        float posX = Mathf.Lerp(panel.anchoredPosition.x, position, Time.deltaTime * 4f);
        Vector2 newPos = new Vector2(posX, panel.anchoredPosition.y);

        panel.anchoredPosition = newPos;
    }

    void ScaleSelectedImage(int currentIndex, int prevIndex, float duration)
    {
        images[currentIndex].transform.localScale = Vector3.Lerp(images[currentIndex].transform.localScale,
                                                               maxSize, duration);
        for(int i = 0; i < images.Length; i++)
        {
            if(i != currentIndex)
            {
                images[i].transform.localScale = Vector3.Lerp(images[i].transform.localScale, minSize, duration);
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
            case DIRECTION.LEFT:
                minImageIndex--;
                if (minImageIndex < 0) minImageIndex = 0;
                break;
            case DIRECTION.RIGHT:
                minImageIndex++;
                if (minImageIndex > images.Length - 1) minImageIndex = images.Length - 1;

                break;
            default:
                break;
        }
    
        direction = DIRECTION.IDLE;

        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);

        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
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
        Vector3 screenInputPos =
                Services.GameManager.MainCamera.WorldToScreenPoint(inputPos);

        float dir = 1;
        if (twoPlayers)
        {
            if (screenInputPos.x < initalTouchPos.x)
            {
                dir = -1;
                direction = DIRECTION.RIGHT;
            }
            else if (screenInputPos.x > initalTouchPos.x)
            {
                direction = DIRECTION.LEFT;
            }
        }
        else
        {
            if (screenInputPos.y < initalTouchPos.y)
            {
                direction = DIRECTION.LEFT;
            }
            else if (screenInputPos.y > initalTouchPos.y)
            {
                dir = -1;

                direction = DIRECTION.RIGHT;
            }
        }

        Debug.Log(direction.ToString());

        if(Mathf.Abs(initalTouchPos.y - screenInputPos.y) == imageDistance)
            Debug.Log(initalTouchPos.y - screenInputPos.y);

        if (twoPlayers)
        {
            travelDistance = Mathf.Abs(initalTouchPos.x - screenInputPos.x);
        }
        else
        {
            travelDistance = Mathf.Abs(initalTouchPos.y - screenInputPos.y);
        }

        if (travelDistance > imageDistance) travelDistance = imageDistance;

        Vector3 newPanelPos = new Vector3(travelDistance * dir + ((minImageIndex * -imageDistance)), 0, 0);
       
        panel.anchoredPosition = Vector3.Lerp(panel.anchoredPosition, newPanelPos, Time.deltaTime * swipeSpeed);
        
    }
}

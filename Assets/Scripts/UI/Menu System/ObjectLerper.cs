using UnityEngine;

public class ObjectLerper : MonoBehaviour
{
    private RectTransform rect;
    private Vector2 target;
    private Vector2 start;
    private float timeElapsed;
    private float delay;
    private bool moving;
    private float duration;
    // Use this for initialization
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (delay > 0) DecayDelay();
        if (moving) Move();
    }

    private void DecayDelay()
    {
        delay -= Time.deltaTime;
        if (delay <= 0)
        {
            StartMovement();
        }

    }

    private void Move()
    {
        timeElapsed += Time.deltaTime;

        rect.anchoredPosition = Vector2.Lerp(start, target,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / duration));
        if (timeElapsed >= duration)
        {
            rect.anchoredPosition = target;
            moving = false;
        }

    }

    private void StartMovement()
    {
        timeElapsed = 0;
        moving = true;
    }

    private void SetRect()
    {
        rect = GetComponent<RectTransform>();
    }

    public void LerpTo(Vector2 start, Vector2 target, float delay, float duration)
    {
        this.target = target;
        this.start = start;
        this.delay = delay + float.Epsilon;
        this.duration = duration;
        if (rect == null) SetRect();
        rect.anchoredPosition = start;
    }
}

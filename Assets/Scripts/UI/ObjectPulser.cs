using UnityEngine;
using System.Collections;

public class ObjectPulser : MonoBehaviour
{
    private float timeElapsed;
    private float period;
    private Vector3 minScale;
    private Vector3 maxScale;
    private bool scalingUp;
    private bool active;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if (scalingUp)
            {
                timeElapsed += Time.deltaTime;

                if (timeElapsed >= period) scalingUp = false;
            }
            else
            {
                timeElapsed -= Time.deltaTime;

                if (timeElapsed <= 0) scalingUp = true;
            }

            transform.localScale = Vector3.Lerp(minScale, maxScale,
                EasingEquations.Easing.QuadEaseOut(timeElapsed / period));

        }
    }

    public void StartPulse(float period, Vector3 minScale, Vector3 maxScale)
    {
        active = true;
        scalingUp = true;
        timeElapsed = 0;
        this.maxScale = maxScale;
        this.minScale = minScale;
        this.period = period;
    }

    public void StopPulse()
    {
        active = false;
        transform.localScale = Vector3.one;
    }
}

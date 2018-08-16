using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    private Vector3 basePos;
    private float shakeTime;
    private bool shaking;
    private float currentSeed;
    private float shakeSpeed;
    private float shakeMag;
    public Vector3[] screenEdges;
    private Camera theCamera;
    private bool slow_mo = false;

	// Use this for initialization
	void Start () {
        theCamera = GetComponent<Camera>();
        theCamera.orthographicSize *= 
            ((float)Screen.height / Screen.width) / (4f / 3);
    }

    public void SetScreenEdges()
    {
        screenEdges = new Vector3[2]
        {
            theCamera.ScreenToWorldPoint(new Vector3(Screen.width/2,0)),
            theCamera.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height))
        };
    }
	
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        basePos = pos;
        SetScreenEdges();
        //Debug.Log("map edge: " + GetMapEdgeScreenHeight());
        //Debug.Log("screen height: " + Screen.height);
    }

    // Update is called once per frame
    void Update () {
        if (shaking) Shake();	
	}

    public void StartShake(float dur, float speed, float magnitude, bool interrupt = false)
    {
        if (((!shaking) || (interrupt)) && (!slow_mo))
        {
            transform.position = basePos;
            currentSeed = Random.Range(0, 1000);
            shaking = true;
            shakeTime = dur;
            shakeSpeed = speed;
            shakeMag = magnitude;
        }
    }

    void Shake()
    {
        shakeTime -= Time.deltaTime;
        float noiseMovement = shakeTime * shakeSpeed;
        float xOffset = Mathf.PerlinNoise(currentSeed, noiseMovement) 
            * shakeMag - (shakeMag / 2);
        float yOffset = Mathf.PerlinNoise(currentSeed + 1000, noiseMovement) 
            * shakeMag - (shakeMag / 2);

        transform.position = basePos + new Vector3(xOffset, yOffset);
        if (shakeTime <= 0)
        {
            shaking = false;
            shakeTime = 0;
            transform.position = basePos;
        }
    }

    public TaskTree SlowMo(float duration)
    {
        return SlowMo(duration, Vector3.zero);
    }
    
    public TaskTree SlowMo(float duration, Vector3 location)
    {
        shaking = false;
        float startOrthographicSize = Camera.main.orthographicSize;
        
        ActionTask slow_down = new ActionTask(() =>
        {
            StartCoroutine(Coroutines.DoOverEasedTime(duration/2, Easing.QuadEaseOut,
                t =>
                {
                    transform.position = new Vector3(Mathf.Lerp(basePos.x, location.x, t), Mathf.Lerp(basePos.y, location.y, t), -10f);
                    Camera.main.orthographicSize = Mathf.Lerp(startOrthographicSize, 4, t);
                }));
        });
        
        Wait wait = new Wait(duration/2);
        
        ActionTask speed_up = new ActionTask(() =>
        {
            StartCoroutine(Coroutines.DoOverEasedTime(duration/2, Easing.QuintEaseIn,
                t =>
                {
                    transform.position = new Vector3(Mathf.Lerp(location.x, basePos.x, t), Mathf.Lerp(location.y, basePos.y, t), -10f);
                    Camera.main.orthographicSize = Mathf.Lerp(4, startOrthographicSize, t);
                }));
        });
        
        Wait wait2 = new Wait(duration/2);
        
        ActionTask reset = new ActionTask(() => { Camera.main.orthographicSize = startOrthographicSize; SetPosition(basePos); });

        TaskTree to_return = new TaskTree(slow_down, new TaskTree(wait, new TaskTree(speed_up, new TaskTree(wait2, new TaskTree(reset)))));

        return to_return;
    }
    
    public TaskTree SlowTimeScale(float duration)
    {
        slow_mo = true;
        
        ActionTask slow_down = new ActionTask(() =>
        {
            StartCoroutine(Coroutines.DoOverEasedTime(duration/2, Easing.QuintEaseOut,
                t =>
                {
                    Time.timeScale = Mathf.Lerp(1f, 0.1f, t);
                }));
        });
        
        Wait wait = new Wait(duration/2);
        
        ActionTask speed_up = new ActionTask(() =>
        {
            StartCoroutine(Coroutines.DoOverEasedTime(duration/2, Easing.QuintEaseIn,
                t =>
                {
                    Time.timeScale = Mathf.Lerp(0.1f, 1f, t);
                }));
        });
        
        Wait wait2 = new Wait(duration/2);
        
        ActionTask reset = new ActionTask(() => { Time.timeScale = 1f; slow_mo = false; });

        TaskTree to_return = new TaskTree(slow_down, new TaskTree(wait, new TaskTree(speed_up, new TaskTree(wait2, new TaskTree(reset)))));

        return to_return;
    }

    public float GetMapEdgeScreenHeight()
    {
        return theCamera.WorldToScreenPoint(new Vector3(0, 24, 0)).y;
    }
}

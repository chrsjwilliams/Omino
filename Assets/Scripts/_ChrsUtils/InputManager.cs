using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputManager
{
    private readonly string[] buttons = { "A", "B", "X", "Y" , "LB", "RB", "Start", "Back"};
    static private KeyCode[] validKeyCodes;

    private const float AXIS_THRESHOLD = 0.7f;

    private float leftTriggerAxis;
    private float rightTriggerAxis;

    private Vector2 leftStickAxis;
    private Vector2 rightStickAxis;
    private IntVector2 dPadAxis;
    private Touch[] lastFrameTouches;

    public InputManager()
    {
        if (validKeyCodes != null) return;
        validKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
    }

    //  BUG: Button Presses Register twice for both players
    public void GetInput()
    {
        if (Input.GetButtonDown("Reset")) Services.GameEventManager.Fire(new Reset());
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    Services.GameEventManager.Fire(new TouchDown(touch));
                    break;
                case TouchPhase.Moved:
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Ended:
                    if(lastFrameTouches[i].phase != TouchPhase.Ended)
                    {
                        Services.GameEventManager.Fire(new TouchUp(touch));
                    }
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    break;
            }
        }
    }

    public void Update()
    {
        GetInput();
        lastFrameTouches = Input.touches;
    }


}

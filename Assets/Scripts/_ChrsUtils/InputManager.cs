using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputManager
{
    private Touch[] lastFrameTouches;

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

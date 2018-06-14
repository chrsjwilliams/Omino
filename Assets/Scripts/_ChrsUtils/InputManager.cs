using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputManager
{
    private Touch[] lastFrameTouches;

    public void GetInput()
    {
        if (Input.GetButtonDown("Reset"))
        {
            //Services.GameEventManager.Fire(new Reset());
            Services.GameManager.Reset(new Reset());
        }
		for (int i = 0; i < Input.touches.Length; i++)
        {
			Touch touch = Input.touches [i];
            switch (touch.phase)
            {
			case TouchPhase.Began:
				Services.GameEventManager.Fire (new TouchDown (touch));
				break;
			case TouchPhase.Moved:
				Services.GameEventManager.Fire (new TouchMove (touch));
				break;
			case TouchPhase.Stationary:
                Services.GameEventManager.Fire(new TouchMove(touch));
                break;
			case TouchPhase.Ended:
				Touch touchLastFrame = GetLastFrameTouch (touch.fingerId);
				if (touchLastFrame.phase != TouchPhase.Ended) {
					Services.GameEventManager.Fire (new TouchUp (touch));
                    }
                    break;
			case TouchPhase.Canceled:
				break;
			default:
				break;
            }
        }
        if (Input.GetMouseButtonDown(0)) {
            Services.GameEventManager.Fire(new MouseDown(Input.mousePosition));
        }

        if (Input.GetMouseButton(0))
        {
            Services.GameEventManager.Fire(new MouseMove(Input.mousePosition));
        }

        if (Input.GetMouseButtonUp(0))
        {
            Services.GameEventManager.Fire(new MouseUp(Input.mousePosition));
        }
        //if (Services.UIManager != null)
        //	Services.UIManager.UpdateTouchCount (Input.touches);
    }

    public void Update()
    {
        GetInput();
        lastFrameTouches = Input.touches;
    }

	Touch GetLastFrameTouch(int id){
		for (int i = 0; i < lastFrameTouches.Length; i++) {
			if (lastFrameTouches [i].fingerId == id)
				return lastFrameTouches [i];
		}
		return new Touch ();
	}
}

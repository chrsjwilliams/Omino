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

    private IntVector2 leftStickAxis;
    private IntVector2 rightStickAxis;
    private IntVector2 dPadAxis;

    public InputManager()
    {
        if (validKeyCodes != null) return;
        validKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
    }

    public void GetInput()
    {
        for (int i = 0; i < Services.GameManager.NumPlayers; i++)
        {
            int playerNum = i + 1;
            foreach (string button in buttons)
            {
                if (Input.GetButtonDown(button + "_P" + playerNum))
                {
                    Debug.Log(button);
                    Services.GameEventManager.Fire(new ButtonPressed(button, playerNum));
                }
            }

            rightTriggerAxis = Input.GetAxis("RT_P" + playerNum);
            leftTriggerAxis = Input.GetAxis("LT_P" + playerNum);
            if (0 < rightTriggerAxis || 0 < leftTriggerAxis)
            {
                Services.GameEventManager.Fire(new TriggerAxisEvent(rightTriggerAxis, leftTriggerAxis,playerNum));
            }

            leftStickAxis = new IntVector2((int)Input.GetAxis("LeftStickX_P" + playerNum),
                                           (int)Input.GetAxis("LeftStickY_P" + playerNum));
            if (leftStickAxis.x != 0 || leftStickAxis.y != 0)
            {
                Services.GameEventManager.Fire(new LeftStickAxisEvent(leftStickAxis, playerNum));
            }

            rightStickAxis = new IntVector2((int)Input.GetAxis("RightStickX_P" + playerNum),
                                            (int)Input.GetAxis("RightStickY_P" + playerNum));
            if (rightStickAxis.x != 0 || rightStickAxis.y != 0)
            {
                Services.GameEventManager.Fire(new RightStickAxisEvent(rightStickAxis, playerNum));
            }

            dPadAxis = new IntVector2((int)Input.GetAxis("DPadX_P" + playerNum),
                                      (int)Input.GetAxis("DPadY_P" + playerNum));
            if (dPadAxis.x != 0 || dPadAxis.y != 0)
            {
                Services.GameEventManager.Fire(new DPadAxisEvent(dPadAxis, playerNum));
            }
        }

        if (Input.GetButtonDown("Reset")) Services.GameEventManager.Fire(new Reset());
    }

    private KeyCode FetchKey()
    {
        for (int i = 0; i < validKeyCodes.Length; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                return (KeyCode)i;
            }
        }

        return KeyCode.None;
    }

    public void Update()
    {
        Services.GameEventManager.Fire(new KeyPressedEvent(FetchKey()));
        GetInput();
    }
}

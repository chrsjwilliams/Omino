using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputManager
{
    private readonly string[] buttons = { "A", "B", "X", "Y" };
    static private KeyCode[] validKeyCodes;

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
                if (Input.GetButtonDown(button + "_P" + playerNum))
                    Services.GameEventManager.Fire(new ButtonPressed(button, playerNum));
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
    }
}

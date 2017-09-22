using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*--------------------------------------------------------------------------------------*/
/*																						*/
/*	GameEvent: Abstract class for Game Events for the GameEventsManager				    */
/*																						*/
/*--------------------------------------------------------------------------------------*/
public abstract class GameEvent 
{
   
}


public class ButtonPressed : GameEvent
{
    public string button;
    public int playerNum;
    public ButtonPressed(string _button, int _playerNum)
    {
        button = _button;
        playerNum = _playerNum;
    }
}

public class KeyPressedEvent : GameEvent
{
    public readonly KeyCode key;
    public KeyPressedEvent(KeyCode _key)
    {
        key = _key;
    }
}

public class Reset : GameEvent { }
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    private Stack<Menu> menuStack;
    public GameObject buttonPrefab;
    // Use this for initialization
    void Start()
    {
        menuStack = new Stack<Menu>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PushMenu(Menu menuToPush)
    {
        if(menuStack.Count > 0)
        {
            menuStack.Peek().Hide();
        }
        menuStack.Push(menuToPush);
        menuToPush.Load();
    }

    public void PopMenu()
    {
        Menu topMenu = menuStack.Pop();
        topMenu.Unload();
    }
}

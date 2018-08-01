using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private Stack<Menu> menuStack;
    private Queue<Menu> menusToPop;
    public GameObject buttonPrefab;
    public Menu firstMenu;
    public float buttonSpacing;
    public Transform menuHolder;
    public Button backButton;
    public TitleSceneScript titleScene;
    public Color[] uiColorScheme;
    public GameObject title;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMenuPopQueue();
    }

    public void Init()
    {
        Services.MenuManager = this;
        menuStack = new Stack<Menu>();
        menusToPop = new Queue<Menu>();
        backButton.gameObject.SetActive(false);
        PushMenu(firstMenu);
    }

    public void PushMenu(Menu menuToPush)
    {
        if (menuStack.Count > 0)
        {
            menuStack.Peek().Hide();
        }
        menuStack.Push(menuToPush);
        menuToPush.Load();
        if (menuStack.Count > 1) backButton.gameObject.SetActive(true);
    }

    public void PopMenu()
    {
        Menu topMenu = menuStack.Pop();
        menusToPop.Enqueue(topMenu);
        topMenu.state = Menu.MenuState.QueuedToPop;
    }

    public void LoadScene(TitleSceneScript.GameMode mode)
    {
        titleScene.StartGame(mode);
    }

    private void ProcessMenuPopQueue()
    {
        if(menusToPop.Count > 0)
        {
            Menu firstInLine = menusToPop.Peek();
            if (firstInLine.state == Menu.MenuState.QueuedToPop)
            {
                firstInLine.Hide();
                if(menusToPop.Count == 1)
                {
                    if (menuStack.Count > 0)
                    {
                        menuStack.Peek().Show();
                    }
                    if (menuStack.Count < 2)
                        backButton.gameObject.SetActive(false);
                }
            }
            else if(firstInLine.state == Menu.MenuState.Hidden)
            {
                menusToPop.Dequeue();
            }
        }
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private Stack<Menu> menuStack;
    public GameObject buttonPrefab;
    public Menu firstMenu;
    public float buttonSpacing;
    public Transform menuHolder;
    public Button backButton;
    public TitleSceneScript titleScene;
    public Color[] uiColorScheme;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Init()
    {
        Services.MenuManager = this;
        menuStack = new Stack<Menu>();
        backButton.gameObject.SetActive(false);
        PushMenu(firstMenu);
    }

    public void PushMenu(Menu menuToPush)
    {
        if(menuStack.Count > 0)
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
        topMenu.Unload();
        if (menuStack.Count > 0) menuStack.Peek().Show();
        if (menuStack.Count < 2) backButton.gameObject.SetActive(false);
    }

    public void LoadScene(TitleSceneScript.GameMode mode)
    {
        titleScene.StartGame(mode);
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private Stack<Menu> menuStack;
    private int menusToPop;
    public GameObject buttonPrefab;
    public GameObject sliderPrefab;
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
        ProcessMenuPopQueue();
        if (Input.GetKeyDown(KeyCode.B)) PopMenu();
    }

    public void OnReload()
    {
        Menu menuLoaded = menuStack.Peek();
        menuLoaded.Show();
    }

    public void Init()
    {
        Services.MenuManager = this;
        menuStack = new Stack<Menu>();
        menusToPop = 0;
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
        if (menuToPush.state == Menu.MenuState.NotLoaded)
            menuToPush.Load();
        else menuToPush.Show();
        if (menuStack.Count > 1) backButton.gameObject.SetActive(true);
    }

    public void PopMenu()
    {
        titleScene.versusIphoneText.SetActive(false);
        if (menuStack.Count > 1 + menusToPop)
        {
            menusToPop += 1;
        }
    }

    public void LoadScene(TitleSceneScript.GameMode mode)
    {
        if ((Services.GameManager.CurrentDevice != DEVICE.IPAD || Services.GameManager.pretendIphone) &&
            mode == TitleSceneScript.GameMode.TwoPlayers)
        {
            titleScene.versusIphoneText.SetActive(true);
            return;
        }
        titleScene.StartGame(mode);
    }

    private void ProcessMenuPopQueue()
    {
        if (menusToPop > 0)
        {
            Menu firstInLine = menuStack.Peek();
            if (firstInLine.state == Menu.MenuState.Active)
            {
                firstInLine.Hide();
                menuStack.Pop();
                menusToPop -= 1;
                if (menuStack.Count > 0)
                {
                    menuStack.Peek().Show();
                }
                if (menuStack.Count < 2)
                    backButton.gameObject.SetActive(false);
            }
        }
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }
}

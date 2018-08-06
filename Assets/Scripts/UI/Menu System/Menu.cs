using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Menu : MonoBehaviour
{
    private MenuObject[] menuObjects;
    public enum MenuState {
        NotLoaded, QueuedToPop, Popping, Active, Hidden, Pushing }
    public MenuState state;
    
    private const float slideStaggerTime = 0.05f;
    private float timeElapsed;
    private float timeToWait;

    // Use this for initialization
    void Awake()
    {
        state = MenuState.NotLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        if(timeToWait >= 0)
        {
            timeToWait -= Time.deltaTime;
            if(timeToWait <= 0)
            {
                switch (state)
                {
                    case MenuState.NotLoaded:
                        break;
                    case MenuState.QueuedToPop:
                        break;
                    case MenuState.Popping:
                        state = MenuState.Hidden;
                        break;
                    case MenuState.Active:
                        break;
                    case MenuState.Hidden:
                        break;
                    case MenuState.Pushing:
                        state = MenuState.Active;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void Load()
    {
        menuObjects = GetComponentsInChildren<MenuObject>();
        foreach (MenuObject menuObj in menuObjects)
        {

            menuObj.Load();
        }
        Show();
    }

    public void Unload()
    {
        Hide();
    }

    public void Show()
    {
        Services.GameManager.SetUnlockingData();
        state = MenuState.Pushing;
        for (int i = 0; i < menuObjects.Length; i++)
        {
            MenuObject menuObj = menuObjects[i];
            if (menuObj is MenuOption)
            {
                TitleSceneScript.GameMode mode = ((MenuOption)menuObj).modeToLoad;
                switch (mode)
                {
                    case TitleSceneScript.GameMode.Challenge:
                        menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                                        * Services.MenuManager.buttonSpacing * Vector2.up,
                                    (menuObjects.Length - 1 - i) * slideStaggerTime,
                                    Services.GameManager.ChallengeModeEnabled);
                        break;
                    case TitleSceneScript.GameMode.DungeonRun:
                        menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                                        * Services.MenuManager.buttonSpacing * Vector2.up,
                                    (menuObjects.Length - 1 - i) * slideStaggerTime,
                                    Services.GameManager.DungeonRunModeEnabled);
                        break;
                    case TitleSceneScript.GameMode.HyperSOLO:
                    case TitleSceneScript.GameMode.HyperVS:
                        menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                                        * Services.MenuManager.buttonSpacing * Vector2.up,
                                    (menuObjects.Length - 1 - i) * slideStaggerTime,
                                    Services.GameManager.HyperModeEnabled);
                        break;
                    case TitleSceneScript.GameMode.Practice:
                        menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                                        * Services.MenuManager.buttonSpacing * Vector2.up,
                                    (menuObjects.Length - 1 - i) * slideStaggerTime,
                                    Services.GameManager.PracticeModeEnabled);
                        break;
                    case TitleSceneScript.GameMode.TwoPlayers:
                        menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                                        * Services.MenuManager.buttonSpacing * Vector2.up,
                                    (menuObjects.Length - 1 - i) * slideStaggerTime,
                                    Services.GameManager.TwoPlayerModeEnabled);
                        break;
                    default:
                        menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                                        * Services.MenuManager.buttonSpacing * Vector2.up,
                                    (menuObjects.Length - 1 - i) * slideStaggerTime);
                        break;
                }

            }
            else
            {

                menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                    * Services.MenuManager.buttonSpacing * Vector2.up,
                    (menuObjects.Length - 1 - i) * slideStaggerTime);
            }
        }
        timeToWait = slideStaggerTime * menuObjects.Length 
            + MenuObject.movementTime;
    }

    public void Hide()
    {
        state = MenuState.Popping;
        for (int i = 0; i < menuObjects.Length; i++)
        {
            MenuObject menuObj = menuObjects[i];
            menuObj.Hide((menuObjects.Length - 1 - i) * slideStaggerTime);
        }
        timeToWait = slideStaggerTime * menuObjects.Length
            + MenuObject.movementTime;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Menu : MonoBehaviour
{
    private MenuObject[] menuObjects;
    
    private const float slideStaggerTime = 0.05f;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

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
        for (int i = 0; i < menuObjects.Length; i++)
        {
            MenuObject menuObj = menuObjects[i];
            menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                * Services.MenuManager.buttonSpacing * Vector2.up,
                (menuObjects.Length - 1 - i) * slideStaggerTime);
        }
    }

    public void Hide()
    {
        for (int i = 0; i < menuObjects.Length; i++)
        {
            MenuObject menuObj = menuObjects[i];
            menuObj.Hide((menuObjects.Length - 1 - i) * slideStaggerTime);
        }
    }
}

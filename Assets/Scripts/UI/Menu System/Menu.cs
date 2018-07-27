using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Menu : MonoBehaviour
{
    private MenuOption[] options;

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
        options = GetComponentsInChildren<MenuOption>();
        foreach (MenuOption option in options)
        {
            option.Load();
        }
        Show();
    }

    public void Unload()
    {
        Hide();
    }

    public void Show()
    {
        for (int i = 0; i < options.Length; i++)
        {
            MenuOption option = options[i];
            option.Show(i * Services.MenuManager.buttonSpacing * Vector2.up,
                (options.Length - 1 - i) *slideStaggerTime);
        }
    }

    public void Hide()
    {
        for (int i = 0; i < options.Length; i++)
        {
            MenuOption option = options[i];
            option.Hide((options.Length - 1 - i) * slideStaggerTime);
        }
    }
}

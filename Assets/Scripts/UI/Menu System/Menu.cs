using UnityEngine;


public class Menu : MonoBehaviour
{
    private MenuObject[] menuObjects;
    public enum MenuState {
        NotLoaded, Popping, Active, Hidden, Pushing }
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
        for (int i = 0; i < menuObjects.Length; i++)
        {
            menuObjects[i].Unload();
        }
    }

    public void Show()
    {
        state = MenuState.Pushing;
        for (int i = 0; i < menuObjects.Length; i++)
        {
            MenuObject menuObj = menuObjects[i];
            menuObj.Show((i - (menuObjects.Length / 2f) + 0.5f)
                * Services.MenuManager.buttonSpacing * Vector2.up,
                (menuObjects.Length - 1 - i) * slideStaggerTime);
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

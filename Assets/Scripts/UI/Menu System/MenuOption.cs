using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class MenuOption : MonoBehaviour
{
    public Menu menuToLoad;
    public UnityEvent actionToPerform;
    public string buttonText;
    private GameObject button;
    private TextMeshProUGUI uiText;
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
        GameObject button = Instantiate(Services.MenuManager.buttonPrefab, Services.MenuManager.transform);
        transform.SetParent(button.transform);
        uiText = button.GetComponent<TextMeshProUGUI>();
        uiText.text = buttonText;
    }

    public void OnPress()
    {
        if (menuToLoad != null)
        {
            Services.MenuManager.PushMenu(menuToLoad);
        }
        else
        {
            actionToPerform.Invoke();
        }
    }
}

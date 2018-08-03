using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MenuOption : MenuObject
{
    public UnityEvent onLoadActions;
    public Menu menuToLoad;
    public UnityEvent actionToPerform;
    public TitleSceneScript.GameMode modeToLoad;
    public string buttonText;
    private TextMeshProUGUI uiText;
    private RectTransform buttonRect;
    [HideInInspector]
    public bool toggled;

    // Update is called once per frame
    void Update()
    {

    }

    public override void Load()
    {
        objectPrefabToSpawn = Services.MenuManager.buttonPrefab;
        base.Load();
        uiText = associatedObject.GetComponentInChildren<TextMeshProUGUI>();
        uiText.text = buttonText;
        Button buttonComponent = associatedObject.GetComponent<Button>();
        buttonComponent.onClick.AddListener(OnPress);
        onLoadActions.Invoke();
    }

    public override void Show(Vector2 pos, float delay)
    {
        associatedObject.GetComponent<Button>().enabled = true;
        base.Show(pos, delay);
    }

    public override void Hide(float delay)
    {
        associatedObject.GetComponent<Button>().enabled = false;
        base.Hide(delay);
    }

    public void OnPress()
    {
        if (menuToLoad != null)
        {
            Services.MenuManager.PushMenu(menuToLoad);
        }
        if(modeToLoad != TitleSceneScript.GameMode.NONE)
        {
            Services.MenuManager.LoadScene(modeToLoad);
        }
        else
        {
            actionToPerform.Invoke();
        }
        Services.MenuManager.UIButtonPressedSound();
    }

    public void ToggleStrikethrough()
    {
        SetStrikethrough(!toggled);
    }

    private void SetStrikethrough(bool status)
    {
        toggled = status;
        associatedObject.GetComponent<Image>().color = status ?
            Services.GameManager.Player2ColorScheme[0] :
            Services.GameManager.Player2ColorScheme[1];
        TextMeshProUGUI textMesh = associatedObject.GetComponentInChildren<TextMeshProUGUI>(true);
        string textContent = textMesh.text;
        string[] textSplit = textContent.Split('<', '>');
        if (textSplit.Length > 1)
        {
            for (int i = 0; i < textSplit.Length; i++)
            {
                if (textSplit[i] == "s")
                {
                    textContent = textSplit[i + 1];
                    break;
                }
            }
        }
        if (!status)
        {
            textContent = "<s>" + textContent + "</s>";
        }

        textMesh.text = textContent;
    }

    private void SetColor(Color color)
    {
        associatedObject.GetComponent<Image>().color = color;
    }

    public void GetBlueprintAssistStatus()
    {
        SetStrikethrough(Services.GameManager.BlueprintAssistEnabled);
    }

    public void GetSoundFxStatus()
    {
        SetStrikethrough(Services.GameManager.SoundEffectsEnabled);
    }

    public void GetMusicStatus()
    {
        SetStrikethrough(Services.GameManager.MusicEnabled);
    }

    public void GetTutorialCompletionStatus()
    {
        int progress = 0;
        if (File.Exists(GameOptionsSceneScript.progressFileName))
        {
            string fileText = File.ReadAllText(GameOptionsSceneScript.progressFileName);
            int.TryParse(fileText, out progress);
        }
        if (progress == 5)
        {
            SetColor(Services.MenuManager.uiColorScheme[0]);
        }
        else
        {
            SetColor(Services.MenuManager.uiColorScheme[1]);
        }
    }
}

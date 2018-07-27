using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MenuOption : MonoBehaviour
{
    public UnityEvent onLoadActions;
    public Menu menuToLoad;
    public UnityEvent actionToPerform;
    public TitleSceneScript.GameMode modeToLoad;
    public string buttonText;
    private GameObject button;
    private TextMeshProUGUI uiText;
    private RectTransform buttonRect;
    [HideInInspector]
    public bool toggled;

    private Vector2 target;
    private Vector2 startPoint;
    private float timeElapsed;
    private const float movementTime = 0.3f;
    private const float offset = 1000;
    private float delayTimeRemaining;
    private bool moving;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (delayTimeRemaining > 0) DecayDelay();
        if (moving) Move();
    }

    private void DecayDelay()
    {
        delayTimeRemaining -= Time.deltaTime;
        if (delayTimeRemaining <= 0)
        {
            StartMovement();
        }

    }

    private void Move()
    {
        timeElapsed += Time.deltaTime;

        buttonRect.anchoredPosition = Vector2.Lerp(startPoint, target,
            EasingEquations.Easing.QuadEaseOut(timeElapsed / movementTime));
        if(timeElapsed >= movementTime)
        {
            buttonRect.anchoredPosition = target;
            moving = false;
        }

    }

    private void StartMovement()
    {
        timeElapsed = 0;
        moving = true;
    }

    public void Load()
    {
        button = Instantiate(Services.MenuManager.buttonPrefab, Services.MenuManager.menuHolder);
        button.transform.localPosition = Vector3.zero;
        uiText = button.GetComponentInChildren<TextMeshProUGUI>();
        uiText.text = buttonText;
        Button buttonComponent = button.GetComponent<Button>();
        buttonComponent.onClick.AddListener(OnPress);
        buttonRect = button.GetComponent<RectTransform>();
        onLoadActions.Invoke();
    }

    public void Unload()
    {
        Destroy(button);
    }

    public void Show(Vector2 pos, float delay)
    {
        button.SetActive(true);
        button.GetComponent<Button>().enabled = true;
        target = pos;
        startPoint = pos + (offset * Vector2.up);
        buttonRect.anchoredPosition = startPoint;
        delayTimeRemaining = delay + float.Epsilon;
    }

    public void Hide(float delay)
    {
        button.GetComponent<Button>().enabled = false;
        startPoint = buttonRect.anchoredPosition;
        target = startPoint + (offset * Vector2.down);
        delayTimeRemaining = delay  +float.Epsilon;
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
    }

    public void ToggleStrikethrough()
    {
        SetStrikethrough(!toggled);
    }

    private void SetStrikethrough(bool status)
    {
        toggled = status;
        button.GetComponent<Image>().color = status ?
            Services.GameManager.Player2ColorScheme[0] :
            Services.GameManager.Player2ColorScheme[1];
        TextMeshProUGUI textMesh = button.GetComponentInChildren<TextMeshProUGUI>(true);
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
        button.GetComponent<Image>().color = color;
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
        if (progress == 4)
        {
            SetColor(Services.MenuManager.uiColorScheme[0]);
        }
        else
        {
            SetColor(Services.MenuManager.uiColorScheme[1]);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionMenuSceneScript : Scene<TransitionData>
{
    [SerializeField]
    private Button neonButton;
    [SerializeField]
    private Button musicButton;
    [SerializeField]
    private Button soundFXButton;
    [SerializeField]
    private Button blueprintAssistButton;

    [SerializeField]
    private GameObject optionMenu;

    [SerializeField]
    private GameObject backButton;

    [SerializeField]
    private HandicapSystem handicapSystem;

    private TaskManager _tm = new TaskManager();

    // Use this for initialization
    void Start()
    {

    }

    internal override void OnEnter(TransitionData data)
    {
        optionMenu.SetActive(false);
        backButton.SetActive(false);

        switch (Services.GameManager.mode)
        {
            case TitleSceneScript.GameMode.TwoPlayers:
            case TitleSceneScript.GameMode.Practice:
            case TitleSceneScript.GameMode.Demo:
                handicapSystem.gameObject.SetActive(true);
                break;
            default:
                handicapSystem.gameObject.SetActive(false);
                break;
        }


        TaskTree optionMenuEntrance = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(optionMenu)),
                new TaskTree(new LevelSelectTextEntrance(backButton, true)));

        _tm.Do(optionMenuEntrance);
        SetOptionButtonStatus(neonButton, Services.GameManager.NeonEnabled);
        SetOptionButtonStatus(blueprintAssistButton, Services.GameManager.BlueprintAssistEnabled);
        SetOptionButtonStatus(musicButton, Services.GameManager.MusicEnabled);
        SetOptionButtonStatus(soundFXButton, Services.GameManager.SoundEffectsEnabled);

        handicapSystem.UpdateHandicapText();
    }

    internal override void OnExit()
    {
    }

    internal override void ExitTransition()
    {
        TaskTree optionMenuExit = new TaskTree(new EmptyTask(),
                new TaskTree(new LevelSelectTextEntrance(optionMenu, false, true)),
                new TaskTree(new LevelSelectTextEntrance(backButton, true, true)));

        _tm.Do(optionMenuExit);
    }

    private void TurnOnHandicapOptions(bool isOn)
    {
        handicapSystem.gameObject.SetActive(isOn);
    }
    public void UIClick()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 0.55f);
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }

    private void SetOptionButtonStatus(Button button, bool status)
    {
        button.GetComponent<Image>().color = status ?
            Services.GameManager.Player2ColorScheme[0] :
            Services.GameManager.Player2ColorScheme[1];
        TextMeshProUGUI textMesh = button.GetComponentInChildren<TextMeshProUGUI>();
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

    public void ToggleNeon()
    {
        Services.GameManager.ToggleNeon();
        SetOptionButtonStatus(neonButton, Services.GameManager.NeonEnabled);
    }

    public void ToggleBlueprintAssist()
    {
        Services.GameManager.ToggleBlueprintAssist();
        SetOptionButtonStatus(blueprintAssistButton, Services.GameManager.BlueprintAssistEnabled);
    }

    public void ToggleMusic()
    {
        Services.AudioManager.ToggleMusic();
        SetOptionButtonStatus(musicButton, Services.GameManager.MusicEnabled);
    }

    public void ToggleSoundFX()
    {
        Services.AudioManager.ToggleSoundEffects();
        SetOptionButtonStatus(soundFXButton, Services.GameManager.SoundEffectsEnabled);
    }

    // Update is called once per frame
    void Update()
    {
        _tm.Update();
    }
}

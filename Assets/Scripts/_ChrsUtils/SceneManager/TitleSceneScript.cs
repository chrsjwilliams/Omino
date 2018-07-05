using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;
    [SerializeField]
    private Button[] modeButtons;
    [SerializeField]
    private Button[] optionButtons;
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button optionsButton;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private Button musicButton;
    [SerializeField]
    private Button soundFXButton;
    [SerializeField]
    private Button blueprintAssistButton;
    [SerializeField]
    private GameObject title;

    public enum GameMode { TwoPlayers, PlayerVsAI, Demo, Campaign, DungeonRun }

    private const float SECONDS_TO_WAIT = 0.01f;

    private TaskManager _tm = new TaskManager();

    internal override void OnEnter(TransitionData data)
    {
        foreach(Button button in modeButtons)
        {
            button.gameObject.SetActive(false);
        }
        foreach(Button button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }
        backButton.gameObject.SetActive(false);

        SetOptionButtonStatus(optionsButton, true);
        SetOptionButtonStatus(musicButton, Services.GameManager.MusicEnabled);
        SetOptionButtonStatus(soundFXButton, Services.GameManager.SoundEffectsEnabled);
        SetOptionButtonStatus(blueprintAssistButton, Services.GameManager.BlueprintAssistEnabled);

        Services.GameManager.eloTrackingMode = false;
    }

    public void StartGame(GameMode mode)
    {
        Services.GameManager.mode = mode;
        Task start = new SlideOutTitleScreenButtons(modeButtons);
        start.Then(new ActionTask(ChangeScene));

        _tm.Do(start);
    }

    public void PlayPlayerVsAI()
    {
        StartGame(GameMode.PlayerVsAI);
    }

    public void Play2Players()
    {
        StartGame(GameMode.TwoPlayers);
    }

    public void PlayCampaignMode()
    {
        StartGame(GameMode.Campaign);
    }

    public void PlayDungeonRunMode()
    {
        StartGame(GameMode.DungeonRun);
    }

    public void DemoMode()
    {
        StartGame(GameMode.Demo);
    }

    public void PlayChallengeMode()
    {
        Services.GameManager.eloTrackingMode = true;
        StartGame(GameMode.PlayerVsAI);
    }

    private void ChangeScene()
    {
        Services.Scenes.Swap<GameOptionsSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
    }

    public void OnPlayHit()
    {
        playButton.gameObject.SetActive(false);
        optionsButton.enabled = false;
        _tm.Do(new SlideInTitleScreenButtons(modeButtons, playButton.transform.localPosition, 
            title, optionsButton));
        backButton.gameObject.SetActive(true);
    }

    public void OnOptionsHit()
    {
        optionsButton.gameObject.SetActive(false);
        playButton.enabled = false;
        _tm.Do(new SlideInTitleScreenButtons(optionButtons, optionsButton.transform.localPosition,
            title, playButton));
        backButton.gameObject.SetActive(true);
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed, 0.55f);
    }
    
    public void UIClick()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIClick, 0.55f);
    }

    public void Back()
    {
        Services.AudioManager.PlaySoundEffect(Services.Clips.UIButtonPressed);
        Services.Scenes.Swap<TitleSceneScript>();
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

    public void ToggleBlueprintAssist()
    {
        Services.GameManager.ToggleBlueprintAssist();
        SetOptionButtonStatus(blueprintAssistButton, Services.GameManager.BlueprintAssistEnabled);
    }
}

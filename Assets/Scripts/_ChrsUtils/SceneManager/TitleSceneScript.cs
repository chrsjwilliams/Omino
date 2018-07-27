using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class TitleSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;
    [SerializeField]
    private Button[] modeButtons;
    [SerializeField]
    private Button[] optionButtons;
    [SerializeField]
    private Button[] playerCountButtons;
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
    [SerializeField]
    private MenuManager menuManager;

    public enum GameMode { NONE, TwoPlayers, Practice, Demo, Tutorial, DungeonRun, Elo }

    private const float SECONDS_TO_WAIT = 0.01f;

    private TaskManager _tm = new TaskManager();

    internal override void Init()
    {

        //foreach (Button button in modeButtons)
        //{
        //    button.gameObject.SetActive(false);
        //}
        //foreach (Button button in optionButtons)
        //{
        //    button.gameObject.SetActive(false);
        //}
        //foreach (Button button in playerCountButtons)
        //{
        //    button.gameObject.SetActive(false);
        //}
        //backButton.gameObject.SetActive(false);

        //switch (Services.GameManager.mode)
        //{
        //    case GameMode.TwoPlayers:
        //        foreach (Button button in playerCountButtons)
        //        {
        //            button.gameObject.SetActive(true);
        //        }
        //        backButton.gameObject.SetActive(true);

        //        break;
        //    case GameMode.DungeonRun:
        //    case GameMode.Elo:
        //    case GameMode.Practice:
        //    case GameMode.Tutorial:
        //        foreach (Button button in modeButtons)
        //        {
        //            button.gameObject.SetActive(true);
        //        }
        //        backButton.gameObject.SetActive(true);

        //        break;
        //    default:
        //        break;
        //}
        //Services.GameManager.mode = GameMode.NONE;



        //SetOptionButtonStatus(optionsButton, true);
        //SetOptionButtonStatus(musicButton, Services.GameManager.MusicEnabled);
        //SetOptionButtonStatus(soundFXButton, Services.GameManager.SoundEffectsEnabled);
        //SetOptionButtonStatus(blueprintAssistButton, Services.GameManager.BlueprintAssistEnabled);

        //int progress = 0;
        //if (File.Exists(GameOptionsSceneScript.progressFileName))
        //{
        //    string fileText = File.ReadAllText(GameOptionsSceneScript.progressFileName);
        //    int.TryParse(fileText, out progress);
        //}
        //if(progress == 4)
        //{
        //    modeButtons[0].GetComponent<Image>().color = uiColorScheme[0];
        //}
        //else
        //{
        //    modeButtons[0].GetComponent<Image>().color = uiColorScheme[1];
        //}
        menuManager.Init();
    }

    internal override void OnEnter(TransitionData data)
    {


    }

    internal override void OnExit()
    {
        
    }

    public void StartGame(GameMode mode)
    {
        Services.GameManager.mode = mode;
        Services.Analytics.MatchStarted(mode);
        //Task start;
        switch (mode)
        {
            case GameMode.TwoPlayers:
                Services.Scenes.PushScene<MapSelectSceneScript>();
                break;
            case GameMode.Practice:
                Services.Scenes.PushScene<AIDifficultySceneScript>();
                break;
            case GameMode.Demo:
                break;
            case GameMode.Tutorial:
                Services.Scenes.PushScene<TutorialLevelSceneScript>();
                break;
            case GameMode.DungeonRun:
                Services.Scenes.PushScene<DungeonRunSceneScript>();
                break;
            case GameMode.Elo:
                Services.Scenes.PushScene<EloSceneScript>();
                break;
            case GameMode.NONE:
                break;
            default:
                break;
        }
        //if (mode == GameMode.TwoPlayers)
        //{
        //    start = new SlideOutTitleScreenButtons(playerCountButtons);
        //}
        //else
        //{
        //    start = new SlideOutTitleScreenButtons(modeButtons);
        //}
        //start.Then(new ActionTask(ChangeScene));

        //_tm.Do(start);
    }

    public void PlayPractice()
    {
        //Services.GameManager.mode = GameMode.Practice;
        //Services.Scenes.PushScene<AIDifficultySceneScript>();
        StartGame(GameMode.Practice);

    }

    public void Play2Players()
    {
        //Services.GameManager.mode = GameMode.TwoPlayers;
        //Services.Scenes.PushScene<MapSelectSceneScript>();
        StartGame(GameMode.TwoPlayers);
    }

    public void PlayTutorialMode()
    {
        //Services.GameManager.mode = GameMode.Tutorial;
        //Services.Scenes.PushScene<TutorialLevelSceneScript>();
        StartGame(GameMode.Tutorial);
    }

    public void PlayDungeonRunMode()
    {
        //Services.GameManager.mode = GameMode.Tutorial;
        //Services.Scenes.PushScene<DungeonRunSceneScript>();
        StartGame(GameMode.DungeonRun);
    }

    public void DemoMode()
    {
        StartGame(GameMode.Demo);
    }

    public void PlayChallengeMode()
    {
        //Services.GameManager.mode = GameMode.Elo;
        //Services.Scenes.PushScene<EloSceneScript>();
        StartGame(GameMode.Elo);
    }

    private void ChangeScene()
    {
        Services.Scenes.Swap<GameOptionsSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
    }

    public void OnSinglePlayerButtonSelected()
    {
        _tm.Do(new SlideInTitleScreenButtons(modeButtons, playerCountButtons[0].transform.localPosition,
            title, playerCountButtons[1]));
        playerCountButtons[1].enabled = false;
        playerCountButtons[0].gameObject.SetActive(false);
    }

    public void OnPlayHit()
    {
        playButton.gameObject.SetActive(false);
        optionsButton.enabled = false;
        _tm.Do(new SlideInTitleScreenButtons(playerCountButtons, playButton.transform.localPosition, 
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

    public void ToggleMusic()
    {
        Services.AudioManager.ToggleMusic();
        //SetOptionButtonStatus(musicButton, Services.GameManager.MusicEnabled);
    }

    public void ToggleSoundFX()
    {
        Services.AudioManager.ToggleSoundEffects();
        //SetOptionButtonStatus(soundFXButton, Services.GameManager.SoundEffectsEnabled);
    }

    public void ToggleBlueprintAssist()
    {
        Services.GameManager.ToggleBlueprintAssist();
        //SetOptionButtonStatus(blueprintAssistButton, Services.GameManager.BlueprintAssistEnabled);
    }
}

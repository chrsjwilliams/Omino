using UnityEngine;
using UnityEngine.UI;

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
    private GameObject title;

    public enum GameMode { TwoPlayers, PlayerVsAI, Demo, Campaign }

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
        if (Services.NetData != null)
        {
            if (Services.NetData.GetGameOverMessage() != "")
            {
                Services.Scenes.PushScene<NetworkGameOverMessageScreen>();
            }
        }
    }

    public void StartGame(GameMode mode)
    {
        Services.GameManager.mode = mode;
        Task start = new SlideOutTitleScreenButtons(modeButtons);
        start.Then(new ActionTask(ChangeScene));

        _tm.Do(start);
    }

    private void StartNetworkedMode()
    {
        Services.Scenes.PushScene<NetworkJoinSceneScript>();
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

    public void DemoMode()
    {
        StartGame(GameMode.Demo);
    }

    private void ChangeScene()
    {
        Services.Scenes.Swap<GameOptionsSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
        if (Input.GetKey(KeyCode.N))  StartNetworkedMode();
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
        Services.AudioManager.CreateTempAudio(Services.Clips.UIButtonPressed, 0.55f);
    }
    
    public void UIClick()
    {
        Services.AudioManager.CreateTempAudio(Services.Clips.UIClick, 0.55f);
    }

    public void Back()
    {
        Services.Scenes.Swap<TitleSceneScript>();
    }
}

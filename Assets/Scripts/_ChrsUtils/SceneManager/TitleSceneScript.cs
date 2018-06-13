using UnityEngine;
using UnityEngine.UI;

public class TitleSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;
    [SerializeField]
    private Button[] buttons;
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private GameObject title;

    public enum GameMode { TwoPlayers, PlayerVsAI, Demo, Campaign }

    private const float SECONDS_TO_WAIT = 0.01f;

    private TaskManager _tm = new TaskManager();

    internal override void OnEnter(TransitionData data)
    {
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<ButtonPressed>(OnButtonPressed);
        foreach(Button button in buttons)
        {
            button.gameObject.SetActive(false);
        }

        if (Services.NetData != null)
        {
            if (Services.NetData.GetGameOverMessage() != "")
            {
                Services.Scenes.PushScene<NetworkGameOverMessageScreen>();
            }
        }
    }

    internal override void OnExit()
    {
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<ButtonPressed>(OnButtonPressed);
    }

    private void OnTouchDown(TouchDown e)
    {
        //StartGame();
    }

    private void OnButtonPressed(ButtonPressed e)
    {
        if((e.button == "Start" || e.button == "A"))
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        Task start = new SlideOutTitleScreenButtons(buttons);
        start.Then(new ActionTask(ChangeScene));

        _tm.Do(start);
    }

    private void StartNetworkedMode()
    {
        Services.Scenes.PushScene<NetworkJoinSceneScript>();
    }

    public void Play()
    {
        StartGame();
    }

    public void PlayPlayerVsAI()
    {
        Services.GameManager.mode = GameMode.PlayerVsAI;
        StartGame();
    }

    public void Play2Players()
    {
        Services.GameManager.mode = GameMode.TwoPlayers;
        StartGame();
    }

    public void PlayCampaignMode()
    {
        Services.GameManager.mode = GameMode.Campaign;
        StartGame();
    }

    public void DemoMode()
    {
        Services.GameManager.mode = GameMode.Demo;
        StartGame();
    }

    private void ChangeScene()
    {
        Services.Scenes.Swap<GameOptionsSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
        //if (Input.GetMouseButtonDown(0)) StartGame();
        if (Input.GetKey(KeyCode.N))  StartNetworkedMode();
    }

    public void OnPlayHit()
    {
        playButton.gameObject.SetActive(false);
        _tm.Do(
            new SlideInTitleScreenButtons(buttons, playButton.transform.localPosition, title));
    }

    public void UIButtonPressedSound()
    {
        Services.AudioManager.CreateTempAudio(Services.Clips.UIButtonPressed, 0.55f);
    }
    
    public void UIClick()
    {
        Services.AudioManager.CreateTempAudio(Services.Clips.UIClick, 0.55f);
    }
}

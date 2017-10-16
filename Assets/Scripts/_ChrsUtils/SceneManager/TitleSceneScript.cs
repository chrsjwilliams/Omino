using UnityEngine;

public class TitleSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;

    private const float SECONDS_TO_WAIT = 0.01f;

    private TaskManager _tm = new TaskManager();

    internal override void OnEnter(TransitionData data)
    {
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
        Services.GameEventManager.Register<ButtonPressed>(OnButtonPressed);
    }

    internal override void OnExit()
    {
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<ButtonPressed>(OnButtonPressed);
    }

    private void OnTouchDown(TouchDown e)
    {
        StartGame();
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
        _tm.Do
        (
                    new Wait(SECONDS_TO_WAIT))
              .Then(new ActionTask(ChangeScene)
        );
    }

    private void ChangeScene()
    {
        Services.Scenes.Swap<GameSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
        if (Input.GetMouseButtonDown(0)) StartGame();
    }
}

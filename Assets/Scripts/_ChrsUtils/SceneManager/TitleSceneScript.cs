using UnityEngine;

public class TitleSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;

    private const float SECONDS_TO_WAIT = 1.0f;

    private TaskManager _tm = new TaskManager();

    internal override void OnEnter(TransitionData data)
    {
        Services.GameEventManager.Register<KeyPressedEvent>(OnKeyPressed);
    }

    internal override void OnExit()
    {
        Services.GameEventManager.Unregister<KeyPressedEvent>(OnKeyPressed);
    }

    private void OnKeyPressed(KeyPressedEvent e)
    {
        if(e.key == startGame)
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
    }
}

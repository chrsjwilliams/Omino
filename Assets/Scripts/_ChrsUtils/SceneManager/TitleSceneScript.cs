using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleSceneScript : Scene<TransitionData>
{
    public KeyCode startGame = KeyCode.Space;
    [SerializeField]
    private MenuManager menuManager;

    public enum GameMode {  NONE, TwoPlayers, Practice,
                            Demo, Tutorial, DungeonRun,
                            Challenge, HyperSOLO, HyperVS,
                            Shop, Reference, Edit}

    public static GameMode[] unlockableModes = new GameMode[] {
        GameMode.Challenge, GameMode.DungeonRun, GameMode.HyperSOLO,
        GameMode.HyperVS, GameMode.Practice };

    private const float SECONDS_TO_WAIT = 0.01f;

    private TaskManager _tm = new TaskManager();

    internal override void Init()
    {
        menuManager.Init();
    }

    internal override void OnEnter(TransitionData data)
    {
        menuManager.OnReload();
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
            case GameMode.HyperVS:
                Services.Scenes.PushScene<LevelSelectSceneScript>();
                break;
            case GameMode.Practice:
            case GameMode.HyperSOLO:
                Services.Scenes.PushScene<AIDifficultySceneScript>();
                break;
            case GameMode.Demo:
                Services.Scenes.PushScene<LevelSelectSceneScript>();
                break;
            case GameMode.Tutorial:
                Services.Scenes.PushScene<TutorialLevelSceneScript>();
                break;
            case GameMode.DungeonRun:
                if (DungeonRunManager.dungeonRunData.selectingNewTech)
                {
                    Services.Scenes.PushScene<TechSelectSceneScript>();
                }
                else
                {
                    Services.Scenes.PushScene<DungeonRunSceneScript>();
                }
                break;
            case GameMode.Challenge:
                Services.Scenes.PushScene<EloSceneScript>();
                break;
            case GameMode.Shop:
                Services.Scenes.PushScene<InGameShopSceneScript>();
                break;
            case GameMode.Reference:
                Services.Scenes.PushScene<ReferenceSceneScript>();
                break;
            case GameMode.Edit:
                Services.Scenes.PushScene<LevelSelectSceneScript>();
                break;
            case GameMode.NONE:
                break;
            default:
                break;
        }
       
    }

    private void ChangeScene()
    {
        Services.Scenes.Swap<GameOptionsSceneScript>();
    }

    private void Update()
    {
        _tm.Update();
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

    public void ToggleNeon()
    {
        Services.GameManager.ToggleNeon();
        //SetOptionButtonStatus(neonButton, Services.GameManager.NeonEnabled);
    }
}

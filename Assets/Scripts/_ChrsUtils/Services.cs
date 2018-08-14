
using BeatManagement;
using Tinylytics;

public class Services
{
    public static GameManager GameManager { get; set; }
    public static GameEventsManager GameEventManager { get; set; }
    public static GameEventsManager GlobalEventManager { get; set; }
    public static TaskManager GeneralTaskManager { get; set; }
    public static PrefabDB Prefabs { get; set; }
    public static CameraController CameraController { get; set; }
    public static EloRankData EloRankData { get; set; }

    public static InputManager InputManager { get; set; }

    public static GameSceneManager<TransitionData> Scenes { get; set; }

    public static MapManager MapManager { get; set; }

    public static GameSceneScript GameScene { get; set; }
    public static UIManager UIManager { get; set; }
    public static TutorialManager TutorialManager { get; set; }

    public static AudioManager AudioManager { get; set; }
    public static ClipLibrary Clips { get; set; }
    public static GameData GameData { get; set; }
    public static Clock Clock { get; set; }

    public static TechDataLibrary TechDataLibrary { get; set; }
    
    public static AnalyticsManager Analytics { get; set; }

    //title scene
    public static MenuManager MenuManager { get; set; }
}

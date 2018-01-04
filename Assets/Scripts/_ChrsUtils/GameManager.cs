using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string PLAYER = "Player";

    public readonly int MAX_PLAYERS = 2;
    public readonly int MIN_PLAYERS = 0;

    [SerializeField] private int _numPlayers;
    public int NumPlayers
    {
        get { return _numPlayers; }
        private set
        {
            if (_numPlayers <= 0)
            {
                _numPlayers = 1;
            }
            else
            {
                _numPlayers = value;
            }
        }
    }

    [SerializeField] private Camera _mainCamera;
    public Camera MainCamera
    {
        get { return _mainCamera; }
    }

    [SerializeField] private Player[] _players;
    public Player[] Players
    {
        get { return _players; }
    }

    [SerializeField] private Color[] _player1ColorScheme;
    public Color[] Player1ColorScheme
    {
        get { return _player1ColorScheme; }
    }

    [SerializeField] private Color[] _player2ColorScheme;
    public Color[] Player2ColorScheme
    {
        get { return _player2ColorScheme; }
    }

    [SerializeField] private Color[] _mapColorScheme;
    public Color[] MapColorScheme
    {
        get { return _mapColorScheme; }
    }

    [SerializeField]
    private Color superDestructorResourceColor;
    public Color NeutralColor { get { return superDestructorResourceColor; } }

    [SerializeField]
    public bool turnBasedVersion { get; private set; }
    [SerializeField]
    public bool usingStructures { get; private set; }
    [SerializeField]
    public bool usingMiniBases { get; private set; }
    [SerializeField]
    public bool usingBlueprints { get; private set; }

    private void Awake()
    {
        Services.GameEventManager.Register<Reset>(Reset);
        Input.simulateMouseWithTouches = false;
    }

    public void SetUserPreferences(bool isTurnBased, bool useStructures, bool useMiniBases, bool useBlueprints)
    {
        turnBasedVersion = isTurnBased;
        usingStructures = useStructures;
        usingMiniBases = useMiniBases;
        usingBlueprints = useBlueprints;
    }

    public void Init()
    {
        _mainCamera = Camera.main;
    }

    public void SetNumPlayers(int players)
    {
        _numPlayers = players;
        _players = new Player[MAX_PLAYERS];
    }

    public void InitPlayers()
    {
        int numAIPlayers = MAX_PLAYERS - NumPlayers;

        for (int i = MAX_PLAYERS - 1; i > MIN_PLAYERS - 1; i--)
        {

            if (numAIPlayers == 0)
            {
                _players[i] = Instantiate(Services.Prefabs.Player,
                                            Services.MapManager.Map[0, 0].gameObject.transform.position,
                                            Quaternion.identity,
                                            Services.Main.transform).GetComponent<Player>();
            }
            else
            {
                numAIPlayers--;
                Player _aiPlayer = Instantiate(Services.Prefabs.Player,
                                           Services.MapManager.Map[0, 0].gameObject.transform.position,
                                           Quaternion.identity,
                                           Services.Main.transform);
                GameObject aiPlayerGameObject = _aiPlayer.gameObject;
                Destroy(aiPlayerGameObject.GetComponent<Player>());
                aiPlayerGameObject.AddComponent<AIPlayer>();
                AIPlayer aiPlayer = aiPlayerGameObject.GetComponent<AIPlayer>();
                _players[i] = aiPlayer;
            }     

            int playerNum = i + 1;
            if (_players[i] is AIPlayer)
            {
                _players[i].name = "AI " + PLAYER + playerNum;
            }
            else
            {
                _players[i].name = PLAYER + " " + playerNum;
            }
            _players[i].transform.parent = Services.Scenes.CurrentScene.transform;

            

            switch (i)
            {
                case 0:
                    _players[0].Init(_player1ColorScheme, 0);
                    break;
                case 1:
                    _players[1].Init(_player2ColorScheme, 1);
                    break;
                default:
                    break;
            }
            

        }
    }

    public void ChangeCameraTo(Camera camera)
    {
        _mainCamera = camera;
    }

    // Update is called once per frame
    void Update ()
    {
        Services.InputManager.Update();
    }

    public void Reset(Reset e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string PLAYER = "Player";

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

    private void Awake()
    {
        Services.GameEventManager.Register<Reset>(Reset);
        Input.simulateMouseWithTouches = false;
    }

    public void Init()
    {
        NumPlayers = 1;
        _mainCamera = Camera.main;

        _players = new Player[NumPlayers];
        
    }

	// Use this for initialization
	public void Init (int players)
    {
        _numPlayers = players;
        _mainCamera = Camera.main;
        _players = new Player[NumPlayers];
    }

    public void InitPlayers()
    {
        for (int i = 0; i < NumPlayers; i++)
        {

            _players[i] = Instantiate(  Services.Prefabs.Player, 
                                        Services.MapManager.Map[0, 0].gameObject.transform.position, 
                                        Quaternion.identity, 
                                        Services.Main.transform).GetComponent<Player>();

            int playerNum = i + 1;
            _players[i].name = PLAYER + " " + playerNum;
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
        IntVector2 player1BasePos = new IntVector2(1, 1);
        Services.MapManager.ActivateBase(_players[0], player1BasePos);

        IntVector2 player2BasePos = new IntVector2(
            Services.MapManager.MapWidth - 2,
            Services.MapManager.MapLength - 2);
        Services.MapManager.ActivateBase(_players[1], player2BasePos);
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

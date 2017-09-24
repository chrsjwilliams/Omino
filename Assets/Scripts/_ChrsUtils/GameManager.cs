using UnityEngine.Assertions;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string PLAYER = "Player";

    public int PLAYER_ONE = 0;
    public int PLAYER_TWO = 1;

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
    public Player[] Player
    {
        get { return _players; }
    }

    public int baseWidth;
    public int baseLength;

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
        Services.GameEventManager.Register<LeftStickAxisEvent>(OnLeftStickMoved);
        Services.GameEventManager.Register<DPadAxisEvent>(OnDPadPressed);
    }

    public void Init()
    {
        baseWidth = 3;
        baseLength = 3;
        NumPlayers = 1;
        _mainCamera = Camera.main;

        _players = new Player[NumPlayers];
        InitGameColors();
        
    }

	// Use this for initialization
	public void Init (int players)
    {
        baseWidth = 3;
        baseLength = 3;
        _numPlayers = players;
        _mainCamera = Camera.main;
        _players = new Player[NumPlayers];
        InitGameColors();
    }

    public void InitPlayers()
    {
        int xCoord = Services.MapManager.MapWidth - 1;
        int yCoord = Services.MapManager.MapLength - 1;

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

        Services.MapManager.ActivateBase(_players[0]);
    }
	
    private void InitGameColors()
    {
        _player1ColorScheme = new Color[4];

        _player1ColorScheme[0] = new Color(0.667f, 0.224f, 0.224f);
        _player1ColorScheme[1] = new Color(0.831f, 0.416f, 0.416f);
        _player1ColorScheme[2] = new Color(0.780f, 0.780f, 0.129f);
        _player1ColorScheme[3] = new Color(0.996f, 0.996f, 0.467f);

        _player2ColorScheme = new Color[4];

        _player2ColorScheme[0] = new Color(0.376f, 0.694f, 0.114f);
        _player2ColorScheme[1] = new Color(0.627f, 0.886f, 0.416f);
        _player2ColorScheme[2] = new Color(0.149f, 0.196f, 0.537f);
        _player2ColorScheme[3] = new Color(0.369f, 0.404f, 0.686f);

        _mapColorScheme = new Color[2];

        _mapColorScheme[0] = new Color(0.5f, 0.5f, 0.5f);
        _mapColorScheme[1] = new Color(0.8f, 0.8f, 0.8f);
    }

    public void ChangeCameraTo(Camera camera)
    {
        _mainCamera = camera;
    }

    private void OnLeftStickMoved(LeftStickAxisEvent e)
    {
        Player[e.playerNum - 1].MovePlayerLeftStick(e.leftStickAxis);
    }

    private void OnDPadPressed(DPadAxisEvent e)
    {
        Player[e.playerNum - 1].MovePlayerDPad(e.dPadAxis);
    }

    // Update is called once per frame
    void Update ()
    {
        Services.InputManager.Update();
    }
}

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

    [SerializeField] private Color[] _playerColorScheme;
    public Color[] PlayerColorScheme
    {
        get { return _playerColorScheme; }
    }

    [SerializeField] private Color[] _mapColorScheme;
    public Color[] MapColorScheme
    {
        get { return _mapColorScheme; }
    }

    public void Init()
    {
        NumPlayers = 1;
        _mainCamera = Camera.main;

        _players = new Player[NumPlayers];
        InitGameColors();
    }

	// Use this for initialization
	public void Init (int players)
    {
        NumPlayers = players;
        _mainCamera = Camera.main;
        _players = new Player[NumPlayers];
        InitPlayers();
        InitGameColors();
    }

    public void InitPlayers()
    {
        for(int i = 0; i < NumPlayers; i++)
        {
            _players[i] = Instantiate(  Services.Prefabs.Player, 
                                        Services.MapManager.Map[0,0].gameObject.transform.position, 
                                        Quaternion.identity, 
                                        Services.Main.transform).GetComponent<Player>();

            _players[i].name = PLAYER + " " + i + 1;
        }
    }
	

    private void InitGameColors()
    {
        _playerColorScheme = new Color[4];

        _playerColorScheme[0] = Color.red;
        _playerColorScheme[1] = Color.yellow;
        _playerColorScheme[2] = Color.green;
        _playerColorScheme[3] = Color.blue;

        _mapColorScheme = new Color[2];

        _mapColorScheme[0] = new Color(0.3f, 0.3f, 0.3f);
        _mapColorScheme[1] = new Color(0.7f, 0.7f, 0.7f);
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
}

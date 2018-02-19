using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string PLAYER = "Player";

    public readonly int MAX_PLAYERS = 2;
    public readonly int MIN_PLAYERS = 0;
    
    private bool[] humanPlayers;

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

    public int levelSelected { get; private set; }
    public float winWeight { get; private set; }
    public float structureWeight { get; private set; }

    private void Awake()
    {
        Services.GameEventManager.Register<Reset>(Reset);
        Input.simulateMouseWithTouches = false;
    }

    public void SetUserPreferences(int levelNum)
    {
        levelSelected = levelNum;
    }

    public void Init()
    {
        _mainCamera = Camera.main;
    }

    public void SetNumPlayers(bool[] players)
    {
        humanPlayers = players;
        _players = new Player[MAX_PLAYERS];
    }

    public void SetWinWeight(float weight)
    {
        winWeight = weight;
        PlayerPrefs.SetFloat("winWeight", winWeight);
    }

    public void SetStructureWeight(float weight)
    {
        structureWeight = weight;
        PlayerPrefs.SetFloat("structWeight", structureWeight);
    }

    public void InitPlayers()
    {
        for (int i = 0; i < 2; i++)
        {
            if (humanPlayers[i])
            {
                _players[i] = Instantiate(Services.Prefabs.Player,
                                            Services.MapManager.Map[0, 0].gameObject.transform.position,
                                            Quaternion.identity,
                                            Services.Main.transform).GetComponent<Player>();
            }
            else
            {
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
                    _players[0].Init(_player1ColorScheme, 0, winWeight, structureWeight);
                    break;
                case 1:
                    _players[1].Init(_player2ColorScheme, 1, winWeight, structureWeight);
                    break;
                default:
                    break;
            }

        }
        for (int i = 0; i < 2; i++)
        {
            if (Players[i] is AIPlayer)
            {
                Services.UIManager.ToggleReady(i + 1);
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
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

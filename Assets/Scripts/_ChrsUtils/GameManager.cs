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

    private Color[][] colorSchemes;

    [SerializeField] private Color[] _mapColorScheme;
    public Color[] MapColorScheme
    {
        get { return _mapColorScheme; }
    }

    [SerializeField]
    private Color superDestructorResourceColor;
    public Color NeutralColor { get { return superDestructorResourceColor; } }

    public int levelSelected { get; private set; }
    private float winWeight;
    private float structureWeight;
    private float blueprintWeight;
    private float destructionWeight;

    private AIStrategy[] currentStrategies;

    public bool tutorialMode;


    private void Awake()
    {
        Services.GameEventManager.Register<Reset>(Reset);
        Input.simulateMouseWithTouches = false;
        colorSchemes = new Color[][]
        {
            _player1ColorScheme,
            _player2ColorScheme
        };
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

    public void SetBlueprintWeight(float weight)
    {
        blueprintWeight = weight;
        PlayerPrefs.SetFloat("blueprintWeight", blueprintWeight);
    }

    public void SetAttackWeight(float weight)
    {
        destructionWeight = weight;
        PlayerPrefs.SetFloat("attackWeight", destructionWeight);
    }

    public void InitPlayersTutorialMode()
    {
        winWeight = 1;
        structureWeight = 0;
        blueprintWeight = 0;
        destructionWeight = 0;
        InitPlayers();
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
            AIStrategy strategy = new AIStrategy(
                winWeight, structureWeight, blueprintWeight, destructionWeight);
            _players[i].Init(colorSchemes[i], i, strategy);
        }
        for (int i = 0; i < 2; i++)
        {
            if (Players[i] is AIPlayer)
            {
                Services.UIManager.ToggleReady(i + 1);
            }
        }
    }

    public void InitPlayersEvoMode()
    {
        SetStrategies();
        for (int i = 0; i < 2; i++)
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

            int playerNum = i + 1;

            _players[i].name = "AI " + PLAYER + playerNum;
            _players[i].transform.parent = Services.Scenes.CurrentScene.transform;
            Color[] colorScheme = i == 0 ? _player1ColorScheme : _player2ColorScheme;
            AIStrategy strategy = currentStrategies[i];
            _players[i].Init(colorScheme, i, strategy);
        }
        for (int i = 0; i < 2; i++)
        {
            Services.UIManager.ToggleReady(i + 1);
        }
    }

    public void SetStrategies()
    {
        if (currentStrategies == null) currentStrategies = new AIStrategy[2];
        for (int i = 0; i < 2; i++)
        {
            if (PlayerPrefs.HasKey("strategy" + i))
            {
                string strategyString = PlayerPrefs.GetString("strategy" + i);
                string[] weightArrays = strategyString.Split(',');
                if (weightArrays.Length == 4)
                {
                    currentStrategies[i] = new AIStrategy(
                        float.Parse(weightArrays[0]),
                        float.Parse(weightArrays[1]),
                        float.Parse(weightArrays[2]),
                        float.Parse(weightArrays[3]));
                }
                else
                {
                    float winWeight = 1;
                    float structWeight = 0.11f;
                    float blueprintWeight = 0.23f;
                    float destructionWeight = 0.15f;
                    currentStrategies[i] = new AIStrategy(winWeight, structWeight, 
                        blueprintWeight, destructionWeight);
                }
            }
            else
            {
                float winWeight = 1;
                float structWeight = 0.11f;
                float blueprintWeight = 0.23f;
                float destructionWeight = 0.15f;
                currentStrategies[i] = new AIStrategy(winWeight, structWeight, 
                    blueprintWeight, destructionWeight);
            }
        }

    }

    public void MutateAndSaveStrats(Player winner)
    {
        float mutationRange = 0.02f;
        AIStrategy winningStrat = currentStrategies[winner.playerNum - 1];
        float mutatedWinWeight = winningStrat.winWeight + Random.Range(-mutationRange, mutationRange);
        float mutatedStructWeight = winningStrat.structWeight + Random.Range(-mutationRange, mutationRange);
        float mutatedBlueprintWeight = winningStrat.blueprintWeight + Random.Range(-mutationRange, mutationRange);
        float mutatedDestructionWeight = winningStrat.destructionWeight + Random.Range(-mutationRange, mutationRange);
        float highestWeight = Mathf.Max(mutatedWinWeight, mutatedStructWeight,
            mutatedBlueprintWeight,
            mutatedDestructionWeight);
        mutatedWinWeight /= highestWeight;
        mutatedStructWeight /= highestWeight;
        mutatedBlueprintWeight /= highestWeight;
        mutatedDestructionWeight /= highestWeight;
        AIStrategy mutatedStrat = new AIStrategy(
            mutatedWinWeight, 
            mutatedStructWeight, 
            mutatedBlueprintWeight,
            mutatedDestructionWeight);
        currentStrategies[winner.playerNum % 2] = mutatedStrat;
        for (int i = 0; i < 2; i++)
        {
            AIStrategy strat = currentStrategies[i];
            string strategyString = 
                strat.winWeight + "," + 
                strat.structWeight + "," + 
                strat.blueprintWeight + "," + 
                strat.destructionWeight;
            PlayerPrefs.SetString("strategy" + i, strategyString);
        }
        PlayerPrefs.Save();
        Debug.Log("current best strat: " + winningStrat.winWeight + "," +
            winningStrat.structWeight + "," + winningStrat.blueprintWeight + ","
            + winningStrat.destructionWeight);
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

using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string PLAYER = "Player";

    public readonly int MAX_PLAYERS = 2;
    public readonly int MIN_PLAYERS = 0;
    
    private bool[] humanPlayers;
    public int[] aiLevels = new int[2] { 1, 1 };

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

    public Color[][] colorSchemes { get; private set; }

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
    private float blueprintDestructionWeight;
    private float disconnectionWeight;
    private float destructorForBlueprintWeight;

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

    public void SetBlueprintDestructionWeight(float weight)
    {
        blueprintDestructionWeight = weight;
        PlayerPrefs.SetFloat("blueprintDestructionWeight", structureWeight);
    }

    public void SetDisconnectionWeight(float weight)
    {
        disconnectionWeight = weight;
        PlayerPrefs.SetFloat("disconnectionWeight", blueprintWeight);
    }

    public void SetDestructorForBlueprintWeight(float weight)
    {
        destructorForBlueprintWeight = weight;
        PlayerPrefs.SetFloat("destructorForBlueprintWeight", destructionWeight);
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
        AIStrategy strategy = new AIStrategy(winWeight, structureWeight,
            blueprintWeight, destructionWeight, blueprintDestructionWeight,
            disconnectionWeight, destructorForBlueprintWeight);
        for (int i = 0; i < 2; i++)
        {
            int playerNum = i + 1;
            if (humanPlayers[i])
            {
                _players[i] = Instantiate(Services.Prefabs.Player,
                                            Services.MapManager.Map[0, 0].gameObject.transform.position,
                                            Quaternion.identity,
                                            Services.Scenes.CurrentScene.transform).GetComponent<Player>();
                _players[i].Init(playerNum);
                _players[i].name = PLAYER + " " + playerNum;
            }
            else
            {
                Player _aiPlayer = Instantiate(Services.Prefabs.Player,
                                           Services.MapManager.Map[0, 0].gameObject.transform.position,
                                           Quaternion.identity,
                                           Services.Scenes.CurrentScene.transform);
                GameObject aiPlayerGameObject = _aiPlayer.gameObject;
                Destroy(aiPlayerGameObject.GetComponent<Player>());
                aiPlayerGameObject.AddComponent<AIPlayer>();
                AIPlayer aiPlayer = aiPlayerGameObject.GetComponent<AIPlayer>();
                _players[i] = aiPlayer;
                _players[i].Init(playerNum, strategy, aiLevels[i]);
                _players[i].name = "AI " + PLAYER + playerNum;

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

    public void InitPlayersEvoMode()
    {
        SetStrategies();
        for (int i = 0; i < 2; i++)
        {
            int playerNum = i + 1;
            Player _aiPlayer = Instantiate(Services.Prefabs.Player,
                                       Services.MapManager.Map[0, 0].gameObject.transform.position,
                                       Quaternion.identity,
                                       Services.Scenes.CurrentScene.transform);
            GameObject aiPlayerGameObject = _aiPlayer.gameObject;
            Destroy(aiPlayerGameObject.GetComponent<Player>());
            aiPlayerGameObject.AddComponent<AIPlayer>();
            AIPlayer aiPlayer = aiPlayerGameObject.GetComponent<AIPlayer>();
            _players[i] = aiPlayer;

            _players[i].name = "AI " + PLAYER + playerNum;
            _players[i].transform.parent = Services.Scenes.CurrentScene.transform;
            Color[] colorScheme = i == 0 ? _player1ColorScheme : _player2ColorScheme;
            AIStrategy strategy = currentStrategies[i];

            _players[i].Init(playerNum, strategy, 10);
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
                if (weightArrays.Length == 7)
                {
                    currentStrategies[i] = new AIStrategy(
                        float.Parse(weightArrays[0]),
                        float.Parse(weightArrays[1]),
                        float.Parse(weightArrays[2]),
                        float.Parse(weightArrays[3]),
                        float.Parse(weightArrays[4]),
                        float.Parse(weightArrays[5]),
                        float.Parse(weightArrays[6]));
                }
                else
                {
                    float winWeight = 1;
                    float structWeight = 0.11f;
                    float blueprintWeight = 0.23f;
                    float destructionWeight = 0.15f;
                    float blueprintDestructionWeight = 0.2f;
                    float disconnectionWeight = 0.4f;
                    float destructorForBlueprintWeight = 0.5f;
                    currentStrategies[i] = new AIStrategy(
                        winWeight, structWeight, blueprintWeight, destructionWeight,
                        blueprintDestructionWeight, disconnectionWeight, destructorForBlueprintWeight);
                }
            }
            else
            {
                float winWeight = 1;
                float structWeight = 0.11f;
                float blueprintWeight = 0.23f;
                float destructionWeight = 0.15f;
                float blueprintDestructionWeight = 0.2f;
                float disconnectionWeight = 0.4f;
                float destructorForBlueprintWeight = 0.5f;
                currentStrategies[i] = new AIStrategy(
                    winWeight, structWeight, blueprintWeight, destructionWeight,
                    blueprintDestructionWeight, disconnectionWeight, destructorForBlueprintWeight);
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
        float mutatedBlueprintDestructionWeight = winningStrat.blueprintDestructionWeight + Random.Range(-mutationRange, mutationRange);
        float mutatedDisconnectionWeight = winningStrat.disconnectionWeight + Random.Range(-mutationRange, mutationRange);
        float mutatedDestructorForBlueprintWeight = winningStrat.destructorForBlueprintWeight + Random.Range(-mutationRange, mutationRange);

        float highestWeight = Mathf.Max(mutatedWinWeight, mutatedStructWeight,
            mutatedBlueprintWeight,
            mutatedDestructionWeight,
            mutatedBlueprintDestructionWeight,
            mutatedDisconnectionWeight,
            mutatedDestructorForBlueprintWeight);

        mutatedWinWeight /= highestWeight;
        mutatedStructWeight /= highestWeight;
        mutatedBlueprintWeight /= highestWeight;
        mutatedDestructionWeight /= highestWeight;
        mutatedBlueprintDestructionWeight /= highestWeight;
        mutatedDisconnectionWeight /= highestWeight;
        mutatedDestructorForBlueprintWeight /= highestWeight;

        AIStrategy mutatedStrat = new AIStrategy(
            mutatedWinWeight, 
            mutatedStructWeight, 
            mutatedBlueprintWeight,
            mutatedDestructionWeight,
            mutatedBlueprintDestructionWeight,
            mutatedDisconnectionWeight,
            mutatedDestructorForBlueprintWeight);

        currentStrategies[winner.playerNum % 2] = mutatedStrat;

        for (int i = 0; i < 2; i++)
        {
            AIStrategy strat = currentStrategies[i];
            string strategyString = 
                strat.winWeight + "," + 
                strat.structWeight + "," + 
                strat.blueprintWeight + "," + 
                strat.destructionWeight + "," +
                strat.blueprintDestructionWeight + "," +
                strat.disconnectionWeight + "," +
                strat.destructorForBlueprintWeight;

            PlayerPrefs.SetString("strategy" + i, strategyString);
        }
        PlayerPrefs.Save();
        Debug.Log("current best strat: WW: " + winningStrat.winWeight + ", Struct: " +
            winningStrat.structWeight + ", BP: " + winningStrat.blueprintWeight + ", ATK: "
            + winningStrat.destructionWeight + ", BPATTK: " + winningStrat.blueprintDestructionWeight +
            "DISCON: , " + winningStrat.disconnectionWeight + ", DES4BP: " + winningStrat.destructorForBlueprintWeight);
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

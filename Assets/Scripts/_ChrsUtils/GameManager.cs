using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string PLAYER = "Player";

    public readonly int MAX_PLAYERS = 2;
    
    private bool[] humanPlayers;
    public AILEVEL[] aiLevels;
    public TitleSceneScript.GameMode mode;
    public bool destructorsEnabled = true;

    public bool blueprintsEnabled = true;

    private bool soundEffectsEnabled = true;
    public bool SoundEffectsEnabled
    {
        get { return soundEffectsEnabled; }
        set { soundEffectsEnabled = value;    UpdateSoundEffectPlayerPrefs(); }
    }

    private bool musicEnabled = true;

    public bool MusicEnabled
    {
        get { return musicEnabled; }
        set { musicEnabled = value;    UpdateMusicPlayerPrefs(); }
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

    public Color[][] colorSchemes { get; private set; }

    [SerializeField] private Color[] _mapColorScheme;
    public Color[] MapColorScheme
    {
        get { return _mapColorScheme; }
    }

    [SerializeField]
    private Color neutralColor;
    public Color NeutralColor { get { return neutralColor; } }

    public Level levelSelected { get; private set; }
    private float winWeight;
    private float structureWeight;
    private float blueprintWeight;
    private float destructionWeight;
    private float blueprintDestructionWeight;
    private float disconnectionWeight;
    private float destructorForBlueprintWeight;
    private float dangerWeight;

    private readonly string SOUNDEFFECTSENABLED = "soundEffectsEnabledKey";
    private readonly string MUSICENABLED = "musicEnabledKey";

    private AIStrategy[] currentStrategies;

    private void Awake()
    {
        Services.GameEventManager.Register<Reset>(Reset);
        Input.simulateMouseWithTouches = false;
        colorSchemes = new Color[][]
        {
            _player1ColorScheme,
            _player2ColorScheme
        };

        if ((PhotonNetwork.connected) && (!PhotonNetwork.player.isMasterClient))
        {
            colorSchemes = new Color[][]
            {
                _player2ColorScheme,
                _player1ColorScheme
            };
        }

        CheckPlayerPrefs();
    }

    public void SetCurrentLevel(Level level)
    {
        levelSelected = level;
        if (level != null)
        {
            blueprintsEnabled = level.blueprintsEnabled;
            destructorsEnabled = level.destructorsEnabled;
        }
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
        PlayerPrefs.SetFloat("blueprintDestructionWeight", blueprintDestructionWeight);
    }

    public void SetDisconnectionWeight(float weight)
    {
        disconnectionWeight = weight;
        PlayerPrefs.SetFloat("disconnectionWeight", disconnectionWeight);
    }

    public void SetDestructorForBlueprintWeight(float weight)
    {
        destructorForBlueprintWeight = weight;
        PlayerPrefs.SetFloat("destructorForBlueprintWeight", destructorForBlueprintWeight);
    }

    public void SetDangerWeight(float weight)
    {
        dangerWeight = weight;
        PlayerPrefs.SetFloat("dangerMod", dangerWeight);
    }

    public void InitPlayers()
    {
        AIStrategy strategy = new AIStrategy(winWeight, structureWeight,
            blueprintWeight, destructionWeight, blueprintDestructionWeight,
            disconnectionWeight, destructorForBlueprintWeight, dangerWeight);
        if (Services.MapManager.currentLevel != null &&
            Services.MapManager.currentLevel.overrideStrategy.overrideDefault)
            strategy = Services.MapManager.currentLevel.overrideStrategy;
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
            AIStrategy strategy = currentStrategies[i];

            _players[i].Init(playerNum, strategy, AILEVEL.HARD);
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
                if (weightArrays.Length == 8)
                {
                    currentStrategies[i] = new AIStrategy(
                        float.Parse(weightArrays[0]),
                        float.Parse(weightArrays[1]),
                        float.Parse(weightArrays[2]),
                        float.Parse(weightArrays[3]),
                        float.Parse(weightArrays[4]),
                        float.Parse(weightArrays[5]),
                        float.Parse(weightArrays[6]),
                        float.Parse(weightArrays[6]));
                }
                else
                {
                    float winWeight = 1;
                    float structWeight = 0.11f;
                    float blueprintWeight = 0.23f;
                    float destructionWeight = 0.2f;
                    float blueprintDestructionWeight = 0.25f;
                    float disconnectionWeight = 0.6f;
                    float destructorForBlueprintWeight = 0.5f;
                    float dangerMod = 1.75f;
                    currentStrategies[i] = new AIStrategy(
                        winWeight, structWeight, blueprintWeight, destructionWeight,
                        blueprintDestructionWeight, disconnectionWeight, destructorForBlueprintWeight,
                        dangerMod);
                }
            }
            else
            {
                float winWeight = 1;
                float structWeight = 0.11f;
                float blueprintWeight = 0.23f;
                float destructionWeight = 0.2f;
                float blueprintDestructionWeight = 0.25f;
                float disconnectionWeight = 0.6f;
                float destructorForBlueprintWeight = 0.5f;
                float dangerMod = 1.75f;
                currentStrategies[i] = new AIStrategy(
                    winWeight, structWeight, blueprintWeight, destructionWeight,
                    blueprintDestructionWeight, disconnectionWeight, destructorForBlueprintWeight,
                    dangerMod);
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
        float mutatedDangerMod = winningStrat.dangerMod + Random.Range(-mutationRange, mutationRange);

        float highestWeight = Mathf.Max(mutatedWinWeight, mutatedStructWeight,
            mutatedBlueprintWeight,
            mutatedDestructionWeight,
            mutatedBlueprintDestructionWeight,
            mutatedDisconnectionWeight,
            mutatedDestructorForBlueprintWeight,
            mutatedDangerMod);

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
            mutatedDestructorForBlueprintWeight,
            mutatedDangerMod);

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
                strat.destructorForBlueprintWeight + "," +
                strat.dangerMod;

            PlayerPrefs.SetString("strategy" + i, strategyString);
        }
        PlayerPrefs.Save();
        Debug.Log("current best strat: WW: " + winningStrat.winWeight + ", Struct: " +
            winningStrat.structWeight + ", BP: " + winningStrat.blueprintWeight + ", ATK: "
            + winningStrat.destructionWeight + ", BPATTK: " + winningStrat.blueprintDestructionWeight +
            " DISCON: , " + winningStrat.disconnectionWeight + ", DES4BP: " + winningStrat.destructorForBlueprintWeight +
            " DNGR: " + winningStrat.dangerMod);
    }

    public Color AdjustColorAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    private void CheckPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(SOUNDEFFECTSENABLED))
        {
            switch (PlayerPrefs.GetInt(SOUNDEFFECTSENABLED))
            {
                case (1) :
                    SoundEffectsEnabled = true;
                    break;
                case (0) :
                    SoundEffectsEnabled = false;
                    break;
            }
        }
        else
        {
            UpdateSoundEffectPlayerPrefs();
        }
        
        if (PlayerPrefs.HasKey(MUSICENABLED))
        {   
            switch (PlayerPrefs.GetInt(MUSICENABLED))
            {
                case (1) :
                    MusicEnabled = true;
                    break;
                case (0) :
                    MusicEnabled = false;
                    break;
            }
        }
        else
        {
            UpdateMusicPlayerPrefs();
        }
        
        Services.AudioManager.SetMusicOnOrOff();
        Services.AudioManager.SetSoundEffectsOnOrOff();
    }

    private void UpdateSoundEffectPlayerPrefs()
    {
        PlayerPrefs.SetInt(SOUNDEFFECTSENABLED, SoundEffectsEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateMusicPlayerPrefs()
    {
        PlayerPrefs.SetInt(MUSICENABLED, MusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
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

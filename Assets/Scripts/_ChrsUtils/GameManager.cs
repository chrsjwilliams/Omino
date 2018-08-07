using System;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Beat;
using Tinylytics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public string PLAYER = "Player";

    public readonly int MAX_PLAYERS = 2;

    [SerializeField] private bool debug;
    [SerializeField] private bool timedRestart;
    public bool disableUI;
    public bool pretendIphone;
    public bool onIPhone
    {
        get
        {
            return SystemInfo.deviceModel.Contains("iPhone") || pretendIphone;
        }
    }

    private bool[] humanPlayers;

    public static string modeStatusFileName
    {
        get
        {
            return Application.persistentDataPath + Path.DirectorySeparatorChar +
              "modestatus.txt";
        }
    }

    public Dictionary<TitleSceneScript.GameMode, bool> modeUnlockStatuses { get; private set; }
    
    public AILEVEL[] aiLevels;
    public TitleSceneScript.GameMode mode = TitleSceneScript.GameMode.NONE;
    public bool destructorsEnabled = true;

    public double levelBPM;

    public bool blueprintsEnabled = true;
    public bool generatorEnabled = true;
    public bool factoryEnabled = true;
    public bool barracksEnabled = true;
    public bool BlueprintAssistEnabled
    {
        get { return blueprintAssistEnabled; }
        set
        {
            blueprintAssistEnabled = value;
            PlayerPrefs.SetInt(BLUEPRINTASSISTENABLED, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
    private bool blueprintAssistEnabled = true;

    public readonly string BLUEPRINTASSISTENABLED = "BlueprintAssistEnabled";

    private bool soundEffectsEnabled = true;
    public bool SoundEffectsEnabled
    {
        get { return soundEffectsEnabled; }
        set { 
            soundEffectsEnabled = value;    
            UpdateSoundEffectPlayerPrefs();
        }
    }

    private bool musicEnabled = true;

    public bool MusicEnabled
    {
        get { return musicEnabled; }
        set
        {
            musicEnabled = value;
            UpdateMusicPlayerPrefs();
        }
    }

    public PlayerHandicap[] handicapValue { get; private set; }

    [SerializeField] private Camera _mainCamera;
    public Camera MainCamera
    {
        get { return _mainCamera; }
    }

	[SerializeField]
	private Clock clock;

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

    [SerializeField]
    private float defaultWinWeight;
    [SerializeField]
    private float defaultStructWeight;
    [SerializeField]
    private float defaultBlueprintWeight;
    [SerializeField]
    private float defaultAttackWeight;
    [SerializeField]
    private float defaultBlueprintDestructionWeight;
    [SerializeField]
    private float defaultDisconnectionWeight;
    [SerializeField]
    private float defaultDestructorForBlueprintWeight;
    [SerializeField]
    private float defaultDangerWeight;

    private readonly string SOUNDEFFECTSENABLED = "soundEffectsEnabledKey";
    private readonly string MUSICENABLED = "musicEnabledKey";

    private AIStrategy[] currentStrategies;
    private float inactivityTimer;
    private const float inactivityBeforeReset = 180f;

    public bool eloTrackingMode;

    

    private void Awake()
    {

        Assert.raiseExceptions = true;
        InitializeServices();

        Services.GlobalEventManager.Register<Reset>(Reset);
        Services.GlobalEventManager.Register<TouchDown>(ResetInactivity);
        Services.GlobalEventManager.Register<MouseDown>(ResetInactivity);
        Input.simulateMouseWithTouches = false;
        colorSchemes = new Color[][]
        {
            _player1ColorScheme,
            _player2ColorScheme
        };

        CheckPlayerPrefs();

        ELOManager.LoadData();
        DungeonRunManager.LoadData();

        if (!debug)
        {
            Services.Scenes.PushScene<TitleSceneScript>();
        }

    }

    private void InitializeServices()
    {
        Services.GameEventManager = new GameEventsManager();
        Services.GlobalEventManager = new GameEventsManager();
        Services.GameManager = this;
        Services.GameData = GetComponent<GameData>();
        Init();

        Services.MapManager = GetComponent<MapManager>();
        Services.MapManager.Init();

        Services.Clips = Resources.Load<ClipLibrary>("Audio/ClipLibrary");
        Services.AudioManager = new GameObject("Audio Manager").AddComponent<AudioManager>();

        Services.GeneralTaskManager = new TaskManager();
        Services.Prefabs = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
        Services.TechDataLibrary = Resources.Load<TechDataLibrary>("ContentData/TechDataLibrary");
        Services.EloRankData = Resources.Load<EloRankData>("ContentData/EloRankData");

        Services.InputManager = new InputManager();
        Services.Scenes = new GameSceneManager<TransitionData>(gameObject, Services.Prefabs.Scenes);
		Services.CameraController = MainCamera.GetComponent<CameraController>();
		Services.Clock = clock;
        Services.Clock.Init(levelBPM);

        Services.Analytics = AnalyticsManager.Instance;
    }

    public void SetCurrentLevel(Level level)
    {
        levelSelected = level;
        if (level != null)
        {
            blueprintsEnabled = level.blueprintsEnabled;
            generatorEnabled = level.generatorEnabled;
            factoryEnabled = level.factoryEnabled;
            barracksEnabled = level.barracksEnabled;

            destructorsEnabled = level.destructorsEnabled;
        }
    }

    public void Init()
    {
        //_mainCamera = Camera.main;
        SetWinWeight(defaultWinWeight);
        SetStructureWeight(defaultStructWeight);
        SetBlueprintWeight(defaultBlueprintWeight);
        SetAttackWeight(defaultAttackWeight);
        SetBlueprintDestructionWeight(defaultBlueprintDestructionWeight);
        SetDisconnectionWeight(defaultDisconnectionWeight);
        SetDestructorForBlueprintWeight(defaultDestructorForBlueprintWeight);
        SetDangerWeight(defaultDangerWeight);

        HandicapSystem.Init();
        LoadModeStatusData();
    }

    private void LoadModeStatusData()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            modeStatusFileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(filePath))
        {
            file = File.OpenRead(filePath);
            try
            {
                string modeStatusDataString = (string)bf.Deserialize(file);
                //unlockedModes = StringToBoolArray(modeStatusDataString);
                modeUnlockStatuses = ParseModeDictString(modeStatusDataString);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to deserialize. Reason: " + e.Message);
                file.Dispose();
                //currentModeStatusData = defaultModeStatus;
                modeUnlockStatuses = DefaultUnlockStatus();
                SaveModeStatusData();
                // throw;
            }
            finally
            {
                file.Close();
            }
        }
        else
        {
            file = File.Create(filePath);
            //currentModeStatusData = defaultModeStatus;
            //unlockedModes = StringToBoolArray(currentModeStatusData);
            //SetUnlockingData();
            modeUnlockStatuses = DefaultUnlockStatus();
            //bf.Serialize(file, currentModeStatusData);
            bf.Serialize(file, ModeDictToString(modeUnlockStatuses));

            file.Close();
        }
    }

    public void SaveModeStatusData()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            modeStatusFileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        file = File.OpenWrite(filePath);

        //bf.Serialize(file, currentModeStatusData);
        bf.Serialize(file, ModeDictToString(modeUnlockStatuses));
        file.Close();
    }

    public void UnlockMode(TitleSceneScript.GameMode mode, bool status)
    {
        modeUnlockStatuses[mode] = status;
        SaveModeStatusData();
    }

    private string ModeDictToString(Dictionary<TitleSceneScript.GameMode, bool> unlockStatusDict)
    {
        string stringVal = "";
        foreach(KeyValuePair<TitleSceneScript.GameMode, bool> kvPair in unlockStatusDict)
        {
            stringVal += (int)kvPair.Key + ":" + Convert.ToInt32(kvPair.Value)+",";
        }
        return stringVal;
    }

    private Dictionary<TitleSceneScript.GameMode, bool> ParseModeDictString(string modeDictString)
    {
        Dictionary<TitleSceneScript.GameMode, bool> dict = new Dictionary<TitleSceneScript.GameMode, bool>();
        string[] splitStr = modeDictString.Split(',');
        foreach(string substr in splitStr)
        {
            string[] splitSubstr = substr.Split(':');
            int modeVal = 0;
            int unlockVal = 1;
            if(int.TryParse(splitSubstr[0], out modeVal) && int.TryParse(splitSubstr[1], out unlockVal))
            {
                dict.Add((TitleSceneScript.GameMode)modeVal, unlockVal != 0);
            }
        }
        foreach(TitleSceneScript.GameMode mode in TitleSceneScript.unlockableModes)
        {
            if (!dict.ContainsKey(mode)) dict.Add(mode, false);
        }
        return dict;
    }

    private Dictionary<TitleSceneScript.GameMode, bool> DefaultUnlockStatus()
    {
        Dictionary<TitleSceneScript.GameMode, bool> dict = new Dictionary<TitleSceneScript.GameMode, bool>();
        foreach(TitleSceneScript.GameMode mode in TitleSceneScript.unlockableModes)
        {
            dict.Add(mode, false);
        }
        return dict;
    }

    public void UnlockAllModes()
    {
        List<TitleSceneScript.GameMode> modes = 
            new List<TitleSceneScript.GameMode>(modeUnlockStatuses.Keys);
        foreach(TitleSceneScript.GameMode mode in modes)
        {
            modeUnlockStatuses[mode] = true;
        }
        SaveModeStatusData();
    }

    public void ModeUnlockReset()
    {
        modeUnlockStatuses = DefaultUnlockStatus();
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

    public void SetHandicapValues(PlayerHandicap[] handicapValue_)
    {
        handicapValue = handicapValue_;
    }

    public void InitPlayers(PlayerHandicap[] handicapValue)
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

                _players[i].Init(playerNum, handicapValue[i]);

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
                _players[i].Init(playerNum, strategy, aiLevels[i], handicapValue[i]);
                _players[i].name = "AI " + PLAYER + playerNum;

            }
        }
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

        float mutatedWinWeight = winningStrat.winWeight + UnityEngine.Random.Range(-mutationRange, mutationRange);
        float mutatedStructWeight = winningStrat.structWeight + UnityEngine.Random.Range(-mutationRange, mutationRange);
        float mutatedBlueprintWeight = winningStrat.blueprintWeight + UnityEngine.Random.Range(-mutationRange, mutationRange);
        float mutatedDestructionWeight = winningStrat.destructionWeight + UnityEngine.Random.Range(-mutationRange, mutationRange);
        float mutatedBlueprintDestructionWeight = winningStrat.blueprintDestructionWeight + UnityEngine.Random.Range(-mutationRange, mutationRange);
        float mutatedDisconnectionWeight = winningStrat.disconnectionWeight + UnityEngine.Random.Range(-mutationRange, mutationRange);
        float mutatedDestructorForBlueprintWeight = winningStrat.destructorForBlueprintWeight + UnityEngine.Random.Range(-mutationRange, mutationRange);
        float mutatedDangerMod = winningStrat.dangerMod + UnityEngine.Random.Range(-mutationRange, mutationRange);

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
        if (PlayerPrefs.HasKey(BLUEPRINTASSISTENABLED))
        {
            BlueprintAssistEnabled = PlayerPrefs.GetInt(BLUEPRINTASSISTENABLED) == 1;
        }
        else
        {
            BlueprintAssistEnabled = true;
        }

        if (PlayerPrefs.HasKey(SOUNDEFFECTSENABLED))
        {
            SoundEffectsEnabled = PlayerPrefs.GetInt(SOUNDEFFECTSENABLED) == 1;
        }
        else
        {
            UpdateSoundEffectPlayerPrefs();
        }
        
        if (PlayerPrefs.HasKey(MUSICENABLED))
        {
            MusicEnabled = PlayerPrefs.GetInt(MUSICENABLED) == 1;
        }
        else
        {
            UpdateMusicPlayerPrefs();
        }
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
        Services.GeneralTaskManager.Update();
        if (timedRestart)
        {
            InactivityCheck();
        }

        if (Input.GetKeyDown(KeyCode.M)) UnlockAllModes();
    }

    public void Reset(Reset e)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToggleBlueprintAssist()
    {
        BlueprintAssistEnabled = !BlueprintAssistEnabled;
    }

    private void InactivityCheck()
    {
        inactivityTimer += Time.deltaTime;
        if (inactivityTimer >= inactivityBeforeReset)
        {
            Services.AudioManager.FadeOutLevelMusic();
            Reset(new Reset());
        }
    }

    private void ResetInactivity(MouseDown e)
    {
        inactivityTimer = 0;
    }

    private void ResetInactivity(TouchDown e)
    {
        inactivityTimer = 0;
    }

    public Color[][] GetColorScheme()
    {
        return colorSchemes;
    }
    
    public void SetColorScheme(Color[][] colors)
    {
        colorSchemes = colors;
    }
}

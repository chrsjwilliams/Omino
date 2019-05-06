using System;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.SceneManagement;
using BeatManagement;
using Tinylytics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public enum DEVICE { IPAD, IPHONE, IPHONE_X, IPHONE_SE}

public class GameManager : MonoBehaviour
{
    public string PLAYER = "Player";

    public readonly int MAX_PLAYERS = 2;

    [SerializeField] private bool debug;
    [SerializeField] private bool timedRestart;
    public bool disableUI;

    public DEVICE CurrentDevice = DEVICE.IPAD;

    public bool pretendIphone;
    public bool onIPhone
    {
        get
        {
            return CurrentDevice == DEVICE.IPHONE || pretendIphone;
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
    
    private bool neonEnabled = true;
    public bool NeonEnabled
    {
        get { return neonEnabled; }
        set
        {
            neonEnabled = value;
            UpdateNeonEnabledPlayerPrefs();
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

    #region TileSkin Items
    public Dictionary<TileSkin, bool> ownedTileSkins = new Dictionary<TileSkin, bool>();
    public TileSkin[] currentTileSkins;

    [SerializeField] private Color[] _player1ColorScheme;
    public Color[] Player1ColorScheme
    {
        get
        {
            if (currentTileSkins[0] != null)
            {
                return currentTileSkins[0].Player1ColorScheme;
            }
            else
            {
                return _player1ColorScheme;
            }
        }
    }

    [SerializeField] private Color[] _player2ColorScheme;
    public Color[] Player2ColorScheme
    {
        get
        {
            if (currentTileSkins[1] != null)
            {
                return currentTileSkins[1].Player2ColorScheme;
            }
            else
            {
                return _player2ColorScheme;
            }
        }
    }

    public Color[][] colorSchemes { get; private set; }

    [SerializeField] private Color[] _mapColorScheme;
    public Color[] MapColorScheme
    {
        get
        {
            return _mapColorScheme;          
        }
    }

    [SerializeField]
    private Color neutralColor;
    public Color NeutralColor {
        get
        {
            return neutralColor;          
        }
    }
    #endregion


    public Sprite proceduralDisplayImage;
    public Sprite customMapDisplayImage;
    public bool loadedLevel { get; private set; }
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
    private readonly string NEONENABLED = "neonEnableKey";

    public AIStrategy[] currentStrategies { get; private set; }
    private float inactivityTimer;
    private const float inactivityBeforeReset = 180f;

    public bool eloTrackingMode;

    private const string autoUnlockFileName = "autounlock.txt";
    private const bool defaultAutoUnlockStatus = true;

    private void Awake()
    {
        Assert.raiseExceptions = true;
        SetDevice();
        InitializeServices();
        Services.GlobalEventManager.Register<Reset>(Reset);
        Services.GlobalEventManager.Register<TouchDown>(ResetInactivity);
        Services.GlobalEventManager.Register<MouseDown>(ResetInactivity);
        Input.simulateMouseWithTouches = false;
        colorSchemes = new Color[][]
        {
            Player1ColorScheme,
            Player2ColorScheme
        };

        CheckPlayerPrefs();
        LevelManager.LoadData();
        Level[] allLevels = Resources.LoadAll<Level>("Levels");
        foreach (Level level in allLevels)
        {
            if (!level.name.Contains("Tutorial"))
            {
                level.SetLevelData();
                LevelManager.AddLevel(level.name, level.data);
            }
        }

        Level[] dungeonLevels = Resources.LoadAll<Level>("Levels/DungeonLevels");
        foreach(Level level in dungeonLevels)
        {
            level.SetLevelData();
            LevelManager.AddLevel(level.name, level.data, false, true);
        }
        Services.MapManager.PopulateDungeonRunLevels();
       
        ELOManager.LoadData();
        ELOManager.eloData.ReportScore();
        DungeonRunManager.LoadData();

        if (!debug)
        {
            Services.Scenes.PushScene<TitleSceneScript>();
        }
        // for testflight purposes so people don't have to play tutorial
        CheckAutoUnlock();
        //  Adds the levels in the resource folder that aren't Tutorial levels
        
    }

    private void SetDevice()
    {
        switch (UnityEngine.iOS.Device.generation)
        {
            case UnityEngine.iOS.DeviceGeneration.iPhone5:
            case UnityEngine.iOS.DeviceGeneration.iPhone5C:
            case UnityEngine.iOS.DeviceGeneration.iPhone5S:
            case UnityEngine.iOS.DeviceGeneration.iPhone6:
            case UnityEngine.iOS.DeviceGeneration.iPhone6Plus:
            case UnityEngine.iOS.DeviceGeneration.iPhone6S:
            case UnityEngine.iOS.DeviceGeneration.iPhone6SPlus:
            case UnityEngine.iOS.DeviceGeneration.iPhone7:
            case UnityEngine.iOS.DeviceGeneration.iPhone7Plus:
            case UnityEngine.iOS.DeviceGeneration.iPhone8:
            case UnityEngine.iOS.DeviceGeneration.iPhone8Plus:           
            case UnityEngine.iOS.DeviceGeneration.iPhoneUnknown:
            case UnityEngine.iOS.DeviceGeneration.iPhoneSE1Gen:
                CurrentDevice = DEVICE.IPHONE;
                break;
            case UnityEngine.iOS.DeviceGeneration.iPhoneX:
                CurrentDevice = DEVICE.IPHONE_X;
                break;
            default:
                CurrentDevice = DEVICE.IPAD;
                break;
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
        Services.LevelDataLibrary  = Resources.Load<LevelDataLibrary>("ContentData/LevelDataLibrary");
        Services.EloRankData = Resources.Load<EloRankData>("ContentData/EloRankData");

        Services.InputManager = new InputManager();
        Services.Scenes = new GameSceneManager<TransitionData>(gameObject, Services.Prefabs.Scenes);
		Services.CameraController = MainCamera.GetComponent<CameraController>();
		Services.Clock = clock;
        Services.Clock.Init(levelBPM);
        Services.LeaderBoard = GetComponent<GameCenterLeaderBoard>();
        Services.LeaderBoard.Init();

        Services.Analytics = AnalyticsManager.Instance;
    }

    public void SetCurrentLevel(Level level, bool loaded = false)
    {
        if (loaded) loadedLevel = true;
        else loadedLevel = false;

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
        //UnlockMode(TitleSceneScript.GameMode.Shop, false);
    }

    private void CheckAutoUnlock()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            autoUnlockFileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        bool autoUnlock;
        if (File.Exists(filePath))
        {
            file = File.OpenRead(filePath);
            try
            {
                autoUnlock = (bool)bf.Deserialize(file);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to deserialize. Reason: " + e.Message);
                file.Dispose();
                SaveDefaultAutoUnlockStatus();
                autoUnlock = defaultAutoUnlockStatus;
            }
            finally
            {
                file.Close();
            }
        }
        else
        {
            file = File.Create(filePath);
            
            bf.Serialize(file, defaultAutoUnlockStatus);

            file.Close();
            autoUnlock = defaultAutoUnlockStatus;
        }

        if (autoUnlock) UnlockAllModes();
    }

    public void SaveDefaultAutoUnlockStatus()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            autoUnlockFileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        file = File.OpenWrite(filePath);
        bf.Serialize(file, defaultAutoUnlockStatus);
        file.Close();
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
                modeUnlockStatuses = ParseModeDictString(modeStatusDataString);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to deserialize. Reason: " + e.Message);
                file.Dispose();
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

            modeUnlockStatuses = DefaultUnlockStatus();
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
        //UnlockMode(TitleSceneScript.GameMode.Shop, false);
        SaveModeStatusData();
    }

    public void ModeUnlockReset()
    {
        modeUnlockStatuses = DefaultUnlockStatus();
        SaveModeStatusData();
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

    public void InitPlayers(PlayerHandicap[] handicapValue, bool editMode = false)
    {
        AIStrategy strategy = new AIStrategy(winWeight, structureWeight,
            blueprintWeight, destructionWeight, blueprintDestructionWeight,
            disconnectionWeight, destructorForBlueprintWeight, dangerWeight);
        if (!editMode && Services.MapManager.currentLevel != null &&
            Services.MapManager.currentLevel.overrideStrategy != null &&
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
                if (editMode)
                {
                    GameObject playerGameObject = _players[i].gameObject;
                    Destroy(playerGameObject.GetComponent<Player>());
                    playerGameObject.AddComponent<EditModePlayer>();
                    EditModePlayer editModePlayer = playerGameObject.GetComponent<EditModePlayer>();
                    editModePlayer.Init(1);
                    _players[i] = editModePlayer;
                }
                else
                {
                    _players[i].Init(playerNum, handicapValue[i]);

                    _players[i].name = PLAYER + " " + playerNum;
                }
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

    public void SetStrategies(bool fromLevelGenerator = false, string strategyFile = "")
    {
        int numStrategies = fromLevelGenerator ? 1 : 2;
        if (currentStrategies == null) currentStrategies = new AIStrategy[numStrategies];
        
        for (int i = 0; i < numStrategies; i++)
        {
            if (PlayerPrefs.HasKey("strategy" + i) || fromLevelGenerator)
            {
                string strategyString = PlayerPrefs.GetString("strategy" + i);
                if(fromLevelGenerator)
                {
                    strategyString = strategyFile;
                }
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

    private void UpdateNeonEnabledPlayerPrefs()
    {
        PlayerPrefs.SetInt(NEONENABLED, NeonEnabled? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateMusicPlayerPrefs()
    {
        PlayerPrefs.SetInt(MUSICENABLED, MusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public LevelGenerator test_LevelGenerator;
    public TextAsset test_LevelGeneratorTextAsset;
    public Texture2D text_LevelGeneratorMapData;
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

        if (Input.GetKeyDown(KeyCode.T))
        {
            test_LevelGenerator.Init(test_LevelGeneratorTextAsset, text_LevelGeneratorMapData);
        }
        if(Input.GetKeyDown(KeyCode.Y))
        {
            Level[] allLevels = Resources.LoadAll<Level>("Levels");
            foreach(Level level in allLevels)
            {
                if(!level.name.Contains("Tutorial"))
                {
                    level.SetLevelData();
                    LevelManager.AddLevel(level.name, level.data);
                }
            }

            LevelManager.PrintLevelNames();
        }
        if(Input.GetKeyDown(KeyCode.U))
        {
            LevelManager.PrintLevelNames();
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log(test_LevelGenerator);
            test_LevelGenerator.level.SetLevelData();
            LevelManager.AddLevel("Test", test_LevelGenerator.level.data);
            LevelManager.SaveData();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            LevelManager.PrintLevelNames();
        }

    }

    public void Reset(Reset e)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ToggleNeon()
    {
        MK.Glow.MKGlowFree glow = MainCamera.GetComponent<MK.Glow.MKGlowFree>();
        NeonEnabled = !NeonEnabled;
        if (NeonEnabled)
        {
            glow.GlowIntensityInner = 0.2f;
        }
        else
        {
            glow.GlowIntensityInner = 0;
        }
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

    public Color HexToColor(string color)
    {
        int withAlpha = 8;
        int noAlpha = 6;
        float maxValue = 255;

        if (color.Contains("#")) color = color.Remove('#');

        string r, g, b, a = "";


        if (color.Length == withAlpha)
        {
            r = color.Substring(0, 2);
            g = color.Substring(2, 2);
            b = color.Substring(4, 2);
            a = color.Substring(6, 2);
            return new Color((HexToInteger(r) / maxValue),
                                (HexToInteger(g) / maxValue),
                                (HexToInteger(b) / maxValue),
                                (HexToInteger(a) / maxValue));

        }
        else if (color.Length == noAlpha)
        {
            r = color.Substring(0, 2);
            g = color.Substring(2, 2);
            b = color.Substring(4, 2);
            return new Color((HexToInteger(r) / maxValue),
                                (HexToInteger(g) / maxValue),
                                (HexToInteger(b) / maxValue));
        }
        else
        {
            return Color.magenta;
        }
    }

    private int HexToInteger(string hex)
    {
        int power = 1;
        int result = 0;
        char[] hexCharArray = hex.ToCharArray();
        
        for (int i = hex.Length - 1; i >= 0; i--)
        {
            result += GetRawInt(hexCharArray[i]) * power;
            power *= 16;
        }

        return result;
    }

    private int GetRawInt(char c)
    {
        if (Char.IsLetter(c))
        {
            return Char.ToUpper(c) - 'A' + 10;
        }
        return (int)Char.GetNumericValue(c);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EditSceneScript : GameSceneScript {

    public enum TerrainMode { NONE = -1, ERASE, INDESTRUCTIBLE, DESTRUCTIBLE};

    public bool editting { get; private set; }
    public TerrainMode currentMode;
    public List<Button> terrainButtons;
    public EditModeBuilding editModeBuilding;
    public ToggleButton toggleExpansions;
    public TextMeshProUGUI usedTechCounter;
    public bool overwriteMenuActive = false;
    public GameObject overwriteMenu;
    public GameObject savedText;

    protected bool overwrite = false;
    public bool hasExpansions { get; private set; }
    protected bool toLevelSelect;

    protected int touchID;

    // Use this for initialization
    void Start () {
        editting = false;
        TurnOffAllButtons();
        currentMode = TerrainMode.NONE;
    }

    internal override void OnEnter(TransitionData data)
    {
        hideSavedWord();
        Services.GameEventManager.Register<RefreshLevelSelectSceneEvent>(OnLevelSelectSceneRefresh);
        Time.timeScale = 1;
        Services.GameScene = this;
        tm = new TaskManager();
        Services.GameEventManager = new GameEventsManager();
        Services.UIManager = GetComponentInChildren<UIManager>();
        Services.GameManager.blueprintsEnabled = false;
        base.OnEnter(data);
        foreach(MapTile tile in Services.MapManager.Map)
        {
            tile.gameObject.AddComponent<BoxCollider2D>();
        }

        if (!Services.GameManager.levelSelected.isNewEditLevel())
        {
            hasExpansions = Services.GameManager.levelSelected.cornerBases;
            toggleExpansions.SetToggleImageColor(hasExpansions);
            overwrite = true;
        }
        else
        {
            hasExpansions = false;
        }
        Services.GameManager.InitPlayers(Services.GameManager.handicapValue, true);
        foreach(TechBuilding techBuilding in Services.MapManager.structuresOnMap)
        {
            if (techBuilding is EditModeBuilding)
            {
                ((EditModeBuilding)techBuilding).editModePlayer = Services.GameManager.Players[0];
            }
        }
        Services.MapManager.UpdateMapTileBrightness();
        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);
		touchID = -1;
    }

    internal override void OnExit()
    {
		Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
		Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);
        Services.GameEventManager.Unregister<RefreshLevelSelectSceneEvent>(OnLevelSelectSceneRefresh);
    }

    public void OnLevelSelectSceneRefresh(RefreshLevelSelectSceneEvent e)
    {
        //ReturnToLevelSelect();
    }

    public string GenerateName()
    {
        int levelNum = 1;
        string levelName = "custom " + levelNum;

        while (LevelManager.levelInfo.CustomLevelsContainName(levelName))
        {
            levelNum++;
            levelName = "custom " + levelNum;
        }
        return levelName;
    }

    public void viewSavedWord()
    {
        savedText.SetActive(true);
    }

    public void hideSavedWord()
    {
        savedText.SetActive(false);
    }

    public void SaveSuccessful()
    {
        Time.timeScale = 1;

        List<Task> saveTaskList = new List<Task>();
        Task viewSaveWordTask = new ActionTask(viewSavedWord);
        Wait wait = new Wait(1.5f);
        Task moveToMenu;

        saveTaskList.Add(viewSaveWordTask);
        saveTaskList.Add(wait);

        if (toLevelSelect)
        {
            moveToMenu = new ActionTask(LoadLevelSelect);
        }
        else
        {
            moveToMenu = new ActionTask(Reset);
        }
        saveTaskList.Add(moveToMenu);

        TaskQueue saveSequence = new TaskQueue(saveTaskList);


        tm.Do(saveSequence);
    }

    public void ShowOverwriteMenu(bool levelSelect)
    {
        toLevelSelect = levelSelect;
        if (overwrite)
        {
            Time.timeScale = 1;
            ToggleOverwriteMenu(false);
        }
        else
        {
            SaveSuccessful();
        }
    }

    public override void LoadLevelSelect()
    {
        Services.AudioManager.FadeOutLevelMusic();
        if (overwrite)
        {
            SaveMap(Services.GameManager.levelSelected.levelName);
        }
        else
        {
            SaveMap(GenerateName());
        }
        Time.timeScale = 1;
        Services.Scenes.Swap<LevelSelectSceneScript>();

    }

    public override void Reset()
    {
        if (overwrite)
        {
            SaveMap(Services.GameManager.levelSelected.levelName);
        }
        else
        {
            SaveMap(GenerateName());
        }
        Services.Analytics.MatchEnded();
        Services.AudioManager.FadeOutLevelMusicMainMenuCall();
        Services.GameManager.Reset(new Reset());
    }

    public void SaveMap(string name)
    {
        LevelData level;

        string levelName = name;
        int campaignLevelNum = 0;
        string[] objectives = new string[0];
        List<BuildingType> availableStructures = new List<BuildingType>(TechBuilding.techTypes);
        Coord p1HomeBasePos = new Coord(1, 1);
        Coord p2HomeBasePos = new Coord(18, 18);
        List<Coord> preplacedP1Tiles = new List<Coord>();
        List<Coord> preplacedP2Tiles = new List<Coord>();
        List<Coord> structCoords = new List<Coord>();
        List<Coord> indestructibleTerrainCoords = new List<Coord>();
        List<Coord> destructibleTerrainCoords = new List<Coord>();
        int width = Services.MapManager.MapWidth;
        int height = Services.MapManager.MapHeight;
        bool cornerBases = hasExpansions;
        bool destructorsEnabled = true;
        bool blueprintsEnabled = true;
        bool generatorEnabled = true;
        bool factoryEnabled = true;
        bool barracksEnabled = true;
        TooltipInfo[] tooltips = new TooltipInfo[0];
        AIStrategy overrideStrategy = null;
        foreach (MapTile tile in Services.MapManager.Map)
        {
            if(tile.occupyingPiece != null)
            {
                Polyomino piece = tile.occupyingPiece;
                if(piece.owner != null && piece.owner.playerNum == 1)
                {
                    if (piece is Base && ((Base)piece).mainBase)
                    {
                        p1HomeBasePos = piece.centerCoord;
                    }
                    else if(!preplacedP1Tiles.Contains(piece.centerCoord))
                    {
                        preplacedP1Tiles.Add(piece.centerCoord);
                    }
                }
                else if(piece.owner != null && piece.owner.playerNum == 2)
                {
                    if (piece is Base && ((Base)piece).mainBase)
                    {
                        p2HomeBasePos = piece.centerCoord;
                    }
                    else if (!preplacedP2Tiles.Contains(piece.centerCoord))
                    {
                        preplacedP2Tiles.Add(piece.centerCoord);
                    }
                }
                else if(piece.owner == null && !(piece is TechBuilding))
                {
                    if (piece.destructible)
                    {
                        if(!destructibleTerrainCoords.Contains(piece.centerCoord))
                            destructibleTerrainCoords.Add(piece.centerCoord);
                    }
                    else
                    {
                        if (!indestructibleTerrainCoords.Contains(piece.centerCoord))
                            indestructibleTerrainCoords.Add(piece.centerCoord);
                    }
                }
                else if(piece.owner == null && !(piece is Base))
                {
                    if (!structCoords.Contains(piece.centerCoord))
                        structCoords.Add(piece.centerCoord);
                }
            }
        }

        level = new LevelData(levelName, campaignLevelNum, objectives, availableStructures.ToArray(),
                                p1HomeBasePos, p2HomeBasePos, preplacedP1Tiles.ToArray(), preplacedP2Tiles.ToArray(),
                                structCoords.ToArray(), indestructibleTerrainCoords.ToArray(), destructibleTerrainCoords.ToArray(),
                                width, height, cornerBases, destructorsEnabled, blueprintsEnabled, generatorEnabled, factoryEnabled, barracksEnabled, 
                                false, tooltips, overrideStrategy);
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Edit)
        {
            if (overwrite)
            {
                LevelManager.OverwriteLevel(level);
            }
            else
            {
                LevelManager.AddLevel(level.levelName, level, true);
            }
        }
        else if (Services.GameManager.mode == TitleSceneScript.GameMode.DungeonEdit)
        {
            if (overwrite)
            {
                LevelManager.OverwriteLevel(level);
            }
            else
            {
                LevelManager.AddLevel(level.levelName, level, false, true);
            }
        }
    }

   

    public void ToggleOverwriteMenu(bool selectNo)
    {
        overwriteMenuActive = !overwriteMenuActive;
        overwriteMenu.SetActive(overwriteMenuActive);


        if (toLevelSelect && selectNo)
        {
            base.LoadLevelSelect();
        }
        else if (selectNo)
        {
            base.Reset();
        }
    }

    public void ToggleExpansions()
    {
        hasExpansions = !hasExpansions;
        if(hasExpansions)
        {
            Services.MapManager.MakeExpansions();
        }
        else
        {
            List<TechBuilding> expansionsToRemove = new List<TechBuilding>();
            foreach(TechBuilding tech in Services.MapManager.structuresOnMap)
            {
                if(tech is Base && !((Base)tech).mainBase)
                {
                    expansionsToRemove.Add(tech);
                    EraseTerrain(tech);
                }
            }

            foreach(TechBuilding tech in expansionsToRemove)
            {

                Services.MapManager.RemoveStructure(tech);
            }
        }
    }

    public void TurnOffAllButtons()
    {
        currentMode = TerrainMode.NONE;
        foreach(Button button in terrainButtons)
        {
            button.GetComponentsInChildren<Image>(true)[2].gameObject.SetActive(false);
        }
    }

    public void ChangeTerrainMode(int mode)
    {
        TurnOffAllButtons();
        currentMode = (TerrainMode)mode;
        terrainButtons[mode].GetComponentsInChildren<Image>(true)[2].gameObject.SetActive(true);
    }

    public void AddTerrain(MapTile tile, bool destructible)
    {
        Polyomino terrain = new Polyomino(1, 0, null, true, destructible);
        terrain.MakePhysicalPiece();
        terrain.ScaleHolder(Vector3.one);
        terrain.PlaceAtLocation(tile.coord, false, true);
    }

    public void ModifyTerrain(MapTile tile)
    {
        if (tile == null) return;
        switch (currentMode)
        {
            case TerrainMode.DESTRUCTIBLE:
                if(tile.occupyingPiece == null)
                {
                    AddTerrain(tile, true);
                }
                break;
            case TerrainMode.INDESTRUCTIBLE:
                if (tile.occupyingPiece == null)
                {
                    AddTerrain(tile, false);
                }
                break;
            case TerrainMode.NONE:
            default:
                break;
        }
    }

    public void EraseTerrain(Polyomino piece)
    {
        if (!Services.MapManager.IsCoordContainedInMap(piece.centerCoord)) return;
        if (!(piece is Base) || (piece is Base && !((Base)piece).mainBase && !hasExpansions))
            piece.Remove(true);    
    }

    public override void OnMouseDownEvent(MouseDown e)
    {
        Vector3 mouseWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos);
        OnInputDown(mouseWorldPos);
    }

    public override void OnTouchDown(TouchDown e)
    {
        Vector3 touchWorldPos =
            Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position);
		//touchID = e.touch.fingerId;
        OnInputDown(touchWorldPos);
    }


    public override void OnInputDown(Vector3 position)
    {
        Vector3 touchPos = new Vector3(position.x, position.y, 5);
        RaycastHit2D hit = Physics2D.Raycast(touchPos, -Vector2.up);
        if (hit.collider != null && !gamePaused)
        {
            Tile pieceTile = hit.transform.GetComponent<Tile>();
            MapTile mapTile = hit.transform.GetComponent<MapTile>();
            if ((pieceTile != null || mapTile.occupyingPiece != null) && currentMode == TerrainMode.ERASE)
            {
                if (pieceTile) EraseTerrain(pieceTile.pieceParent);
                else EraseTerrain(mapTile.occupyingPiece);
                
            }
            else if(hit.transform.name.Contains("Tile"))
            {
                ModifyTerrain(mapTile);
            } 
        }

        Services.GameEventManager.Register<TouchUp>(OnTouchUp);
        Services.GameEventManager.Register<TouchMove>(OnTouchMove);
        Services.GameEventManager.Unregister<TouchDown>(OnTouchDown);

        Services.GameEventManager.Register<MouseUp>(OnMouseUpEvent);
        Services.GameEventManager.Register<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Unregister<MouseDown>(OnMouseDownEvent);
    }

    public void OnMouseMoveEvent(MouseMove e)
    {
        OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.mousePos));
    }

    public void OnTouchMove(TouchMove e)
    {
        //if (e.touch.fingerId == touchID)
        //{
            OnInputDrag(Services.GameManager.MainCamera.ScreenToWorldPoint(e.touch.position));
        //}
    }


    public void OnInputDrag(Vector3 position)
    {
        Vector3 touchPos = new Vector3(position.x, position.y, 5);
        RaycastHit2D hit = Physics2D.Raycast(touchPos, -Vector2.up);
        if (hit.collider != null && !gamePaused)
        {
            Tile pieceTile = hit.transform.GetComponent<Tile>();
            MapTile mapTile = hit.transform.GetComponent<MapTile>();
            if ((pieceTile != null || mapTile.occupyingPiece != null) && currentMode == TerrainMode.ERASE)
            {
                if (pieceTile) EraseTerrain(pieceTile.pieceParent);
                else EraseTerrain(mapTile.occupyingPiece);

            }
            else if (hit.transform.name.Contains("Tile"))
            {
                ModifyTerrain(mapTile);
            }
        }
    }

    public void OnTouchUp(TouchUp e)
    {
        //if (e.touch.fingerId == touchID)
        //{
            OnInputUp();            
        //}
    }

    public void OnMouseUpEvent(MouseUp e)
    {
        OnInputUp();
    }

    public void OnInputUp()
    {
        //touchID = -1;
        if(Services.GameManager.Players[0].selectedPiece is EditModeBuilding)
        {
            EraseTerrain(Services.GameManager.Players[0].selectedPiece);
        }

        Services.GameEventManager.Register<MouseDown>(OnMouseDownEvent);
        Services.GameEventManager.Register<TouchDown>(OnTouchDown);

        Services.GameEventManager.Unregister<TouchMove>(OnTouchMove);
        Services.GameEventManager.Unregister<TouchUp>(OnTouchUp);

        Services.GameEventManager.Unregister<MouseMove>(OnMouseMoveEvent);
        Services.GameEventManager.Unregister<MouseUp>(OnMouseUpEvent);
    }



    public override void StartGameSequence()
    {
        TaskTree startSequence = new TaskTree(new ScrollReadyBanners(Services.UIManager.UIBannerManager.readyBanners, false, true));
        TaskTree uiEntry;
        TaskTree handEntry;

        uiEntry =
            new TaskTree(new EmptyTask(),
                new TaskTree(
                    new EditModeUIEntryAnimation(editModeBuilding, terrainButtons, toggleExpansions.gameObject)));
        handEntry =
            new TaskTree(new EmptyTask(),
                new TaskTree(
                    new HandPieceEntry(Services.GameManager.Players[0].hand)));

        startSequence
            .Then(uiEntry)
            .Then(handEntry)
            .Then(new ActionTask(StartGame));

        editting = true;

        Services.GameScene.tm.Do(startSequence);
    }

    // Update is called once per frame
    void Update () {
        tm.Update();
        
	}
}

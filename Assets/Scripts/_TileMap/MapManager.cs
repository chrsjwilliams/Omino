using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour
{ 
	[SerializeField] private int _mapWidth;
    public int MapWidth
    {
        get { return _mapWidth; }
    }

  	[SerializeField] private int _mapHeight;
    public int MapHeight
    {
        get { return _mapHeight; }
    }

	[SerializeField] private Tile[,] _map;
    public Tile[,] Map
    {
        get { return _map; }
    }

    [SerializeField] private static IntVector2 _center;
    public static IntVector2 Center
    {
        get { return _center; }
    }

    public List<TechBuilding> structuresOnMap { get; private set; }
    public List<Coord> structureCoords { get; private set; }
    [SerializeField]
    private int structDistMin;
    [SerializeField]
    private int structRadiusMin;
    [SerializeField]
    private int resourceBorderMin;
    [SerializeField]
    private int startingStructCount;
    [SerializeField]
    private int procGenTriesMax;
    [SerializeField]
    private Level[] levels;
    public Level[] campaignLevels;
    public Level currentLevel
    {
        get
        {
            return Services.GameManager.levelSelected;
        }
    }
    [SerializeField]
    private Level[] eloLevelPool;
    [SerializeField]
    private Level[] dungeonRunLevelPool;
    public void Init()
    {
        _center = Services.MapManager.CenterIndexOfGrid();
    }

    public void GenerateMap()
    {
        if (Services.GameManager.mode == TitleSceneScript.GameMode.Elo)
        {
            Level level = eloLevelPool[Random.Range(0, eloLevelPool.Length)];
            Services.GameManager.SetCurrentLevel(level);
        }
        if(Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            Level level = dungeonRunLevelPool[Random.Range(0, dungeonRunLevelPool.Length -1)];
            Services.GameManager.SetCurrentLevel(level);
        }
        if (Services.GameManager.levelSelected != null)
        {
            Level level = currentLevel;
            _mapWidth = level.width;
            _mapHeight = level.height;
        }
        else
        {
            _mapWidth = 20;
            _mapHeight = 20;
        }
        Services.GameData.totalMapTiles = MapWidth * MapHeight;
        for (int i = 0; i < 2; i++)
        {
            Services.GameData.filledMapTiles[i] = 0;
            Services.GameData.distancesToOpponentBase[i] = MapWidth + MapHeight - 8;
        }

        Services.CameraController.SetPosition(
            new Vector3((MapWidth - 1) / 2f, (MapHeight - 1) / 2f, -10));
        Services.GameScene.backgroundImage.transform.position = 
            new Vector3((MapWidth - 1) / 2f, (MapHeight - 1) / 2f, 0);

        _map = new Tile[MapWidth, MapHeight];
        for (int i = 0; i < MapWidth; i++)
        {
            for (int j = 0; j < MapHeight; j++)
            {
                Tile tile = Instantiate(Services.Prefabs.Tile, 
                    GameSceneScript.tileMapHolder).GetComponent<Tile>();
                
                tile.Init(new Coord(i, j));
                _map[i, j] = tile;
                tile.name = "Tile [X: " + i + ", Y: " + j + "]";
                tile.SetHighlightStatus(false);
            }
        }
        if(Services.GameManager.levelSelected == null)
        {
            GenerateStructures(null);
        }
        else
        {
            GenerateStructures(Services.GameManager.levelSelected);
        }
        foreach (Tile mapTile in Map)
        {
            mapTile.gameObject.SetActive(false);
        }
        foreach(TechBuilding structure in structuresOnMap)
        {
            structure.holder.gameObject.SetActive(false);
        }
        TaskQueue boardAnimationTasks = new TaskQueue(new List<Task>() {
            new Wait(0.3f),
            new BoardEntryAnimation(),
            new InitialBuildingEntryAnimation(),
            new ScrollReadyBanners(Services.UIManager.readyBanners, true)
            });

        Services.GameScene.tm.Do(boardAnimationTasks);
    }

    TechBuilding GenerateStructure(BuildingType type)
    {
        return GenerateStructure(type, GenerateValidStructureCoord());
    }

    TechBuilding GenerateStructure(BuildingType type, Coord structCoord)
    {
        if (structCoord != new Coord(-1, -1))
        {
            TechBuilding structure;
            switch (type)
            {
                case BuildingType.BASE:
                    structure = new Base();
                    break;
                case BuildingType.DYNAMO:
                    structure = new Dynamo();
                    break;
                case BuildingType.SUPPLYBOOST:
                    structure = new SupplyBoost();
                    break;
                case BuildingType.UPSIZE:
                    structure = new Upsize();
                    break;
                case BuildingType.ATTACKUPSIZE:
                    structure = new AttackUpsize();
                    break;
                case BuildingType.COMBUSTION:
                    structure = new Combustion();
                    break;
                case BuildingType.SHIELDEDPIECES:
                    structure = new ShieldedPieces();
                    break;
                case BuildingType.ARMORY:
                    structure = new Armory();
                    break;
                case BuildingType.FISSION:
                    structure = new Fission();
                    break;
                case BuildingType.RECYCLING:
                    structure = new Recycling();
                    break;
                case BuildingType.CROSSSECTION:
                    structure = new CrossSection();
                    break;
                default:
                    return null;
            }
            structure.MakePhysicalPiece();
            structure.PlaceAtLocation(structCoord);
            return structure;
        }
        return null;
    }

    void GetStructureCoords()
    {
        structureCoords = new List<Coord>();
        foreach (TechBuilding structure in Services.MapManager.structuresOnMap)
        {
            foreach (Tile tile in structure.tiles)
            {
                if (!structureCoords.Contains(tile.coord))
                {
                    structureCoords.Add(tile.coord);
                }
            }
        }
    }

    Coord GenerateValidStructureCoord(bool mirrored = false)
    {
        Coord nullCoord = new Coord(-1, -1);
        for (int i = 0; i < procGenTriesMax; i++)
        {
            Coord candidateCoord = GenerateRandomCoord();
            Coord mirroredCoord = MirroredCoord(candidateCoord);
            if (IsStructureCoordValid(candidateCoord) &&
                (!mirrored || (IsStructureCoordValid(mirroredCoord) &&
                candidateCoord.Distance(mirroredCoord) >= structDistMin)))
                return candidateCoord;
        }
        return nullCoord;
    }

    Coord MirroredCoord(Coord coord)
    {
        return new Coord((MapWidth - 1) - coord.x, (MapHeight - 1) - coord.y);
    }

    bool IsStructureCoordValid(Coord candidateCoord)
    {
        if (candidateCoord.Distance(new Coord(0, 0)) < structRadiusMin ||
            candidateCoord.Distance(new Coord(MapWidth - 1, MapHeight - 1))
            < structRadiusMin)
            return false;
        for (int i = 0; i < structuresOnMap.Count; i++)
        {
            if (candidateCoord.Distance(structuresOnMap[i].centerCoord) < structDistMin)
                return false;
        }
        return true;
    }

    List<BuildingType> InitStructureTypeList()
    {
        return new List<BuildingType>()
        {
            BuildingType.DYNAMO,
            BuildingType.SUPPLYBOOST,
            BuildingType.UPSIZE,
            //BuildingType.BIGGERBOMBS,
            //BuildingType.SPLASHDAMAGE,
            BuildingType.SHIELDEDPIECES,
            BuildingType.ARMORY,
            BuildingType.FISSION,
            BuildingType.RECYCLING,
            //BuildingType.CROSSSECTION
        };
    }

    void GenerateStructures(Level level)
    {
        structuresOnMap = new List<TechBuilding>();
        List<BuildingType> structureTypes = InitStructureTypeList();

        if (level == null || level.cornerBases)
        {
            for (int j = 0; j < 2; j++)
            {
                Base cornerBase = new Base();
                cornerBase.MakePhysicalPiece();
                Coord location;
                if (j == 0)
                {
                    location = new Coord(MapWidth - 2, 1);
                }
                else
                {
                    location = new Coord(0, MapHeight - 1);
                }
                cornerBase.PlaceAtLocation(location);
                structuresOnMap.Add(cornerBase);
            }
        }

        if (level == null) // use procedural generation if no supplied level
        {
            for (int i = 0; i < startingStructCount; i++)
            {
                BuildingType type;
                if (structureTypes.Count == 0) structureTypes = InitStructureTypeList();
                type = structureTypes[Random.Range(0, structureTypes.Count)];
                structureTypes.Remove(type);
                TechBuilding structure = GenerateStructure(type);
                if (structure == null)
                {
                    break;
                }
                structuresOnMap.Add(structure);
            }
        }
        else GenerateLevel(level);

        GetStructureCoords();
    }

    List<BuildingType> RemoveBuildingTypesFromTechPool(BuildingType[] availableTech, List<BuildingType> techToRemove)
    {
        List<BuildingType> currentList = new List<BuildingType>(availableTech);

        foreach(BuildingType type in techToRemove)
        {
            if (currentList.Contains(type))
            {
                currentList.Remove(type);
            }
        }

        return currentList;
    }

    void GenerateLevel(Level level)
    {
        List<BuildingType> structureTypes;
        if (Services.GameManager.mode == TitleSceneScript.GameMode.DungeonRun)
        {
            structureTypes = RemoveBuildingTypesFromTechPool(level.availableStructures,
                                                                     DungeonRunManager.dungeonRunData.currentTech);
        }
        else
        {
            structureTypes = new List<BuildingType>(level.availableStructures);
        }

        for (int i = 0; i < level.structCoords.Length; i++)
        {
            BuildingType type;
            if (structureTypes.Count == 0)
                structureTypes = new List<BuildingType>(level.availableStructures);
            type = structureTypes[Random.Range(0, structureTypes.Count)];
            structureTypes.Remove(type);
            TechBuilding structure = GenerateStructure(type, level.structCoords[i]);
            structuresOnMap.Add(structure);
        }
    }

    Coord GenerateRandomCoord()
    {
        return new Coord(Random.Range(resourceBorderMin, MapWidth - resourceBorderMin),
            Random.Range(resourceBorderMin, MapHeight - resourceBorderMin));
    }

    public IntVector2 CenterIndexOfGrid()
    {
        return new IntVector2(MapWidth / 2, MapHeight / 2);
    }

    public void CreateMainBase(Player player, Coord coord)
    {
        Base playerBase = new Base(player, true);
        playerBase.MakePhysicalPiece();
        playerBase.PlaceAtLocation(coord);
        playerBase.TogglePieceConnectedness(true);
        player.AddBase(playerBase);
    }

    public Tile GetRandomTile()
    {
        return _map[Random.Range(0, MapWidth), Random.Range(0, MapHeight)];
    }

    public Tile GetRandomEmptyTile()
    {
        Tile randomTile = GetRandomTile();

        return randomTile;
    }

    public bool IsCoordContainedInMap(Coord coord)
    {
        return coord.x >= 0 && coord.x < MapWidth &&
                coord.y >= 0 && coord.y < MapHeight;
    }

    public bool ValidateTile(Tile tile, Player owner)
    {
        foreach(Coord direction in Coord.Directions())
        {
            Coord adjacentCoord = tile.coord.Add(direction);
            if (IsCoordContainedInMap(adjacentCoord))
            {
                if (Map[adjacentCoord.x, adjacentCoord.y].IsOccupied() &&
                    Map[adjacentCoord.x, adjacentCoord.y].occupyingPiece.owner == owner)
                    return true;
            }
        }
        return false;
    }

    public bool ValidateEyeProperty(Tile tile, Player player)
    {
        bool isValidEye = false;
        foreach(Coord direction in Coord.Directions())
        {
            Coord adjacentCoord = tile.coord.Add(direction);
            if(IsCoordContainedInMap(adjacentCoord))
            {
                Tile adjacentTile = Map[adjacentCoord.x, adjacentCoord.y];
                if (adjacentTile.IsOccupied() &&
                    adjacentTile.occupyingPiece.buildingType != BuildingType.BASE &&
                    adjacentTile.occupyingPiece.owner == player)
                {
                    isValidEye = true;
                }
                else
                {
                    return false;
                }
            }
        }

        return isValidEye;
    }

    public void DetermineConnectedness(Player player)
    {
        Base mainBase = player.mainBase;
        List<Polyomino> connectedPieces = new List<Polyomino>();
        HashSet<Polyomino> frontier = new HashSet<Polyomino>();
        connectedPieces.Add(mainBase);
        frontier.UnionWith(mainBase.GetAdjacentPolyominos());
        int iterationCount = 0;
        while (frontier.Count > 0)
        {
            iterationCount += 1;
            if (iterationCount > 1000) return;
            HashSet<Polyomino> frontierQueue = new HashSet<Polyomino>();
            Polyomino piece = frontier.First();
            if ((piece is TechBuilding && (piece.owner == player || piece.owner == null)) || 
                piece.owner == player)
            {
                connectedPieces.Add(piece);
                List<Polyomino> adjacentPieces = piece.GetAdjacentPolyominos();
                for (int j = 0; j < adjacentPieces.Count; j++)
                {
                    Polyomino adjacentPiece = adjacentPieces[j];
                    if (!connectedPieces.Contains(adjacentPiece))
                    {
                        frontierQueue.Add(adjacentPiece);
                    }
                }
            }
            frontier.Remove(piece);
            frontier.UnionWith(frontierQueue);
        }

        for (int i = player.boardPieces.Count - 1; i >= 0; i--)
        {
            Polyomino piece = player.boardPieces[i];
            if (!connectedPieces.Contains(piece) && !(piece is Blueprint))
            {
                if(piece is TechBuilding)
                {
                    TechBuilding structure = piece as TechBuilding;
                    structure.OnClaimLost();
                }
                else
                {
                    piece.TogglePieceConnectedness(false);
                }
            }
        }
        int minDistToOpponentBase = int.MaxValue;
        Coord opposingBaseCoord = Services.GameManager.Players[player.playerNum % 2].mainBase.centerCoord;
        for (int i = 0; i < connectedPieces.Count; i++)
        {
            Polyomino piece = connectedPieces[i];
            if ((!piece.connected && !(piece is TechBuilding)) || 
                (piece is TechBuilding && piece.owner == null))
            {
                if(piece is TechBuilding)
                {
                    TechBuilding structure = piece as TechBuilding;
                    structure.OnClaim(player);
                }
                else
                {
                    piece.TogglePieceConnectedness(true);
                }
            }
            int dist = piece.centerCoord.Distance(opposingBaseCoord);
            if (dist < minDistToOpponentBase)
            {
                minDistToOpponentBase = dist;
            }
        }
        Services.GameData.distancesToOpponentBase[player.playerNum - 1] = minDistToOpponentBase;
    }

	public bool ConnectedToBase(Polyomino piece, List<Polyomino> checkedPieces)
    {
        if (piece.buildingType == BuildingType.BASE) return true;

        checkedPieces.Add(piece);

        List<Polyomino> piecesToCheck = new List<Polyomino>();
        foreach (Tile tile in piece.tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (IsCoordContainedInMap(adjacentCoord))
                {
                    Tile adjTile = Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.IsOccupied() &&
                        adjTile.occupyingPiece.owner == piece.owner &&
                        adjTile.occupyingPiece != piece &&
                        adjTile.occupyingPiece.buildingType == BuildingType.BASE)
                    {
                        return true;
                    }
                    else
                    {
                        if (adjTile.occupyingPiece != null)
                        {
                            if (!checkedPieces.Contains(adjTile.occupyingPiece) &&
                                !piecesToCheck.Contains(adjTile.occupyingPiece) &&
                                adjTile.occupyingPiece.owner == piece.owner)
                            {
                                piecesToCheck.Add(adjTile.occupyingPiece);
                            }
                        }
                    }
                }
            }
        }
        for (int i = 0; i < piecesToCheck.Count; i++)
        {
            if (ConnectedToBase(piecesToCheck[i], checkedPieces)) return true;
        }
        return false;
    }

    public bool CheckForWin(Polyomino piece)
    {
        foreach (Tile tile in piece.tiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (IsCoordContainedInMap(adjacentCoord))
                {
                    Tile adjTile = Map[adjacentCoord.x, adjacentCoord.y];
                    if (adjTile.IsOccupied() &&
                        adjTile.occupyingPiece.owner != null &&
                        adjTile.occupyingPiece.owner != piece.owner &&
                        adjTile.occupyingPiece.buildingType == BuildingType.BASE)
                    {
                        Base occupyingBase = adjTile.occupyingPiece as Base;
                        if(occupyingBase.mainBase)
                            return true;
                    }
                }
            }
        }
        return false;
    }

    public Level GetNextLevel()
    {
        Level nextLevel = null;
        for (int i = 0; i < campaignLevels.Length; i++)
        {
            Level level = campaignLevels[i];
            if(level == currentLevel && i < campaignLevels.Length -1)
            {
                nextLevel = campaignLevels[i + 1];
                break;
            }
        }
        return nextLevel;
    }
}

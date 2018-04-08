using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public List<Structure> structuresOnMap { get; private set; }
    public List<Coord> structureCoords { get; private set; }
    [SerializeField]
    private int resourceDistMin;
    [SerializeField]
    private int resourceRadiusMin;
    [SerializeField]
    private int structDistMin;
    [SerializeField]
    private int structRadiusMin;
    [SerializeField]
    private int resourceBorderMin;
    [SerializeField]
    private int resourceSizeMin;
    [SerializeField]
    private int resourceSizeMax;
    [SerializeField]
    private int startingResourceCount;
    [SerializeField]
    private int startingStructCount;
    [SerializeField]
    private int procGenTriesMax;
    [SerializeField]
    private Level[] levels;

    public void Init()
    {
        _center = Services.MapManager.CenterIndexOfGrid();
    }

    public void GenerateMap()
    {
        if(Services.GameManager.levelSelected != 0)
        {
            Level level = levels[Services.GameManager.levelSelected - 1];
            _mapWidth = level.width;
            _mapHeight = level.height;
        }

        Camera.main.transform.position = 
            new Vector3((MapWidth - 1) / 2f, (MapHeight - 1) / 2f, -10);
        Services.GameScene.backgroundImage.transform.position = 
            new Vector3((MapWidth - 1) / 2f, (MapHeight - 1) / 2f, 0);


        //Debug.Log("width " + MapWidth + ", height " + MapHeight);

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
                //tile.SetMaskSrAlpha(0);
                tile.SetHighlightStatus(false);
            }
        }
        if(Services.GameManager.levelSelected == 0)
        {
            GenerateStructures(null);
        }
        else
        {
            GenerateStructures(levels[Services.GameManager.levelSelected - 1]);
        }
    }

    Structure GenerateStructure(BuildingType type)
    {
        return GenerateStructure(type, GenerateValidStructureCoord());
    }

    Structure GenerateStructure(BuildingType type, Coord structCoord)
    {
        if (structCoord != new Coord(-1, -1))
        {
            //int numRotations = Random.Range(0, 4);
            Structure structure;
            switch (type)
            {
                case BuildingType.BASE:
                    structure = new Base();
                    break;
                case BuildingType.MININGDRILL:
                    structure = new MiningDrill();
                    break;
                case BuildingType.ASSEMBLYLINE:
                    structure = new AssemblyLine();
                    break;
                case BuildingType.FORTIFIEDSTEEL:
                    structure = new FortifiedSteel();
                    break;
                case BuildingType.BIGGERBRICKS:
                    structure = new BiggerBricks();
                    break;
                case BuildingType.BIGGERBOMBS:
                    structure = new BiggerBombs();
                    break;
                case BuildingType.SPLASHDAMAGE:
                    structure = new SplashDamage();
                    break;
                case BuildingType.SHIELDEDPIECES:
                    structure = new ShieldedPieces();
                    break;
                default:
                    return null;
            }
            structure.MakePhysicalPiece();
            //for (int i = 0; i < numRotations; i++)
            //{
            //    structure.Rotate();
            //}
            structure.PlaceAtLocation(structCoord);
            return structure;
        }
        return null;
    }

    void GetStructureCoords()
    {
        structureCoords = new List<Coord>();
        foreach (Structure structure in Services.MapManager.structuresOnMap)
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

    Coord GenerateValidStructureCoord()
    {
        return GenerateValidStructureCoord(false);
    }

    Coord GenerateValidStructureCoord(bool mirrored)
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
            BuildingType.MININGDRILL,
            BuildingType.ASSEMBLYLINE,
            //BuildingType.FORTIFIEDSTEEL,
            BuildingType.BIGGERBRICKS,
            BuildingType.BIGGERBOMBS,
            BuildingType.SPLASHDAMAGE,
            BuildingType.SHIELDEDPIECES
        };
    }

    void GenerateStructures(Level level)
    {
        structuresOnMap = new List<Structure>();
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
                //List<Structure> structures = new List<Structure>();
                BuildingType type;
                if (structureTypes.Count == 0) structureTypes = InitStructureTypeList();
                type = structureTypes[Random.Range(0, structureTypes.Count)];
                structureTypes.Remove(type);
                Structure structure = GenerateStructure(type);
                if (structure == null)
                {
                    break;
                }
                //foreach (Structure structure in structures)
                //{
                structuresOnMap.Add(structure);
                //}
            }
        }
        else GenerateLevel(level);

        GetStructureCoords();
    }

    void GenerateLevel(Level level)
    {
        List<BuildingType> structureTypes = new List<BuildingType>(level.availableStructures);
        for (int i = 0; i < level.structCoords.Length; i++)
        {
            BuildingType type;
            if (structureTypes.Count == 0)
                structureTypes = new List<BuildingType>(level.availableStructures);
            type = structureTypes[Random.Range(0, structureTypes.Count)];
            structureTypes.Remove(type);
            Structure structure = GenerateStructure(type, level.structCoords[i]);
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
        //playerBase.ShiftColor(player.ColorScheme[0]);
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
        List<Polyomino> frontier = new List<Polyomino>();
        connectedPieces.Add(mainBase);
        frontier.AddRange(mainBase.GetAdjacentPolyominos());
        while (frontier.Count > 0)
        {
            List<Polyomino> frontierQueue = new List<Polyomino>();
            for (int i = frontier.Count - 1; i >= 0; i--)
            {
                Polyomino piece = frontier[i];
                if (!connectedPieces.Contains(piece) && 
                    ((piece is Structure && 
                    (piece.owner == player || piece.owner == null)) || piece.owner == player))
                {
                    connectedPieces.Add(piece);
                    List<Polyomino> adjacentPieces = piece.GetAdjacentPolyominos();
                    for (int j = 0; j < adjacentPieces.Count; j++)
                    {
                        Polyomino adjacentPiece = adjacentPieces[j];
                        if (!frontier.Contains(adjacentPiece) && 
                            !frontierQueue.Contains(adjacentPiece))
                            frontierQueue.Add(adjacentPiece);
                    }
                }
                frontier.Remove(piece);
            }
            frontier.AddRange(frontierQueue);
        }

        for (int i = player.boardPieces.Count - 1; i >= 0; i--)
        {
            Polyomino piece = player.boardPieces[i];
            if (!connectedPieces.Contains(piece) && !(piece is Blueprint))
            {
                if(piece is Structure)
                {
                    Structure structure = piece as Structure;
                    structure.OnClaimLost();
                }
                else
                {
                    piece.TogglePieceConnectedness(false);
                }
            }
        }
        for (int i = 0; i < connectedPieces.Count; i++)
        {
            Polyomino piece = connectedPieces[i];
            if ((!piece.connected && !(piece is Structure)) || 
                (piece is Structure && piece.owner == null))
            {
                if(piece is Structure)
                {
                    Structure structure = piece as Structure;
                    structure.OnClaim(player);
                }
                else
                {
                    piece.TogglePieceConnectedness(true);
                }
            }
        }
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

    public bool CheckForFortification(Polyomino piece, List<Tile> emptyTiles, bool isBeingDestroyed)
    {
        List<Polyomino> piecesToFortify = new List<Polyomino>();

        foreach (Tile tile in emptyTiles)
        {
            foreach (Coord direction in Coord.Directions())
            {
                Coord adjacentCoord = tile.coord.Add(direction);
                if (IsCoordContainedInMap(adjacentCoord))
                {
                    Tile adjacentTile = Map[adjacentCoord.x, adjacentCoord.y];

                    if (ValidateEyeProperty(tile, piece.owner) && !piece.isFortified 
                        && !piecesToFortify.Contains(adjacentTile.occupyingPiece) &&
                        !(adjacentTile.occupyingPiece is Structure))
                    {
                        piecesToFortify.Add(adjacentTile.occupyingPiece);
                    }
                    else
                    {
                        if (adjacentTile.IsOccupied() && isBeingDestroyed && adjacentTile.occupyingPiece.owner != piece.owner)
                        {
                            //adjacentTile.occupyingPiece.isFortified = false;
                            //adjacentTile.occupyingPiece.ToggleAltColor(false);
                           // adjacentTile.occupyingPiece.CheckForFortification(false);
                        }
                    }
                }
            }
        }

        if(piecesToFortify.Count > 0 && !piecesToFortify.Contains(piece) && !(piece is Structure))
        {
            piecesToFortify.Add(piece);
        }
        foreach(Polyomino polyomino in piecesToFortify)
        {
            FortifyPiece(polyomino);
        }

        return piecesToFortify.Count > 0;
    }

    public void FortifyPiece(Polyomino piece)
    {
        List<Tile> tempTiles = piece.tiles;
        List<Blueprint> prevOccupyingStructures = piece.occupyingBlueprints;
        piece.Remove(true);
        List<Polyomino> monominos = new List<Polyomino>();
        foreach (Tile tile in tempTiles)
        {
            Polyomino fortifiedMonomino = new Polyomino(1, 0, piece.owner);
            fortifiedMonomino.isFortified = true;
            fortifiedMonomino.MakePhysicalPiece();
            fortifiedMonomino.PlaceAtLocation(tile.coord, true);
            monominos.Add(fortifiedMonomino);

            for (int i = 0; i < prevOccupyingStructures.Count; i++)
            {
                foreach(Tile structTile in prevOccupyingStructures[i].tiles)
                {
                    if (structTile.coord == tile.coord)
                    {
                        fortifiedMonomino.AddOccupyingBlueprint(prevOccupyingStructures[i]);
                        break;
                    }
                }
            }
            //fortifiedMonomino.ToggleAltColor(true);
        }
        foreach(Polyomino monomino in monominos)
        {
            monomino.adjacentPieces = 
                monomino.GetAdjacentPolyominos(monomino.owner);
            foreach(Polyomino adjPiece in monomino.adjacentPieces)
            {
                if(!adjPiece.adjacentPieces.Contains(monomino))
                    adjPiece.adjacentPieces.Add(monomino);
            }
        }
    }
}

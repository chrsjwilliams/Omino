using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Level")]
public class Level : ScriptableObject
{
    public string levelName;
    public Sprite displayImage;
    public int campaignLevelNum;
    [TextArea]
    public string[] objectives;
    public BuildingType[] availableStructures;
    public Coord p1HomeBasePos;
    public Coord p2HomeBasePos;
    public Coord[] preplacedP1Tiles;
    public Coord[] preplacedP2Tiles;
    public Coord[] structCoords;
    public Coord[] impassibleCoords;
    public Coord[] destructibleTerrainCoords;
    public int width;
    public int height;
    public bool cornerBases;
    public bool destructorsEnabled = true;
    public bool blueprintsEnabled = true;
    public bool generatorEnabled = true;
    public bool factoryEnabled = true;
    public bool barracksEnabled = true;
    public bool stackDestructorInOpeningHand;
    public TooltipInfo[] tooltips;
    public AIStrategy overrideStrategy;
    public LevelData data;

    private bool newEditLevel = false;

    public void SetLevelData()
    {
        data = new LevelData(   levelName, campaignLevelNum, objectives, availableStructures,
                                p1HomeBasePos, p2HomeBasePos, preplacedP1Tiles, preplacedP2Tiles,
                                structCoords, impassibleCoords, destructibleTerrainCoords,
                                width, height, cornerBases, destructorsEnabled,
                                blueprintsEnabled, generatorEnabled, factoryEnabled,
                                barracksEnabled, stackDestructorInOpeningHand,
                                tooltips, overrideStrategy);
    }

    public void SetLevelData(int _width, int _height)
    {
        width = _width;
        height = _height;
        newEditLevel = true;
        p1HomeBasePos = new Coord(1, 1);
        p2HomeBasePos = new Coord( width - 2, height - 2);
        availableStructures = new BuildingType[0];
        structCoords = new Coord[0];
        cornerBases = true;
    }

    public bool isNewEditLevel() { return newEditLevel; }
    public void setOverwriteMode() { newEditLevel = false; }
}

[Serializable]
public class LevelData
{
    public string levelName;
    public int campaignLevelNum;
    public string[] objectives;
    public BuildingType[] availableStructures;
    public Coord p1HomeBasePos;
    public Coord p2HomeBasePos;
    public Coord[] preplacedP1Tiles;
    public Coord[] preplacedP2Tiles;
    public Coord[] structCoords;
    public Coord[] impassibleCoords;
    public Coord[] destructibleTerrainCoords;
    public int width;
    public int height;
    public bool cornerBases;
    public bool destructorsEnabled = true;
    public bool blueprintsEnabled = true;
    public bool generatorEnabled = true;
    public bool factoryEnabled = true;
    public bool barracksEnabled = true;
    public bool stackDestructorInOpeningHand;
    public TooltipInfo[] tooltips;
    public AIStrategy overrideStrategy;

    public LevelData(   string _levelName, int _campaignLevelNum, string[] _objectives,
                        BuildingType[] _availableStructures, Coord _p1HomeBasePos,
                        Coord _p2HomeBasePos, Coord[] _preplacedP1Tiles,
                        Coord[] _preplacedP2Tiles, Coord[] _structCoords, 
                        Coord[] _impassibleCoords, Coord[] _destructibleTerrainCoords,
                        int _width, int _height, bool _cornerExpansions,
                        bool _destructorsEnabled, bool _blueprintsEnabled,
                        bool _generatorEnabled, bool _factoryEnabled, bool _barracksEnabled,
                        bool _stackDestrcutor, TooltipInfo[] _tooltips, AIStrategy aiStrategy)
    {
        levelName = _levelName;
        campaignLevelNum = _campaignLevelNum;
        objectives = _objectives;
        availableStructures = _availableStructures;
        p1HomeBasePos = _p1HomeBasePos;
        p2HomeBasePos = _p2HomeBasePos;
        preplacedP1Tiles = _preplacedP1Tiles;
        preplacedP2Tiles = _preplacedP2Tiles;
        structCoords = _structCoords;
        impassibleCoords = _impassibleCoords;
        destructibleTerrainCoords = _destructibleTerrainCoords;
        width = _width;
        height = _height;
        cornerBases = _cornerExpansions;
        destructorsEnabled = _destructorsEnabled;
        blueprintsEnabled = _blueprintsEnabled;
        generatorEnabled = _generatorEnabled;
        factoryEnabled = _factoryEnabled;
        barracksEnabled = _barracksEnabled;
        stackDestructorInOpeningHand = _stackDestrcutor;
        tooltips = _tooltips;
        overrideStrategy = aiStrategy;
    }

    public void PrintMap()
    {
        Debug.Log("P1 Base: " + p1HomeBasePos.ToString());
        Debug.Log("P2 Base: " + p2HomeBasePos.ToString());

        Debug.Log("-------------------------------------------------------");

        Debug.Log("Preplaced P1 Tiles");
        for(int i = 0; i < preplacedP1Tiles.Length; i++)
        {
            Debug.Log(preplacedP1Tiles[i].ToString());
        }

        Debug.Log("-------------------------------------------------------");

        Debug.Log("Preplaced P2 Tiles");
        for (int i = 0; i < preplacedP2Tiles.Length; i++)
        {
            Debug.Log(preplacedP2Tiles[i].ToString());
        }

        Debug.Log("-------------------------------------------------------");

        Debug.Log("Indestructible Tiles");
        for (int i = 0; i < impassibleCoords.Length; i++)
        {
            Debug.Log(impassibleCoords[i].ToString());
        }

        Debug.Log("-------------------------------------------------------");

        Debug.Log("Destructible Tiles");
        for (int i = 0; i < destructibleTerrainCoords.Length; i++)
        {
            Debug.Log(destructibleTerrainCoords[i].ToString());
        }

        Debug.Log("-------------------------------------------------------");

        Debug.Log("Struct Coords");
        for (int i = 0; i < structCoords.Length; i++)
        {
            Debug.Log(structCoords[i].ToString());
        }

        Debug.Log("-------------------------------------------------------");

    }

    public Level CreateLevel()
    {
        Level level = ScriptableObject.CreateInstance<Level>();
        level.levelName = levelName;
        level.displayImage = Services.LevelDataLibrary.GetLevelImage(levelName);
        level.campaignLevelNum = campaignLevelNum;
        level.objectives = objectives;
        level.availableStructures = availableStructures;
        level.p1HomeBasePos = p1HomeBasePos;
        level.p2HomeBasePos = p2HomeBasePos;
        level.preplacedP1Tiles = preplacedP1Tiles;
        level.preplacedP2Tiles = preplacedP2Tiles;
        level.structCoords = structCoords;
        level.impassibleCoords = impassibleCoords;
        level.destructibleTerrainCoords = destructibleTerrainCoords;
        level.width = width;
        level.height = height;
        level.cornerBases = cornerBases;
        level.destructorsEnabled = destructorsEnabled;
        level.blueprintsEnabled = blueprintsEnabled;
        level.generatorEnabled = generatorEnabled;
        level.factoryEnabled = factoryEnabled;
        level.barracksEnabled = barracksEnabled;
        level.stackDestructorInOpeningHand = stackDestructorInOpeningHand;
        level.tooltips = tooltips;
        level.overrideStrategy = overrideStrategy;
        return level;
    }
}

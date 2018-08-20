using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public TextAsset levelData;
    public Texture2D map;

    private List<Coord> prePlacedP1Tiles = new List<Coord>();
    private List<Coord> prePlacedP2Tiles = new List<Coord>();
    private List<Coord> structCoords = new List<Coord>();
    private List<Coord> impassableCoords = new List<Coord>();
    private List<Coord> destructibleTerrainCoords = new List<Coord>();

    public Level level { get; private set; }

    // Use this for initialization
    void Awake ()
    {
		
	}

    public void Init(TextAsset _levelData, Texture2D _map)
    {
        levelData = _levelData;
        map = _map;

        level = ScriptableObject.CreateInstance<Level>();

        ParseMapData();
        List<string> textData = ParseTextAsset(levelData);
        SetLevelData(textData);
    }

    private List<string> ParseTextAsset(TextAsset data)
    {
        var listToReturn = new List<string>();
        var arrayString = data.text.Split('\n');
        foreach (var line in arrayString)
        {
            listToReturn.Add(line);
        }
        return listToReturn;
    }

    private void SetLevelData(List<string> data)
    {
        level.campaignLevelNum = int.Parse(data[0]);
        level.objectives = ParseObjectivesData(data[1]);
        level.availableStructures = ParseAvailableTechData(data[2]);
        level.cornerBases = bool.Parse(data[3]);
        level.destructorsEnabled = bool.Parse(data[4]);
        level.blueprintsEnabled = bool.Parse(data[5]);
        level.generatorEnabled = bool.Parse(data[6]);
        level.factoryEnabled = bool.Parse(data[7]);
        level.barracksEnabled = bool.Parse(data[8]);
        level.stackDestructorInOpeningHand = bool.Parse(data[9]);
        Services.GameManager.SetStrategies(true, data[10]);

        level.tooltips = new TooltipInfo[0];
        level.overrideStrategy = Services.GameManager.currentStrategies[0];

        level.preplacedP1Tiles = prePlacedP1Tiles.ToArray();
        level.preplacedP2Tiles = prePlacedP2Tiles.ToArray();
        level.structCoords = structCoords.ToArray();
        level.impassibleCoords = impassableCoords.ToArray();
        level.destructibleTerrainCoords = destructibleTerrainCoords.ToArray();
        level.width = map.width;
        level.height = map.height;     
    }

    private string[] ParseObjectivesData(string data)
    {
        return data.Split('|');
    }

    private BuildingType[] ParseAvailableTechData(string data)
    {
        string[] availableTechString = data.Split('|');
        BuildingType[] availbleTech = new BuildingType[availableTechString.Length];
        for(int i = 0; i < availableTechString.Length; i++)
        {
            availableTechString[i] = availableTechString[i].Trim();
          
            availbleTech[i] =  
                TechBuilding.GetBuildingFromString(availableTechString[i]);
            
        }

        return availbleTech;
    }  
	
    public void ParseMapData()
    {
        for(int x = 0; x < map.width; x++)
        {
            for(int y = 0; y < map.height; y++)
            {
                CategorizeTile(x, y);
            }
        }
    }

    void CategorizeTile(int x, int y)
    {
        Color pixelColor = map.GetPixel(x, y);
        Coord tileCoord = new Coord(x, y);

        if (pixelColor == Color.red)
        {
            // Player 1
            prePlacedP1Tiles.Add(tileCoord);
        }
        else if (pixelColor == Color.blue)
        {
            // Player 2
            prePlacedP2Tiles.Add(tileCoord);
        }
        else if (pixelColor == Color.green)
        {
            // Tech Coord
            structCoords.Add(tileCoord);
        }
        else if(pixelColor == Color.black)
        {
            //  Impassable Terrain
            impassableCoords.Add(tileCoord);
        }
        else if(pixelColor == Color.yellow)
        {
            //  Destructible Terrain
            destructibleTerrainCoords.Add(tileCoord);
        }       
    }
}

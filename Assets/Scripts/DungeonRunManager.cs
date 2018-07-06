using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class DungeonRunManager
{
    private const int MAX_TECH_CHOICES = 3;
    private const float handicapIncrement = 0.1f;
    private const string fileName = "dungeonRunData";    
    public static DungeonRunData dungeonRunData { get; private set; }

    private static readonly BuildingType[] availableTech =
    {
            BuildingType.DYNAMO,        BuildingType.SUPPLYBOOST,       BuildingType.UPSIZE,
            BuildingType.SHIELDEDPIECES,BuildingType.ARMORY,            BuildingType.FISSION,
            BuildingType.RECYCLING
    };

    public static void LoadData()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            fileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(filePath))
        {
            file = File.OpenRead(filePath);
            dungeonRunData = (DungeonRunData)bf.Deserialize(file);
        }
        else
        {
            file = File.Create(filePath);
            dungeonRunData = new DungeonRunData();
            bf.Serialize(file, dungeonRunData);
        }

        file.Close();
    }

    private static void SaveData()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            fileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        file = File.OpenWrite(filePath);
        bf.Serialize(file, dungeonRunData);
        file.Close();
    }

    public static void OnGameWin()
    {
        dungeonRunData.challenegeNum += 1;
        SetHandicap(dungeonRunData.handicapLevel + handicapIncrement);
        SaveData();
    }

    public static void AddNewTech(TechBuilding tech)
    {
        dungeonRunData.currentTech.Add(tech);
    }

    private static bool ContainsTech(BuildingType candidate)
    {
        foreach (TechBuilding tech in dungeonRunData.currentTech)
        {
            if(tech.buildingType == candidate)
            {
                return true;
            }
        }
        return false;
    }

    private static TechBuilding GenerateTech()
    {
        int index = Random.Range(0, availableTech.Length - 1);
        BuildingType techCandidate = availableTech[index];

        while(ContainsTech(techCandidate))
        {
            index = Random.Range(0, availableTech.Length - 1);
            techCandidate = availableTech[index];
        }

        TechBuilding structure;
        switch (techCandidate)
        {
            case BuildingType.DYNAMO:
                structure = new Dynamo();
                break;
            case BuildingType.SUPPLYBOOST:
                structure = new SupplyBoost();
                break;
            case BuildingType.UPSIZE:
                structure = new Upsize();
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
            default:
                return null;
        }

        return structure;
    }

    public static List<TechBuilding> GenerateTechToChooseFrom()
    {
        List<TechBuilding> techChoices = new List<TechBuilding>();

        for (int i = 0; i < MAX_TECH_CHOICES; i++)
        {
            techChoices.Add(GenerateTech());
        }

        return techChoices;
    }

    public static void AddSelectedTech(TechBuilding tech)
    {
        dungeonRunData.currentTech.Add(tech);
    }

    public static void OnGameLoss()
    {
        dungeonRunData.challenegeNum = 1;
        SetHandicap(0);
        SaveData();
    }

    private static void SetHandicap(float handicap)
    {
        dungeonRunData.handicapLevel = Mathf.Max(1, handicap);
    }

    public static void ResetDungeonRun()
    {
        SetHandicap(0);
        SaveData();
    }
}


[System.Serializable]
public class DungeonRunData
{
    public List<TechBuilding> currentTech;
    public int challenegeNum;
    public float handicapLevel;

    public DungeonRunData(List<TechBuilding> ownedTech,  int challengeLevel, float handicap)
    {
        currentTech = ownedTech;
        challenegeNum = challengeLevel;
        handicapLevel = handicap;
    }

    public DungeonRunData()
    {
        currentTech = new List<TechBuilding>();
        challenegeNum = 1;
    }
}
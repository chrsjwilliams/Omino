using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class DungeonRunManager
{
    public const int MAX_DUNGEON_CHALLENGES = 3;
    public static DungeonRunData dungeonRunData { get; private set; }

    private const int MAX_TECH_CHOICES = 3;
    private const int MAX_TECH_INVENTORY = MAX_DUNGEON_CHALLENGES - 1;
    private const float handicapIncrement = 0.1f;
    private const string fileName = "dungeonRunData";
    

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

    private static void ResetDungeonRunData()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            fileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        file = File.Create(filePath);
        dungeonRunData = new DungeonRunData();
        bf.Serialize(file, dungeonRunData);

        file.Close();

    }

    public static void OnGameWin()
    {
        dungeonRunData.techChoices.Clear();
        Services.UIManager.dungeonRunUIManager.OnGameEnd(true);
        dungeonRunData.challenegeNum += 1;

        if (dungeonRunData.challenegeNum > MAX_DUNGEON_CHALLENGES)
        {
            Services.Analytics.DungeonRunWin(dungeonRunData.currentTech);
            OnCompleteDungeonRun();
        }
        else
        {
            dungeonRunData.selectingNewTech = true;
            SetHandicap(dungeonRunData.handicapLevel + handicapIncrement);
        }
        
        SaveData();
    }

    public static void OnGameLoss()
    {
        Services.UIManager.dungeonRunUIManager.OnGameEnd(false);

        Services.Analytics.DungeonRunLoss(dungeonRunData.challenegeNum);
        
        ResetDungeonRunData();
    }

    private static void OnCompleteDungeonRun()
    {
        dungeonRunData.completedRun = true;
        Services.Analytics.MatchEnded();
        
        ResetDungeonRunData();
    }

    private static bool PlayerHasTech(BuildingType candidate)
    {
        foreach (BuildingType tech in dungeonRunData.currentTech)
        {
            if(tech == candidate)
            {
                return true;
            }
        }
        return false;
    }

    private static BuildingType GenerateTech()
    {
        int index = Random.Range(0, availableTech.Length - 1);
        BuildingType techCandidate = availableTech[index];

        while(PlayerHasTech(techCandidate))
        {
            index = Random.Range(0, availableTech.Length - 1);
            techCandidate = availableTech[index];
        }

        return techCandidate;
    }

    public static TechBuilding GetBuildingFromType(BuildingType type)
    {
        
        TechBuilding structure;
        switch (type)
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

    private static List<BuildingType> GenerateTechToChooseFrom()
    {
        List<BuildingType> techChoices = new List<BuildingType>();

        while(techChoices.Count < 3)
        {
            BuildingType candidateTechBuilding = GenerateTech();
            if (TechSelectionIsUnique(techChoices, candidateTechBuilding))
            {
                techChoices.Add(candidateTechBuilding);
            }
        }

        return techChoices;
    }

    public static List<BuildingType> GetTechBuildingSelection()
    {
        if (dungeonRunData.techChoices.Count < 1)
        {
            dungeonRunData.techChoices = GenerateTechToChooseFrom();
        }

        return dungeonRunData.techChoices;
    }

    private static bool TechSelectionIsUnique(List<BuildingType> techChoices, BuildingType tech)
    {
        if (techChoices.Count < 1) return true;
        else
        {
            foreach(BuildingType techChoice in techChoices)
            {
                if (techChoice == tech)
                    return false;
            }

            return true;
        }
    }

    public static void AddSelectedTech(BuildingType tech)
    {
        if (dungeonRunData.currentTech.Count < MAX_TECH_INVENTORY)
        {
            dungeonRunData.currentTech.Add(tech);
            Services.Analytics.TechSelected(tech, dungeonRunData.techChoices);
            dungeonRunData.selectingNewTech = false;
        }
        SaveData();
    }

    private static void SetHandicap(float handicap)
    {
        dungeonRunData.handicapLevel = Mathf.Max(1, handicap);
    }

    public static void ResetDungeonRun()
    {
        ResetDungeonRunData();
    }

    public static void PrintCurrentTech()
    {
        string techList = "Current Tech: ";
        for(int i = 0; i < dungeonRunData.currentTech.Count; i++)
        {
            techList += " " + dungeonRunData.currentTech[i] + ", ";
        }

        Debug.Log(techList);
    }
}

[System.Serializable]
public class DungeonRunData
{
    public bool selectingNewTech;
    public bool completedRun;
    public List<BuildingType> techChoices;
    public List<BuildingType> currentTech;
    public int challenegeNum;
    public float handicapLevel;

    public DungeonRunData(List<BuildingType> ownedTech,  List<BuildingType> techSelection, int challenegeNumLevel, float handicap, bool selecting, bool selected)
    {
        currentTech = ownedTech;
        techChoices = techSelection;
        challenegeNum = challenegeNumLevel;
        handicapLevel = handicap;
        selectingNewTech = selecting;
        completedRun = selected;
    }

    public DungeonRunData()
    {
        currentTech = new List<BuildingType>();
        techChoices = new List<BuildingType>();
        challenegeNum = 1;
        handicapLevel = 1;
        selectingNewTech = false;
        completedRun = false;
    }
}
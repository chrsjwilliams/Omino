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

        if (false)
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

        dungeonRunData.challenegeNum += 1;

        if (dungeonRunData.challenegeNum > MAX_DUNGEON_CHALLENGES)
        {
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
        ResetDungeonRunData();
    }

    private static void OnCompleteDungeonRun()
    {
        dungeonRunData.completedRun = true;
    }

    private static bool PlayerHasTech(BuildingType candidate)
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

        while(PlayerHasTech(techCandidate))
        {
            index = Random.Range(0, availableTech.Length - 1);
            techCandidate = availableTech[index];
        }

        return GetBuildingFromType(techCandidate);
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

    private static List<TechBuilding> GenerateTechToChooseFrom()
    {
        List<TechBuilding> techChoices = new List<TechBuilding>();

        while(techChoices.Count < 3)
        {
            TechBuilding candidateTechBuilding = GenerateTech();
            if (TechSelectionIsUnique(techChoices, candidateTechBuilding))
            {
                techChoices.Add(candidateTechBuilding);
            }
        }

        return techChoices;
    }

    public static List<TechBuilding> GetTechBuildingSelection()
    {
        if (dungeonRunData.techChoices.Count < 1)
        {
            dungeonRunData.techChoices = GenerateTechToChooseFrom();
        }

        return dungeonRunData.techChoices;
    }


    private static bool TechSelectionIsUnique(List<TechBuilding> techChoices, TechBuilding tech)
    {
        if (techChoices.Count < 1) return true;
        else
        {
            foreach(TechBuilding techChoice in techChoices)
            {
                if (techChoice.buildingType == tech.buildingType)
                    return false;
            }

            return true;
        }
    }

    public static void AddSelectedTech(TechBuilding tech)
    {
        if (dungeonRunData.currentTech.Count < MAX_TECH_INVENTORY)
        {
            dungeonRunData.currentTech.Add(tech);
            dungeonRunData.selectingNewTech = false;
        }
        else
        {
            Debug.Log("Too many!");
        }
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
    public List<TechBuilding> techChoices;
    public List<TechBuilding> currentTech;
    public int challenegeNum;
    public float handicapLevel;

    public DungeonRunData(List<TechBuilding> ownedTech,  List<TechBuilding> techSelection, int challengeLevel, float handicap, bool selecting, bool selected)
    {
        currentTech = ownedTech;
        techChoices = techSelection;
        challenegeNum = challengeLevel;
        handicapLevel = handicap;
        selectingNewTech = selecting;
        completedRun = selected;
    }

    public DungeonRunData()
    {
        currentTech = new List<TechBuilding>();
        techChoices = new List<TechBuilding>();
        challenegeNum = 1;
        handicapLevel = 1;
        selectingNewTech = false;
        completedRun = false;
    }
}
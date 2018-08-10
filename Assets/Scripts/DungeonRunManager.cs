using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class DungeonRunManager
{
    public const int MAX_DUNGEON_CHALLENGES = 5;
    public static DungeonRunData dungeonRunData { get; private set; }

    private const int MAX_TECH_CHOICES = 3;
    private const int MAX_TECH_INVENTORY = MAX_DUNGEON_CHALLENGES - 1;
    public const float MIN_HANDICAP_LEVEL = 0.85f;
    public const float MAX_ENERGY_HANDICAP = 1.3f;
    private const float handicapIncrement = 0.15f;
    private const string fileName = "dungeonRunData";
    

    private static readonly BuildingType[] availableTech =
    {
            BuildingType.DYNAMO,        BuildingType.SUPPLYBOOST,       BuildingType.UPSIZE,
            BuildingType.SHIELDEDPIECES,BuildingType.ARMORY,            BuildingType.FISSION,
            BuildingType.RECYCLING,     BuildingType.CROSSSECTION,      BuildingType.ANNEX
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
            try
            {
                dungeonRunData = (DungeonRunData) bf.Deserialize(file);
            }
            catch (Exception e) 
            {
                Debug.Log("Failed to deserialize. Reason: " + e.Message);
                file.Dispose();
                ResetDungeonRunData();
                SaveData();
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
            dungeonRunData = new DungeonRunData();
            bf.Serialize(file, dungeonRunData);
            
            file.Close();
        }
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
        Services.UIManager.UIMenu.dungeonRunChallenegeCompleteMenu.OnGameEnd(true);
        dungeonRunData.challengeNum += 1;

        if (dungeonRunData.challengeNum > MAX_DUNGEON_CHALLENGES)
        {
            Services.Analytics.DungeonRunWin(dungeonRunData.currentTech);
            OnCompleteDungeonRun();
        }
        else
        {
            dungeonRunData.selectingNewTech = true;
            
            SetHandicap();
        }
        
        SaveData();
    }

    public static void OnGameLoss()
    {
        Services.UIManager.UIMenu.dungeonRunChallenegeCompleteMenu.OnGameEnd(false);

        Services.Analytics.DungeonRunLoss(dungeonRunData.challengeNum);
        
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

    public static BuildingType GenerateTech()
    {
        int index = UnityEngine.Random.Range(0, availableTech.Length);
        BuildingType techCandidate = availableTech[index];

        while(PlayerHasTech(techCandidate))
        {
            index = UnityEngine.Random.Range(0, availableTech.Length);
            techCandidate = availableTech[index];
        }

        return techCandidate;
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

    public static PlayerHandicap SetHandicap()
    {
        float hammerProductionHandicap = MIN_HANDICAP_LEVEL + (handicapIncrement * (dungeonRunData.challengeNum - 1));
        float pieceProductionHandicap = MIN_HANDICAP_LEVEL + (handicapIncrement * (dungeonRunData.challengeNum - 1));
        float energyProdictionHandicap = MIN_HANDICAP_LEVEL + (handicapIncrement * (dungeonRunData.challengeNum - 1));

        if(energyProdictionHandicap > MAX_ENERGY_HANDICAP)
        {
            energyProdictionHandicap = MAX_ENERGY_HANDICAP;
        }

        dungeonRunData.handicapLevel = new PlayerHandicap(  energyProdictionHandicap, 
                                                            pieceProductionHandicap, 
                                                            hammerProductionHandicap);

        return dungeonRunData.handicapLevel;
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
    public int challengeNum;
    public PlayerHandicap handicapLevel;

    public DungeonRunData(List<BuildingType> ownedTech,  List<BuildingType> techSelection, int challengeNumLevel, PlayerHandicap handicap, bool selecting, bool selected)
    {
        currentTech = ownedTech;
        techChoices = techSelection;
        challengeNum = challengeNumLevel;
        handicapLevel = handicap;
        selectingNewTech = selecting;
        completedRun = selected;
    }

    public DungeonRunData()
    {
        currentTech = new List<BuildingType>();
        techChoices = new List<BuildingType>();
        challengeNum = 1;

        //float minHandicapUsingEloData = (1 + ELOManager.eloData.handicapLevel) - 0.1f;
        float dungeonRunMinHandicap = Mathf.Max(DungeonRunManager.MIN_HANDICAP_LEVEL, 0);
        handicapLevel = new PlayerHandicap( dungeonRunMinHandicap,
                                            dungeonRunMinHandicap,
                                            dungeonRunMinHandicap);
        selectingNewTech = false;
        completedRun = false;
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class LevelManager
{
    public const string fileName = "levelManager";
    public const int TOTAL_NUM_OF_CUSTOM_MAPS = 5;

    public static LevelInfromation levelInfo { get; private set; }
    
    public static void SaveData()
    {
        string filePath = Path.Combine(
           Application.persistentDataPath,
           fileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        try
        {

            file = File.OpenWrite(filePath);
            bf.Serialize(file, levelInfo);
            file.Close();
        }
        catch(Exception e)
        {
            Debug.Log("Failed to serialize. Reason: " + e.Message);
        }
    }

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
                levelInfo = (LevelInfromation)bf.Deserialize(file);
            }
            catch(Exception e)
            {
                Debug.Log("Failed to deserialize. Reason: " + e.Message);
                ResetLevelManagerData();
                file.Dispose();
                SaveData();
            }
            finally
            {
                file.Close();
            }
        }
        else
        {
            file = File.Create(filePath);
            levelInfo = new LevelInfromation();
            bf.Serialize(file, levelInfo);

            file.Close();
        }
    }

    private static void ResetLevelManagerData()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            fileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        file = File.Create(filePath);
        levelInfo = new LevelInfromation();
        bf.Serialize(file, levelInfo);

        file.Close();

    }

    public static void AddLevel(string levelName, LevelData level, bool customLevel = false)
    {
        switch (customLevel)
        {
            case true:
                if (levelInfo.customLevels.Count < TOTAL_NUM_OF_CUSTOM_MAPS && !levelInfo.levelDictionary.ContainsKey(levelName))
                {
                    Debug.Log("Level " + levelName + " added");
                    levelInfo.customLevels.Add(level);
                    levelInfo.levelDictionary.Add(levelName, level);
                }
                else if (levelInfo.customLevels.Count > TOTAL_NUM_OF_CUSTOM_MAPS)
                {
                    Debug.Log("Cannot add " + levelName + ". All custom map slots have been used.");
                }
                else
                {
                    //Debug.Log("A Level with the name " + levelName + " has already been added");
                }
                break;
            case false:
                if (levelInfo.levelDictionary.ContainsKey(levelName))
                {
                    //Debug.Log("A Level with the name " + levelName + " has already been added");
                }
                else
                {
                    levelInfo.levelDictionary.Add(levelName, level);
                }
                break;
            default:
                break;
        }    
        SaveData();
    }

    public static void OverwriteLevel(LevelData levelData)
    {
        for(int i = 0; i < levelInfo.customLevels.Count; i++)
        {
            if(levelInfo.customLevels[i].levelName == levelData.levelName)
            {
                levelInfo.customLevels[i] = levelData;
                levelInfo.levelDictionary[levelData.levelName] = levelData;
                break;
            }
        }
        Debug.Log("Overwrite");
        SaveData();
    }

    public static void RemoveLevel(string levelName, bool customLevel = false)
    {

        switch (customLevel)
        {
            case true:
                if (levelInfo.levelDictionary.Count > 0 && levelInfo.levelDictionary.ContainsKey(levelName))
                {
                    levelInfo.customLevels.Remove(levelInfo.levelDictionary[levelName]);
                    levelInfo.levelDictionary.Remove(levelName);
                    Debug.Log("Removed level " + levelName);
                }
                else
                {
                    Debug.Log("No level with the name " + levelName + " was found");
                }
                break;
            case false:
                if (!levelInfo.levelDictionary.ContainsKey(levelName))
                {
                    Debug.Log("No level with the name " + levelName + " was found");
                    PrintLevelNames();
                }
                else
                {
                    if (levelInfo.levelDictionary.Remove(levelName))
                    {
                        Debug.Log("Removed level " + levelName);
                    }
                    else
                    {
                        Debug.Log("Could not remove " + levelName);
                    }
                }
                break;
            default:
                break;
        }       
        SaveData();
    }

    public static LevelData GetLevel(string levelName)
    {
        if(!levelInfo.levelDictionary.ContainsKey(levelName))
        {
            Debug.Log("No level with the name " + levelName + " was found");
            PrintLevelNames();
            return null;
        }
        else
        {
            return levelInfo.levelDictionary[levelName];
        }
    } 

    public static void RenameLevel(string oldName, string newName)
    {
        if (!levelInfo.levelDictionary.ContainsKey(oldName))
        {
            Debug.Log("No level with the name " + oldName + " was found");
            PrintLevelNames();
            return;
        }
        else
        {
            LevelData level = levelInfo.levelDictionary[oldName];
            levelInfo.levelDictionary.Remove(oldName);
            levelInfo.levelDictionary.Add(newName, level);
            SaveData();
        }
    }

    public static void PrintLevelNames()
    {
        if (levelInfo.levelDictionary.Count == 0)
        {
            Debug.Log("No levels added");
            return;
        }

        string levelNames = "Current Levels: ";
        foreach(KeyValuePair<string, LevelData> entry in levelInfo.levelDictionary)
        {
            levelNames += entry.Key + ", ";
        }

        levelNames.Remove(levelNames.Length - 2);
        //Debug.Log(levelNames);
    }
}

[System.Serializable]
public class LevelInfromation
{
    public List<LevelData> customLevels;
    public Dictionary<string, LevelData> levelDictionary { get; private set; }

    public LevelInfromation(List<LevelData> customLevels_, Dictionary<string, LevelData> levelDictionary_)
    {
        customLevels = customLevels_;
        levelDictionary = levelDictionary_;
    }

    public LevelInfromation()
    {
        customLevels = new List<LevelData>();
        levelDictionary = new Dictionary<string, LevelData>();
    }

    public bool CustomLevelsContainName(string name)
    {
        foreach(LevelData level in customLevels)
        {
            if (level.levelName == name)
                return true;
        }

        return false;
    }

}
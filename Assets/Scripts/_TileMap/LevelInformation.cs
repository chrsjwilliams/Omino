using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class LevelInformation
{
    public const string fileName = "levelDictionaryInfo";

    public Dictionary<string, LevelData> levelDictionary { get; private set; }
    private BinaryFormatter formatter;
    


    public LevelInformation()
    {
        levelDictionary = new Dictionary<string, LevelData>();
        formatter = new BinaryFormatter();
    }

    public void Save()
    {
        string filePath = Path.Combine(
           Application.persistentDataPath,
           fileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        try
        {

            file = File.OpenWrite(filePath);
            bf.Serialize(file, levelDictionary);
            file.Close();
        }
        catch(Exception e)
        {
            Debug.Log("Failed to serialize. Reason: " + e.Message);
        }
    }

    public void Load()
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
                file =  new FileStream(filePath, FileMode.Open, FileAccess.Read);

                levelDictionary = (Dictionary<string, LevelData>)formatter.Deserialize(file);

                file.Close();
            }
            catch(Exception e)
            {
                Debug.Log("Failed to deserialize. Reason: " + e.Message);

                file.Dispose();
                Save();
            }
            finally
            {
                file.Close();
            }
        }
    }

    public void AddLevel(string levelName, LevelData level)
    {
        if(levelDictionary.ContainsKey(levelName))
        {
            Debug.Log("A Level with the name " + levelName + " has already been added");
        }
        else
        {
            levelDictionary.Add(levelName, level);
            Debug.Log(levelName + " successfully added");
        }
        Save();
    }

    public void RemoveLevel(string levelName)
    {
        if(!levelDictionary.ContainsKey(levelName))
        {
            Debug.Log("No level with the name " + levelName + " was found");
            PrintLevelNames();
        }
        else
        {
            if(levelDictionary.Remove(levelName))
            {
                Debug.Log("Removed level " + levelName);
            }
            else
            {
                Debug.Log("Could not remove " + levelName);
            }
        }
        Save();
    }

    public LevelData GetLevel(string levelName)
    {
        if(!levelDictionary.ContainsKey(levelName))
        {
            Debug.Log("No level with the name " + levelName + " was found");
            PrintLevelNames();
            return null;
        }
        else
        {
            return levelDictionary[levelName];
        }
    }

    public void RenameLevel(string oldName, string newName)
    {
        if (!levelDictionary.ContainsKey(oldName))
        {
            Debug.Log("No level with the name " + oldName + " was found");
            PrintLevelNames();
            return;
        }
        else
        {
            LevelData level = levelDictionary[oldName];
            levelDictionary.Remove(oldName);
            levelDictionary.Add(newName, level);
            Save();
        }
    }

    public void PrintLevelNames()
    {
        if (levelDictionary.Count == 0)
        {
            Debug.Log("No levels added");
            return;
        }

        string levelNames = "Current Levels: ";
        foreach(KeyValuePair<string, LevelData> entry in levelDictionary)
        {
            levelNames += entry.Key + ", ";
        }

        levelNames.Remove(levelNames.Length - 2);
        Debug.Log(levelNames);
    }
}

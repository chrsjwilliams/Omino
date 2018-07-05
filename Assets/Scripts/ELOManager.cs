using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ELOManager
{
    private const float handicapIncrement = 0.025f;
    private const int winStreakLength = 3;
    private const float winStreakHandicapIncrement = 0.05f;
    private const float minHandicap = -0.5f;
    private const string fileName = "eloSaveData";
    public static EloData eloData { get; private set; }

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
            eloData = (EloData)bf.Deserialize(file);
        }
        else
        {
            file = File.Create(filePath);
            eloData = new EloData();
            bf.Serialize(file, eloData);
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
        bf.Serialize(file, eloData);
        file.Close();
    }

    public static void OnGameWin()
    {
        eloData.totalWins += 1;
        eloData.winStreakCount += 1;
        if(eloData.winStreakCount >= winStreakLength)
        {
            SetHandicap(eloData.handicapLevel + winStreakHandicapIncrement);
        }
        else
        {
            SetHandicap(eloData.handicapLevel + handicapIncrement);
        }

        SaveData();
    }

    public static void OnGameLoss()
    {
        eloData.winStreakCount = 0;
        SetHandicap(eloData.handicapLevel - handicapIncrement);
        SaveData();
    }

    private static void SetHandicap(float handicap)
    {
        eloData.handicapLevel = Mathf.Max(minHandicap, handicap);
    }
}

[System.Serializable]
public class EloData
{
    public int winStreakCount;
    public float handicapLevel;
    public int totalWins;

    public EloData(int winStreak, float handicap, int wins)
    {
        winStreakCount = winStreak;
        handicapLevel = handicap;
        totalWins = wins;
    }

    public EloData()
    {
        winStreakCount = 0;
        handicapLevel = 0;
        totalWins = 0;
    }
}


using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ELOManager
{
    private const float handicapIncrement = 0.03f;
    private const int winStreakLength = 3;
    private const float winStreakHandicapIncrement = 0.05f;
    private const float minHandicap = -0.5f;
    public const float baseHandicap = -0.2f;
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
        int prevElo = eloData.GetRating();
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
        int newElo = eloData.GetRating();
        Services.UIManager.eloUIManager.OnGameEnd(true, prevElo, newElo);
        SaveData();
    }

    public static void OnGameLoss()
    {
        int prevElo = eloData.GetRating();
        eloData.winStreakCount = 0;
        SetHandicap(eloData.handicapLevel - handicapIncrement);
        int newElo = eloData.GetRating();
        Services.UIManager.eloUIManager.OnGameEnd(false, prevElo, newElo);
        SaveData();
    }

    private static void SetHandicap(float handicap)
    {
        eloData.handicapLevel = Mathf.Max(minHandicap, handicap);
    }

    public static void ResetRank()
    {
        SetHandicap(baseHandicap);
        SaveData();
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
        handicapLevel = ELOManager.baseHandicap;
        totalWins = 0;
    }

    public int GetRating()
    {
        return Mathf.RoundToInt(100 * (1 + handicapLevel));
    }
}


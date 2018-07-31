using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class ELOManager
{
    private const float handicapIncrement = 0.03f;
    private const int winStreakLength = 3;
    private const float winStreakHandicapIncrement = 0.05f;
    public const float minHandicap = -0.5f;
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
            try
            {
                eloData = (EloData) bf.Deserialize(file);
            }
            catch (SerializationException e)
            {
                Debug.Log("Failed to deserialize, reason: " + e.Message);
                file.Dispose();
                ResetData();
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
            eloData = new EloData();
            bf.Serialize(file, eloData);
            
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
        bf.Serialize(file, eloData);
        file.Close();
    }
    
    private static void ResetData()
    {
        string filePath = Path.Combine(
            Application.persistentDataPath,
            fileName);
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        
        file = File.Create(filePath);
        eloData = new EloData();
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
            eloData.SetHandicap(eloData.handicapLevel + winStreakHandicapIncrement);
        }
        else
        {
            eloData.SetHandicap(eloData.handicapLevel + handicapIncrement);
        }
        int newElo = eloData.GetRating();
        Services.UIManager.eloUIManager.OnGameEnd(true, prevElo, newElo);
        Services.Analytics.ELOWin(true);
        Services.Analytics.ELOStreak(eloData.winStreakCount);
        Services.Analytics.ELOTotalWins(eloData.totalWins);
        Services.Analytics.ELORating(eloData.GetRating());
        SaveData();
    }

    public static void OnGameLoss()
    {
        int prevElo = eloData.GetRating();
        eloData.winStreakCount = 0;
        eloData.SetHandicap(eloData.handicapLevel - handicapIncrement);
        int newElo = eloData.GetRating();
        Services.UIManager.eloUIManager.OnGameEnd(false, prevElo, newElo);
        Services.Analytics.ELOWin(false);
        SaveData();
    }

    public static void ResetRank()
    {
        eloData.SetHandicap(baseHandicap);
        SaveData();
    }
}

[System.Serializable]
public class EloData
{
    public int winStreakCount;
    public float handicapLevel;
    public int totalWins;
    public float highestHandicapAchieved;

    public EloData(int winStreak, float handicap, int wins, float highestAchieved)
    {
        winStreakCount = winStreak;
        totalWins = wins;
        highestHandicapAchieved = highestAchieved;
        SetHandicap(handicap);
    }

    public EloData()
    {
        winStreakCount = 0;
        handicapLevel = ELOManager.baseHandicap;
        totalWins = 0;
        highestHandicapAchieved = handicapLevel;
    }

    public int GetRating()
    {
        return FormatRating(handicapLevel);
    }

    public int GetHighestRating()
    {
        return FormatRating(highestHandicapAchieved);
    }

    private int FormatRating(float handicap)
    {
        return Mathf.RoundToInt(100 * (1 + handicap));
    }

    public void SetHandicap(float handicap)
    {
        handicapLevel = Mathf.Max(ELOManager.minHandicap, handicap);
        if (handicapLevel > highestHandicapAchieved)
        {
            highestHandicapAchieved = handicapLevel;
        }
    }
}


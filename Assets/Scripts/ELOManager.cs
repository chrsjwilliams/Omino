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
        EloData prevElo = new EloData(eloData);
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
        Services.UIManager.UIMenu.eloUIManager.OnGameEnd(true, prevElo, eloData);
        Services.Analytics.ELOWin(true);
        Services.Analytics.ELOStreak(eloData.winStreakCount);
        Services.Analytics.ELOTotalWins(eloData.totalWins);
        Services.Analytics.ELORating(eloData.GetRating());
        SaveData();
    }

    public static void OnGameLoss()
    {
        EloData prevElo = new EloData(eloData);
        eloData.winStreakCount = 0;
        eloData.SetHandicap(eloData.handicapLevel - handicapIncrement);
        Services.UIManager.UIMenu.eloUIManager.OnGameEnd(false, prevElo, eloData);
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
    public enum RankCategory { Bronze, Silver, Gold, Platinum, Diamond, Master }
    public static RankCategory[] ranksInDescendingOrder = new RankCategory[]
    {
        RankCategory.Master, RankCategory.Diamond, RankCategory.Platinum,
        RankCategory.Gold, RankCategory.Silver, RankCategory.Bronze
    };

    public EloData(int winStreak, float handicap, int wins, float highestAchieved)
    {
        winStreakCount = winStreak;
        totalWins = wins;
        highestHandicapAchieved = highestAchieved;
        SetHandicap(handicap);
    }

    public EloData(EloData toCopy)
    {
        winStreakCount = toCopy.winStreakCount;
        totalWins = toCopy.totalWins;
        highestHandicapAchieved = toCopy.highestHandicapAchieved;
        SetHandicap(toCopy.handicapLevel);
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

    public RankCategory GetRank()
    {
        return GetRankCategory(handicapLevel);
    }

    public RankCategory GetHighestRank()
    {
        return GetRankCategory(highestHandicapAchieved);
    }

    public float GetProgressToNextRank()
    {
        return GetProgressToNextRank(handicapLevel);
    }

    public static RankCategory GetRankCategory(float handicap)
    {
        for (int i = 0; i < ranksInDescendingOrder.Length; i++)
        {
            RankCategory rank = ranksInDescendingOrder[i];
            if (handicap >= GetRankMin(rank)) return rank;
        }
        return RankCategory.Bronze;
    }

    public static float GetRankMin(RankCategory category)
    {
        switch (category)
        {
            case RankCategory.Bronze:
                return ELOManager.minHandicap;
            case RankCategory.Silver:
                return 0f;
            case RankCategory.Gold:
                return 0.1f;
            case RankCategory.Platinum:
                return 0.2f;
            case RankCategory.Diamond:
                return 0.3f;
            case RankCategory.Master:
                return 0.4f;
            default:
                return -0.5f;
        }
    }

    public static float GetProgressToNextRank(float handicap)
    {
        RankCategory nextRank = RankCategory.Master;
        for (int i = 0; i < ranksInDescendingOrder.Length; i++)
        {
            RankCategory rank = ranksInDescendingOrder[i];
            if (handicap >= GetRankMin(rank))
            {
                if (rank == RankCategory.Master) return 0f;
                return 1 -((GetRankMin(nextRank) - handicap)/
                    (GetRankMin(nextRank) - GetRankMin(rank)));
            }
            nextRank = rank;
        }
        return 0f;
    }

    private static Sprite GetRankImageByCategory(RankCategory rank)
    {
        return Services.EloRankData.RankSprites[(int)rank];
    }

    public Sprite GetRankImage()
    {
        return GetRankImageByCategory(GetRank());
    }

    public Sprite GetHighestRankImage()
    {
        return GetRankImageByCategory(GetHighestRank());
    }

}


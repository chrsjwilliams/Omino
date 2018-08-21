using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Data")]
public class LevelDataLibrary : ScriptableObject
{
    [System.Serializable]
    public class LevelDataEntry
    {
        public string levelName;
        public Sprite image;
    }
    public LevelDataEntry[] dataArray;

    public Sprite GetLevelImage(string levelName)
    {
        for (int i = 0; i < dataArray.Length; i++)
        {
            if (dataArray[i].levelName == levelName) return dataArray[i].image;
        }
        return dataArray[0].image;
    }
}

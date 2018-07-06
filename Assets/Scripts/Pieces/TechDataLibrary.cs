using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Tech Data")]
public class TechDataLibrary : ScriptableObject
{
    [System.Serializable]
    public class TechData
    {
        public BuildingType type;
        public Sprite icon;
    }
    public Sprite techDropShadow;
    public TechData[] dataArray;

    public Sprite GetIcon(BuildingType type)
    {
        for (int i = 0; i < dataArray.Length; i++)
        {
            if (dataArray[i].type == type) return dataArray[i].icon;
        }
        return null;
    }
}

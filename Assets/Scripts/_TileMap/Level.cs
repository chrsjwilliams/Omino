using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Level")]
public class Level : ScriptableObject
{
    public BuildingType[] availableStructures;
    public Coord[] structCoords;
}

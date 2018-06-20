using UnityEngine;
using System.Collections;

public class GameData : MonoBehaviour
{
    public int[] filledMapTiles = new int[2];
    public int totalFilledMapTiles {
        get
        {
            int total = 0;
            for (int i = 0; i < filledMapTiles.Length; i++)
            {
                total += filledMapTiles[i];
            }
            return total;
        }
    }
    public float[] productionRates = new float[2];
    public int[] distancesToOpponentBase = new int[2];
    public int totalMapTiles;
}

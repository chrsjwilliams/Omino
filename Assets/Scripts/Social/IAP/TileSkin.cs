using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSkin : ScriptableObject
{
    public string tileSkinName = "Tile Skin Name Here";
    public int cost = 50;
    [TextArea]
    public string description;

    public Color[] Player1ColorScheme;
    public Color[] Player2ColorScheme;

    public Sprite[] factoryBottoms;
    public Sprite[] factoryTops;
    public Sprite[] factoryIcons;
    public Sprite[] mineBottoms;
    public Sprite[] mineTops;
    public Sprite[] mineIcons;
    public Sprite[] homeBaseSprites;
    public Sprite baseOverlay;
    public Sprite sideBaseOverlay;
    public Sprite[] bombFactoryBottoms;
    public Sprite[] bombFactoryTops;
    public Sprite[] bombFactoryIcons;
    public Sprite structureOverlay;
    public Sprite[] handSprites;
    public Sprite[] placedSprites;
    public Sprite[] placedSecondarySprites;
    public Sprite[] shieldSprites;
    public Sprite[] disconnectedSprites;

    public Sprite destructibleTerrain;
    public Sprite indestructibleTerrain;

}

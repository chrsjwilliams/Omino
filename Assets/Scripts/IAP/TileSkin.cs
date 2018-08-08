using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSkin : ScriptableObject
{
    public string tileSkinName = "Tile Skin Name Here";
    public int cost = 50;
    [TextArea]
    public string desctiption;

    public Color[] Player1ColorScheme;
    public Color[] Player2ColorScheme;  // use only 1


    public Color[] MapColorScheme;  //  not in skins
    public Color neutralColor;      //  not in skins
    public Sprite destructorIcon;
    public Sprite splashIcon;
    public Sprite[] factoryBottoms;
    public Sprite[] factoryTops;
    public Sprite[] factoryIcons;
    public Sprite[] mineBottoms;
    public Sprite[] mineTops;
    public Sprite[] mineIcons;
    public Sprite baseBottom;
    public Sprite baseOverlay;
    public Sprite sideBaseOverlay;
    public Sprite[] bombFactoryBottoms;
    public Sprite[] bombFactoryTops;
    public Sprite[] bombFactoryIcons;
    public Sprite structureOverlay;
    public Sprite[] tileSprites;
    public Sprite[] destructorSprites;
    public Sprite[] shieldSprites;
    public Sprite disconnectedSprite;
    
}

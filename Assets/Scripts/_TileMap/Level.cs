﻿using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Level")]
public class Level : ScriptableObject
{
    public BuildingType[] availableStructures;
    public Coord[] structCoords;
    public int width;
    public int height;
    public bool cornerBases;
    public bool destructorsEnabled = true;
    public bool blueprintsEnabled = true;
    public TooltipInfo[] tooltips;
    public AIStrategy overrideStrategy;
}

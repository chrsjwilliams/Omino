using UnityEngine;
using System.Collections;
using System;

public class Mine : Blueprint
{
    public const float resourceRateBonus = 0.0375f;
    public Mine(Player player_) : base(BuildingType.MINE, player_)
    {
        onGainText = "+1 Hammer Production";
    }

    protected override string GetName()
    {
        return "Smith";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)resourceGainRateBonus * 10, 3) + 
        //    "</color> resources per second";
        return "Hammer Production Level +1";
    }
}

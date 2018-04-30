using UnityEngine;
using System.Collections;
using System;

public class Mine : Blueprint
{
    public const float resourceRateBonus = 0.0375f;
    public Mine(Player player_) : base(BuildingType.MINE, player_)
    {
        onGainText = "+1 Play Rate Level";
    }

    protected override string GetName()
    {
        return "Accelerator";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)resourceGainRateBonus * 10, 3) + 
        //    "</color> resources per second";
        return "Play Rate Level +1";
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.transform.position = GetCenterpoint();
    }
}

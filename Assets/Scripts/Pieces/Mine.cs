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
        holder.icon.transform.position = GetCenterpoint();
        holder.dropShadow.transform.position = GetCenterpoint();
        int spriteIndex = numRotations % 2;
        holder.spriteBottom.sprite =
            Services.UIManager.mineBottoms[spriteIndex];
        holder.dropShadow.sprite = Services.UIManager.mineTops[spriteIndex];
        holder.icon.sprite = Services.UIManager.mineIcons
            [(2 * (owner.playerNum - 1)) + spriteIndex];
    }
}

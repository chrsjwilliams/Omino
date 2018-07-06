using UnityEngine;
using System.Collections;
using System;

public class Generator : Blueprint
{
    public const float resourceRateBonus = 0.04f;
    public Player player;

    public Generator(Player player_) : base(BuildingType.GENERATOR, player_)
    {
        onGainText = "+1 Energy Recharge Level";
    }

    protected override string GetName()
    {
        return "Generator";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)resourceGainRateBonus * 10, 3) + 
        //    "</color> resources per second";
        return "+1 Energy Recharge Level";
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.transform.localPosition = GetCenterpoint();
        holder.dropShadow.transform.localPosition = GetCenterpoint();
        int spriteIndex = numRotations % 2;
        holder.spriteBottom.sprite =
            Services.UIManager.mineBottoms[spriteIndex];
        holder.dropShadow.sprite = Services.UIManager.mineTops[spriteIndex];
        holder.icon.sprite = Services.UIManager.mineIcons
            [(2 * (owner.playerNum - 1)) + spriteIndex];
    }
}

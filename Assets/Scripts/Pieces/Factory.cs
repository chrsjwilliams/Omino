using UnityEngine;
using System.Collections;
using System;

public class Factory : Blueprint
{
    public const float drawRateBonus = 0.04f;

    public Factory(Player player_) : base(BuildingType.FACTORY, player_)
    {
        onGainText = "+1 Normal Piece Production Level";
    }

    protected override string GetName()
    {
        return "Factory";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)normalDrawRateBonus, 3) + 
        //    "</color> normal pieces per second";
        return "Normal Piece Production Level +1";
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        int spriteIndex = numRotations % 2;
        holder.spriteBottom.sprite =
            Services.UIManager.factoryBottoms[spriteIndex];
        holder.dropShadow.sprite = Services.UIManager.factoryTops[spriteIndex];
        holder.icon.sprite = Services.UIManager.factoryIcons
            [(2 * (owner.playerNum - 1)) + spriteIndex];
    }
}

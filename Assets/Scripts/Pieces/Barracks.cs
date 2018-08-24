using UnityEngine;
using System.Collections;
using System;

public class Barracks : Blueprint
{
    public const float drawRateBonus = 0.02f;
    public Barracks(Player owner_) : base(BuildingType.BARRACKS, owner_)
    {
        maxRotations = 4;
        onGainText = "+1 Hammer Production Level";
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        holder.spriteBottom.transform.localPosition = GetCenterpoint(true);
    }

    public override string GetName()
    {
        return "Barracks";
    }

    public override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)destructorDrawRateBonus, 3) + 
        //    "</color> <color=red>DESTRUCTIVE</color> pieces per second";
        return "+1 <color=red>Hammer</color> Production Level";
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        int spriteIndex = numRotations % 4;
        holder.spriteBottom.sprite =
            Services.UIManager.bombFactoryBottoms[spriteIndex];
        //holder.dropShadow.sprite = Services.UIManager.bombFactoryTops[spriteIndex];
        holder.icon.sprite = Services.UIManager.bombFactoryIcons
            [(4 * (owner.playerNum - 1)) + spriteIndex];
    }
}

using UnityEngine;
using System.Collections;
using System;

public class BombFactory : Blueprint
{
    public const float drawRateBonus = 0.02f;
    public BombFactory(Player owner_) : base(BuildingType.BOMBFACTORY, owner_)
    {
        maxRotations = 4;
        onGainText = "+1 Attack Piece Production Level";
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        holder.spriteBottom.transform.localPosition = GetCenterpoint(true);
    }

    protected override string GetName()
    {
        return "Barracks";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)destructorDrawRateBonus, 3) + 
        //    "</color> <color=red>DESTRUCTIVE</color> pieces per second";
        return "<color=red>Attack</color> Piece Production Level +1";
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        int spriteIndex = numRotations % 4;
        holder.spriteBottom.sprite =
            Services.UIManager.bombFactoryBottoms[spriteIndex];
        holder.dropShadow.sprite = Services.UIManager.bombFactoryTops[spriteIndex];
        holder.icon.sprite = Services.UIManager.bombFactoryIcons
            [(4 * (owner.playerNum - 1)) + spriteIndex];
    }
}

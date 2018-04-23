using UnityEngine;
using System.Collections;
using System;

public class BombFactory : Blueprint
{
    public const float drawRateBonus = 0.02f;
    public BombFactory(Player owner_) : base(BuildingType.BOMBFACTORY, owner_)
    {
        maxRotations = 4;
        onGainText = "+1 Attack Piece Production";
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        spriteOverlay.transform.position = GetCenterpoint(true);
    }

    protected override string GetName()
    {
        return "Barracks";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)destructorDrawRateBonus, 3) + 
        //    "</color> <color=red>DESTRUCTIVE</color> pieces per second";
        return "<color=red>Attack</color> Road Production Level +1";
    }
}

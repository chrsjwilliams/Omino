﻿using UnityEngine;
using System.Collections;

public class Combustion : TechBuilding
{
    public Combustion() : base(0)
    {
        buildingType = BuildingType.COMBUSTION;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleSplashDamage(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleSplashDamage(false);
        base.OnClaimLost();
    }

    protected override string GetName()
    {
        return "Combustion";
    }

    protected override string GetDescription()
    {
        return "<color=red>Attack</color> pieces explode, destroying adjacent enemy " +
            "pieces and themselves.";
    }
}
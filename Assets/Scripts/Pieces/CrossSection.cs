using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossSection : TechBuilding
{
    public CrossSection() : base(0)
    {
        buildingType = BuildingType.CROSSSECTION;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleCrossSection(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleCrossSection(false);
    }

    public override void OnClaim(Player player)
    {
        OnClaimEffect(player);
        base.OnClaim(player);
    }

    public override void OnClaimLost()
    {
        OnLostEffect();
        base.OnClaimLost();
    }

    public override string GetName()
    {
        return "Cross Section";
    }

    public override string GetDescription()
    {
        return "Pieces can overlap neutral and friendly pieces.";
    }
}

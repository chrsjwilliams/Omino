using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CrossSection : TechBuilding
{
    public CrossSection() : base(0)
    {
        label = "Cross Section";

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

    protected override string GetName()
    {
        return "Cross Section";
    }

    public override string GetDescription()
    {
        return "Pieces can overlap neutral and friendly pieces.";
    }
}

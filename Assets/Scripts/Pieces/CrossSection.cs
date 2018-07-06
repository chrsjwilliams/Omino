using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossSection : TechBuilding
{
    public CrossSection() : base(0)
    {
        buildingType = BuildingType.CROSSSECTION;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
       owner.ToggleCrossSection(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleCrossSection(false);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.crossSection;
    }

    protected override string GetName()
    {
        return "Cross Section";
    }

    protected override string GetDescription()
    {
        return "Pieces can overlap neutral and friendly pieces.";
    }
}

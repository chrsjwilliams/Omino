using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Annex : TechBuilding
{
    public Annex() : base(0)
    {
        label = "Annex";

        buildingType = BuildingType.ANNEX;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleAnnex(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleAnnex(false);
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
        return "Annex";
    }

    public override string GetDescription()
    {
        return "Newly placed pieces claim opposing disconnected pieces that do not have Blueprints.";
    }
}

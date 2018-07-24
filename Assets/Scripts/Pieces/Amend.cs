using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amend : TechBuilding
{
    public Amend() : base(0)
    {
        label = "Amend";

        buildingType = BuildingType.AMEND;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleAmend(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleAmend(false);
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
        return "Amend";
    }

    public override string GetDescription()
    {
        return "Newly placed pieces claim opposing disconnected pieces that do not have Blueprints.";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulldoze : TechBuilding
{
    public Bulldoze() : base(0)
    {
        label = "Bulldoze";

        buildingType = BuildingType.BULLDOZE;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleBulldoze(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleBulldoze(false);
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
        return "Bulldoze";
    }

    public override string GetDescription()
    {
        //  Should Bulldoze cost 1 hammer?
        return "Pieces destroy adjacent opposing disconnected pieces.";
    }
}

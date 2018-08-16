using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Retaliate : TechBuilding
{
    public const float hammerReward = 0.5f;

    public Retaliate() : base(0)
    {
        buildingType = BuildingType.RETALIATE;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleRetaliate(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleRetaliate(false);
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
        return "Retaliate";
    }

    public override string GetDescription()
    {
        return "Whenever one of your pieces is destroyed by a hammer, gain " + hammerReward + "  hammer.";
    }
}
using UnityEngine;
using System.Collections;

public class Dynamo : TechBuilding
{
    private const float resourceGainIncrementMultiplier = 0.15f;

    public Dynamo() : base(0)
    {
        buildingType = BuildingType.DYNAMO;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.AugmentResourceGainFactor(resourceGainIncrementMultiplier);
    }

    public override void OnClaimLost()
    {
        owner.AugmentResourceGainFactor(-resourceGainIncrementMultiplier);
        base.OnClaimLost();
    }

    protected override string GetName()
    {
        return "Dynamo";
    }

    protected override string GetDescription()
    {
        return "+" + 100 * resourceGainIncrementMultiplier + "%" +
            " Energy Recharge Rate";
    }

}


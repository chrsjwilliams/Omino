using UnityEngine;
using System.Collections;

public class SupplyBoost : TechBuilding
{
    private const float drawRateMultiplier = 0.15f;

    public SupplyBoost() : base(1)
    {
        buildingType = BuildingType.SUPPLYBOOST;
    }

    public override void OnClaimEffect(Player player)
    {
        player.AugmentDrawRateFactor(drawRateMultiplier);
    }

    public override void OnLostEffect()
    {
        owner.AugmentDrawRateFactor(-drawRateMultiplier);
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
        return "Supply Boost";
    }

    public override string GetDescription()
    {
        return "+" + 100 * drawRateMultiplier + "%" + 
            " piece production rate";
    }
}

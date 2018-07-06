using UnityEngine;
using System.Collections;

public class SupplyBoost : TechBuilding
{
    private const float drawRateMultiplier = 0.15f;

    public SupplyBoost() : base(1)
    {
        buildingType = BuildingType.SUPPLYBOOST;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.AugmentDrawRateFactor(drawRateMultiplier);
    }

    public override void OnClaimLost()
    {
        owner.AugmentDrawRateFactor(-drawRateMultiplier);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.gearIcon;
    }

    protected override string GetName()
    {
        return "Supply Boost";
    }

    protected override string GetDescription()
    {
        return "+" + 100 * drawRateMultiplier + "%" + 
            " piece production rate";
    }
}

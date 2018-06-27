using UnityEngine;
using System.Collections;

public class MiningDrill : Structure
{
    private const float resourceGainIncrementMultiplier = 0.15f;

    public MiningDrill() : base(0)
    {
        buildingType = BuildingType.MININGDRILL;
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

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.drillIcon;
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


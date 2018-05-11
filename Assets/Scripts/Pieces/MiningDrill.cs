using UnityEngine;
using System.Collections;

public class MiningDrill : Structure
{
    private const float resourceGainIncrementMultiplier = 0.1f;

    public MiningDrill() : base(0)
    {
        buildingType = BuildingType.MININGDRILL;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.AugmentResourceGainIncrementFactor(resourceGainIncrementMultiplier);
    }

    public override void OnClaimLost()
    {
        owner.AugmentResourceGainIncrementFactor(-resourceGainIncrementMultiplier);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.drillIcon;
    }

    protected override string GetName()
    {
        return "Fast Forward";
    }

    protected override string GetDescription()
    {
        return "Increases Play Rate by <color=green>" 
            + 100 * resourceGainIncrementMultiplier + "</color>%";
    }

}


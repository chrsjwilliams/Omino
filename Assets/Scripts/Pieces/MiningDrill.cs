using UnityEngine;
using System.Collections;

public class MiningDrill : Structure
{
    private const float resourceGainIncrementMultiplier = 0.1f;

    public MiningDrill() : base(0)
    {
        isFortified = true;
        buildingType = BuildingType.MININGDRILL;
    }

    protected override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.AugmentResourceGainIncrementFactor(resourceGainIncrementMultiplier);
    }

    protected override void OnClaimLost()
    {
        owner.AugmentResourceGainIncrementFactor(-resourceGainIncrementMultiplier);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.drillIcon;
    }

}


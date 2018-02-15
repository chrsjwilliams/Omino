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
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.drillIcon;
    }

    protected override string GetName()
    {
        return "Mining Drill";
    }

    protected override string GetDescription()
    {
        return "Increases Brick Production by <color=green>" 
            + 100 * resourceGainIncrementMultiplier + "</color>%";
    }

}


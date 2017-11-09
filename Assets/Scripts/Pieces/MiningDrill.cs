using UnityEngine;
using System.Collections;

public class MiningDrill : Structure
{
    public MiningDrill() : base(7, 0)
    {
        isFortified = true;
        buildingType = BuildingType.MININGDRILL;
    }

    protected override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.AugmentResourceGainIncrementFactor(0.2f);
    }

    protected override void OnClaimLost()
    {
        owner.AugmentResourceGainIncrementFactor(-0.2f);
        base.OnClaimLost();
    }

}


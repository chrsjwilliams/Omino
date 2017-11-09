using UnityEngine;
using System.Collections;

public class AssemblyLine : Structure

{
    public AssemblyLine() : base(7, 1)
    {
        buildingType = BuildingType.ASSEMBLYLINE;
    }

    protected override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.AugmentDrawRateFactor(0.2f);
    }

    protected override void OnClaimLost()
    {
        owner.AugmentDrawRateFactor(-0.2f);
        base.OnClaimLost();
    }
}

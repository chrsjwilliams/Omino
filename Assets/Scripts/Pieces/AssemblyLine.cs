using UnityEngine;
using System.Collections;

public class AssemblyLine : Structure

{
    public AssemblyLine() : base(7, 1)
    {
        isFortified = true;
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

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.gearIcon;
    }
}

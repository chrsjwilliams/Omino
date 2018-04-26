using UnityEngine;
using System.Collections;

public class AssemblyLine : Structure
{
    private const float drawRateMultiplier = 0.1f;

    public AssemblyLine() : base(1)
    {
        isFortified = true;
        buildingType = BuildingType.ASSEMBLYLINE;
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
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.gearIcon;
    }

    protected override string GetName()
    {
        return "Supply Boost";
    }

    protected override string GetDescription()
    {
        return "Increases piece draw rate by <color=green>"
            + 100 * drawRateMultiplier + "</color>%";
    }
}

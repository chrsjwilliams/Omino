using UnityEngine;
using System.Collections;

public class SplashDamage : Structure
{
    public SplashDamage() : base(0)
    {
        buildingType = BuildingType.SPLASHDAMAGE;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleSplashDamage(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleSplashDamage(false);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.splashStructIcon;
    }

    protected override string GetName()
    {
        return "Combustion";
    }

    protected override string GetDescription()
    {
        return "<color=red>Attack</color> pieces explode, destroying adjacent enemy " +
            "pieces and themeselves.";
    }
}

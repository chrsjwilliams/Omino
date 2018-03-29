using UnityEngine;
using System.Collections;

public class SplashDamage : Structure
{
    public SplashDamage() : base(0)
    {
        buildingType = BuildingType.SPLASHDAMAGE;
        isFortified = true;
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
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.splashIcon;
    }

    protected override string GetName()
    {
        return "Splash Damage";
    }

    protected override string GetDescription()
    {
        return "<color=red>Attack</color> roads also destroy adjacent enemy " +
            "roads, but are removed after placement.";
    }
}

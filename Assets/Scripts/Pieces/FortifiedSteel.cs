using UnityEngine;
using System.Collections;

public class FortifiedSteel : Structure
{
    public FortifiedSteel() : base(2)
    {
        buildingType = BuildingType.FORTIFIEDSTEEL;
        isFortified = true;
    }

    protected override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleAutoFortify(true);
    }

    protected override void OnClaimLost()
    {
        owner.ToggleAutoFortify(false);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.steelIcon;
    }
}

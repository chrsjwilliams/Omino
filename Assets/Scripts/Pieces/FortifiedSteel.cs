using UnityEngine;
using System.Collections;

public class FortifiedSteel : Structure
{
    public FortifiedSteel() : base(2)
    {
        buildingType = BuildingType.FORTIFIEDSTEEL;
        isFortified = true;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleAutoFortify(true);
    }

    public override void OnClaimLost()
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

    protected override string GetName()
    {
        return "Fortified Steel";
    }

    protected override string GetDescription()
    {
        return "All newly placed pieces are automatically fortified";
    }
}

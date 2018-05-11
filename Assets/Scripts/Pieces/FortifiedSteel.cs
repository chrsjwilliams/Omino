using UnityEngine;
using System.Collections;

public class FortifiedSteel : Structure
{
    public FortifiedSteel() : base(2)
    {
        buildingType = BuildingType.FORTIFIEDSTEEL;
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
        holder.icon.sprite = Services.UIManager.steelIcon;
    }

    protected override string GetName()
    {
        return "Fortified Steel";
    }

    protected override string GetDescription()
    {
        return "Newly placed pieces are split up into individual bricks";
    }
}

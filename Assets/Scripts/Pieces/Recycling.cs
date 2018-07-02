using UnityEngine;
using System.Collections;

public class Recycling : Structure
{
    public Recycling() : base(1)
    {
        buildingType = BuildingType.RECYCLING;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleRecycling(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleRecycling(false);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.recyclingIcon;
    }

    protected override string GetName()
    {
        return "Recycling";
    }

    protected override string GetDescription()
    {
        return "Whenever one of your pieces is destroyed by a hammer, draw a piece.";
    }
}

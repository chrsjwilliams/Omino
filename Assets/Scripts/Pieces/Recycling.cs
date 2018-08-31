using UnityEngine;
using System.Collections;

public class Recycling : TechBuilding
{
    public Recycling() : base(1)
    {
        buildingType = BuildingType.RECYCLING;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleRecycling(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleRecycling(false);
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        OnClaimEffect(player);
    }

    public override void OnClaimLost()
    {
        OnLostEffect();
        base.OnClaimLost();
    }

    public override string GetName()
    {
        return "Recycling";
    }

    public override string GetDescription()
    {
        return "Whenever one of your pieces is destroyed, get a new piece.";
    }
}

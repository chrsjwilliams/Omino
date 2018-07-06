using UnityEngine;
using System.Collections;

public class Upsize : TechBuilding
{
    public Upsize() : base(0)
    {
        buildingType = BuildingType.UPSIZE;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleBiggerBricks(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleBiggerBricks(false);
        base.OnClaimLost();
    }

    protected override string GetName()
    {
        return "Upsize";
    }

    protected override string GetDescription()
    {
        return "Newly drawn normal pieces are 5 blocks rather than 4";
    }
}

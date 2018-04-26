using UnityEngine;
using System.Collections;

public class BiggerBricks : Structure
{
    public BiggerBricks() : base(0)
    {
        buildingType = BuildingType.BIGGERBRICKS;
        isFortified = true;
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

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.bricksIcon;
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

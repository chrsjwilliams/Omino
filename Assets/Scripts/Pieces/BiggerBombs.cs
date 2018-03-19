using UnityEngine;
using System.Collections;

public class BiggerBombs : Structure
{
    public BiggerBombs() : base(0)
    {
        buildingType = BuildingType.BIGGERBOMBS;
        isFortified = true;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleBiggerBombs(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleBiggerBombs(false);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        iconSr.enabled = true;
        iconSr.sprite = Services.UIManager.bigBombIcon;
    }

    protected override string GetName()
    {
        return "Bigger Militia";
    }

    protected override string GetDescription()
    {
        return "Newly drawn <color=red>Attack</color> pieces are 4 blocks rather than 3";
    }
}

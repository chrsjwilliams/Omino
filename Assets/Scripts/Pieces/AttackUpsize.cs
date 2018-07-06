using UnityEngine;
using System.Collections;

public class AttackUpsize : TechBuilding
{
    public AttackUpsize() : base(0)
    {
        buildingType = BuildingType.ATTACKUPSIZE;
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
        holder.icon.sprite = Services.UIManager.bigBombIcon;
    }

    protected override string GetName()
    {
        return "Attack Upsize";
    }

    protected override string GetDescription()
    {
        return "Newly drawn <color=red>Attack</color> pieces are 4 blocks rather than 3";
    }
}

using UnityEngine;
using System.Collections;

public class Fission : TechBuilding
{
    public const float energyReward = 0.5f;

    public Fission() : base(1)
    {
        buildingType = BuildingType.FISSION;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleFission(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleFission(false);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.fissionIcon;
    }

    protected override string GetName()
    {
        return "Fission";
    }

    protected override string GetDescription()
    {
        return "Whenever you destroy an opponent's piece with a hammer, gain " + energyReward + " energy.";
    }
}

using UnityEngine;
using System.Collections;

public class Fission : TechBuilding
{
    public const float energyReward = 0.5f;

    public Fission() : base(1)
    {
        label = "Fission";

        buildingType = BuildingType.FISSION;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleFission(true);

    }

    public override void OnLostEffect()
    {
        owner.ToggleFission(false);
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

    protected override string GetName()
    {
        return "Fission";
    }

    public override string GetDescription()
    {
        return "Whenever you destroy an opponent's piece with a hammer, gain " + energyReward + " energy.";
    }
}

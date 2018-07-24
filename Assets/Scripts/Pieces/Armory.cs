using UnityEngine;
using System.Collections;

public class Armory : TechBuilding
{
    private const float attackFactorModifier = 0.15f;

    public Armory() : base(1)
    {
        label = "Armory";

        buildingType = BuildingType.ARMORY;
    }

    public override void OnClaimEffect(Player player)
    {
        player.AugmentAttackGainFactor(attackFactorModifier);
    }

    public override void OnLostEffect()
    {
        owner.AugmentAttackGainFactor(-attackFactorModifier);
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
        return "Armory";
    }

    public override string GetDescription()
    {
        return "+" + 100 * attackFactorModifier + "%" +
            " Hammer Production rate";
    }
}

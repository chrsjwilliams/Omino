using UnityEngine;
using System.Collections;

public class Armory : TechBuilding
{
    private const float attackFactorModifier = 0.15f;

    public Armory() : base(1)
    {
        buildingType = BuildingType.ARMORY;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.AugmentAttackGainFactor(attackFactorModifier);
    }

    public override void OnClaimLost()
    {
        owner.AugmentAttackGainFactor(-attackFactorModifier);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.hammerOutline;
    }

    protected override string GetName()
    {
        return "Armory";
    }

    protected override string GetDescription()
    {
        return "+" + 100 * attackFactorModifier + "%" +
            " Hammer Production rate";
    }
}

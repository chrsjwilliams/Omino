using UnityEngine;
using System.Collections;

public class ShieldedPieces : TechBuilding
{
    public const float ShieldDuration = 5f;

    public ShieldedPieces() : base(0)
    {
        buildingType = BuildingType.SHIELDEDPIECES;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleShieldedPieces(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleShieldedPieces(false);
    }

    public override void OnClaim(Player player)
    {
        OnClaimEffect(player);
        base.OnClaim(player);
    }

    public override void OnClaimLost()
    {
        OnLostEffect();
        base.OnClaimLost();
    }

    public override string GetName()
    {
        return "Shielded Pieces";
    }

    public override string GetDescription()
    {
        return "Your pieces can't be destroyed for " +
            ShieldDuration + " seconds after placement.";
    }
}

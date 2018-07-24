using UnityEngine;
using System.Collections;

public class ShieldedPieces : TechBuilding
{
    public const float ShieldDuration = 6f;

    public ShieldedPieces() : base(0)
    {
        label = "Shielded Pieces";
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

    protected override string GetName()
    {
        return "Shielded Pieces";
    }

    public override string GetDescription()
    {
        return "Pieces gain a temporary shield after they're placed. They can't be destroyed until " +
            ShieldDuration + " seconds after placement.";
    }
}

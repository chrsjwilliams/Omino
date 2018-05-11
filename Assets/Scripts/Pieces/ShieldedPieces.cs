using UnityEngine;
using System.Collections;

public class ShieldedPieces : Structure
{
    public const float ShieldDuration = 8f;

    public ShieldedPieces() : base(0)
    {
        buildingType = BuildingType.SHIELDEDPIECES;
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.ToggleShieldedPieces(true);
    }

    public override void OnClaimLost()
    {
        owner.ToggleShieldedPieces(false);
        base.OnClaimLost();
    }

    protected override void SetIconSprite()
    {
        base.SetIconSprite();
        holder.icon.sprite = Services.UIManager.shieldIcon;
    }

    protected override string GetName()
    {
        return "Shielded Pieces";
    }

    protected override string GetDescription()
    {
        return "Newly placed pieces gain a temporary shield. They can't be destroyed until " +
            ShieldDuration + " seconds after placement.";
    }
}

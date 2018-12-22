using UnityEngine;
using System.Collections;
using System;

public class Base : TechBuilding
{
    public bool mainBase { get; private set; }

    public const float normalDrawRate = 0.1f;
    public const float destDrawRate = 0.03f;
    public const float resourceGainRate = 0.09f;

    public Base() : base(0)
    {
        buildingType = BuildingType.BASE;
    }

    public Base(Player _player, bool _mainBase) : base(9, 0, _player)
    {
        if (owner != null) holderName = "Player " + owner.playerNum + " Base";
        else holderName = "Neutral Base";
        mainBase = _mainBase;
        owner = _player;

    }

    public override void Update()
    {
        //UpdateDrawMeter();
        //UpdateResourceMeter();
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        TogglePieceConnectedness(true);
        player.AddActiveExpansion(this);
    }

    public override void OnClaimLost()
    {
        owner.RemoveActiveExpansion(this);
        base.OnClaimLost();
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        if (mainBase)
        {
            holder.icon.sprite = Services.UIManager.baseOverlay;
            holder.spriteBottom.sprite = Services.UIManager.homeBaseSprites[owner.playerNum-1];
            holder.SetTechStatus(owner, true);
        }
        else
        {
            holder.icon.sprite = Services.UIManager.sideBaseOverlay;
            holder.SetTechStatus(owner);
        }
    }

    public override string GetName()
    {
        if (mainBase)
        {
            return "Home Base";
        }
        else
        {
            return "Expansion";
        }
    }

    public override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)normalDrawRateBonus, 3) + 
        //    "</color> normal pieces per second" + "\n" +
        //    "+<color=green>" + Math.Round((double)destructorDrawRateBonus, 3) +
        //    "</color> <color=red>DESTRUCTIVE</color> pieces per second" + "\n" +
        //    "+<color=green>" + Math.Round((double)resourceGainRateBonus * 10, 3) +
        //    "</color> resources per second";
        if (mainBase)
        {
            return "Home base. Connect to your opponent's home base to win.";
        }
        else
        {
            return "+1 Piece Production Level\n" +
                "+1 Energy Recharge Level\n" +
                "+1 <color=red>Attack</color> Recharge Level";

        }
    }
}

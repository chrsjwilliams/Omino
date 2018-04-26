using UnityEngine;
using System.Collections;
using System;

public class Base : Structure
{
    public bool mainBase { get; private set; }

    public const float normalDrawRate = 0.06f;
    public const float destDrawRate = 0.03f;
    public const float resourceGainRate = 0.08f;

    public Base() : base(0)
    {
        buildingType = BuildingType.BASE;
        isFortified = true;
    }

    public Base(Player _player, bool _mainBase) : base(9, 0, _player)
    {
        mainBase = _mainBase;
        owner = _player;
    }

    public override void Update()
    {
        //UpdateDrawMeter();
        //UpdateResourceMeter();
    }

    protected override void OnPlace()
    {
        ToggleCostUIStatus(false);
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
        secondOverlay.enabled = false;
        if (mainBase)
        {
            iconSr.sprite = Services.UIManager.baseOverlay;
            spriteOverlay.sprite = Services.UIManager.baseBottom;
        }
        else
        {
            iconSr.sprite = Services.UIManager.sideBaseOverlay;
        }
    }

    protected override string GetName()
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

    protected override string GetDescription()
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
            return "Normal Piece Draw Level +1 \n" +
                "<color=red>Attack</color> Piece Draw Level +1 \n" +
                "Play Rate Level +1";
        }
    }
}

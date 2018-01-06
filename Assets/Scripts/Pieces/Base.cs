﻿using UnityEngine;
using System.Collections;

public class Base : Structure
{
    public bool mainBase { get; private set; }

    public Base() : base(0)
    {
        normalDrawRateBonus = 1f / 30f;
        resourceGainRateBonus = 1f / 10f;
        buildingType = BuildingType.BASE;
        isFortified = true;
    }

    public Base(Player _player, bool _mainBase) : base(9, 0, _player)
    {
        mainBase = _mainBase;
        owner = _player;
        resourceGainRateBonus = 1f / 5f;
        normalDrawRateBonus = 1f / 15f;
    }

    public override void ToggleStructureActivation(Player player)
    {
        if (!mainBase) base.ToggleStructureActivation(player);
    }

    public override void Update()
    {
        //UpdateDrawMeter();
        //UpdateResourceMeter();
    }

    protected override void OnPlace()
    {
        //CreateTimerUI();
        ToggleCostUIStatus(false);
        if (owner != null)
        {
            owner.AugmentNormalDrawRate(normalDrawRateBonus);
            owner.AugmentResourceGainRate(resourceGainRateBonus);
        }
    }

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        TogglePieceConnectedness(true);
        player.AugmentNormalDrawRate(normalDrawRateBonus);
        player.AugmentResourceGainRate(resourceGainRateBonus);
    }

    public override void OnClaimLost()
    {
        owner.AugmentNormalDrawRate(-normalDrawRateBonus);
        owner.AugmentResourceGainRate(-resourceGainRateBonus);
        base.OnClaimLost();
    }

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        if (mainBase)
        {
            spriteOverlay.sprite = Services.UIManager.baseOverlay;
        }
        else { spriteOverlay.sprite = Services.UIManager.structureOverlay; }
    }

    protected override string GetName()
    {
        return "Base";
    }

    protected override string GetDescription()
    {
        return "+1 normal piece every " + "<color=green>" 
            + Mathf.RoundToInt(1 / normalDrawRateBonus)
            + "</color>" +" s" + "\n\n" +
            "+10 resources every " + "<color=green>" 
            + Mathf.RoundToInt(1/resourceGainRateBonus) + "</color>" + " s";
    }
}

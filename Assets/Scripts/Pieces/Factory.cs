using UnityEngine;
using System.Collections;
using System;

public class Factory : Blueprint
{
    public Factory(Player player_) : base(BuildingType.FACTORY, player_)
    {
        normalDrawRateBonus = 1f / 30f;
    }

    public override void Update()
    {
        base.Update();
        //UpdateDrawMeter();
    }

    public override void Remove()
    {
        owner.AugmentNormalDrawRate(-normalDrawRateBonus);
        base.Remove();
        //RemoveTimerUI();
    }

    protected override void OnPlace()
    {
        base.OnPlace();
        owner.AugmentNormalDrawRate(normalDrawRateBonus);
        //CreateTimerUI();
    }

    protected override string GetName()
    {
        return "Factory";
    }

    protected override string GetDescription()
    {
        return "+<color=green>" + Math.Round((double)normalDrawRateBonus, 2) + 
            "</color> normal pieces per second";
    }
}

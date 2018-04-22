using UnityEngine;
using System.Collections;
using System;

public class Factory : Blueprint
{
    public static float drawRateBonus;

    public Factory(Player player_) : base(BuildingType.FACTORY, player_)
    {
        normalDrawRateBonus = 0.04f;
        drawRateBonus = normalDrawRateBonus;
        onGainText = "+1 Normal Piece Production";
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
        //owner.AugmentNormalDrawRate(normalDrawRateBonus);
        //CreateTimerUI();
    }

    public override void OnConnect()
    {
        base.OnConnect();
        owner.AugmentNormalDrawRate(normalDrawRateBonus);
    }

    public override void OnDisconnect()
    {
        base.OnDisconnect();
        owner.AugmentNormalDrawRate(-normalDrawRateBonus);
    }

    protected override string GetName()
    {
        return "Brickworks";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)normalDrawRateBonus, 3) + 
        //    "</color> normal pieces per second";
        return "Normal Road Production Level +1";
    }
}

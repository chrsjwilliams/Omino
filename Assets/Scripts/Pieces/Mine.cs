using UnityEngine;
using System.Collections;
using System;

public class Mine : Blueprint
{
    public Mine(Player player_) : base(BuildingType.MINE, player_)
    {
        resourceGainRateBonus = 1f / 8f;
        onGainText = "+1 Hammer Production";
    }

    public override void Update()
    {
        base.Update();
        //UpdateResourceMeter();
    }

    protected override void OnPlace()
    {
        base.OnPlace();
        //owner.AugmentResourceGainRate(resourceGainRateBonus);
    }

    public override void Remove()
    {
        owner.AugmentResourceGainRate(-resourceGainRateBonus);
        base.Remove();
    }

    public override void OnDisconnect()
    {
        base.OnDisconnect();
        owner.AugmentResourceGainRate(-resourceGainRateBonus);
    }

    public override void OnConnect()
    {
        base.OnConnect();
        owner.AugmentResourceGainRate(resourceGainRateBonus);
    }

    protected override string GetName()
    {
        return "Smith";
    }

    protected override string GetDescription()
    {
        //return "+<color=green>" + Math.Round((double)resourceGainRateBonus * 10, 3) + 
        //    "</color> resources per second";
        return "Hammer Production Level +1";
    }
}

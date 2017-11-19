using UnityEngine;
using System.Collections;

public class Factory : Blueprint
{
    public Factory(Player player_) : base(BuildingType.FACTORY, player_)
    {
        baseDrawPeriod = 30f;
    }

    public override void Update()
    {
        base.Update();
        UpdateDrawMeter();
    }

    public override void Remove()
    {
        base.Remove();
        RemoveTimerUI();
    }

    protected override void OnPlace()
    {
        base.OnPlace();
        CreateTimerUI();
    }
}

using UnityEngine;
using System.Collections;

public class BombFactory : Blueprint
{
    public BombFactory(Player owner_) : base(BuildingType.BOMBFACTORY, owner_)
    {
        baseDrawPeriod = 45f;
    }

    public override void Update()
    {
        base.Update();
        UpdateDrawMeter();
    }

    protected override void OnDraw()
    {
        owner.DrawPieces(1, holder.transform.position, true);
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

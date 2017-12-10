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

    protected override void SetOverlaySprite()
    {
        base.SetOverlaySprite();
        spriteOverlay.transform.position = GetCenterpoint(true);
    }

    protected override string GetName()
    {
        return "Bomb Factory";
    }

    protected override string GetDescription()
    {
        return "Creates a new <color=red>Destructor</color> piece every " + "<color=green>" 
            + Mathf.RoundToInt(1 / drawRate)
            + "</color>" + " s";
    }
}

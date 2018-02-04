using UnityEngine;
using System.Collections;
using System;

public class BombFactory : Blueprint
{
    public BombFactory(Player owner_) : base(BuildingType.BOMBFACTORY, owner_)
    {
        destructorDrawRateBonus = 1f / 60f;
    }

    public override void Update()
    {
        base.Update();
        //UpdateDrawMeter();
    }

    //protected override void OnDraw()
    //{
    //    owner.DrawPieces(1, holder.transform.position, true);
    //}

    public override void Remove()
    {
        owner.AugmentDestructorDrawRate(-destructorDrawRateBonus);
        base.Remove();
        //RemoveTimerUI();
    }

    protected override void OnPlace()
    {
        base.OnPlace();
        owner.AugmentDestructorDrawRate(destructorDrawRateBonus);
        //CreateTimerUI();
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
        return "+<color=green>" + Math.Round((double)destructorDrawRateBonus, 3) + 
            "</color> <color=red>DESTRUCTIVE</color> pieces per second";
    }
}

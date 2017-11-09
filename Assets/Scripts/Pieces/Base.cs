using UnityEngine;
using System.Collections;

public class Base : Structure
{
    public Base(int _units, int _index, Player _player) : base(_units, _index, _player)
    {
        owner = _player;
        baseDrawPeriod = 15f;
        baseResourceIncrementPeriod = 3f;
        baseResourcesPerIncrement = 10;
    }

    public override void ActivateStructureCheck() { }

    public override void Update()
    {
        UpdateDrawMeter();
        UpdateResourceMeter();
    }

    protected override void OnPlace()
    {
        CreateTimerUI();
        ToggleCostUIStatus(false);
    }

    public override void OnInputUp() { }

    protected override void OnClaim(Player player)
    {
        base.OnClaim(player);
        owner.GainOwnership(this);
    }

    protected override void OnClaimLost()
    {
        owner.LoseOwnership(this);
        base.OnClaimLost();
    }
}

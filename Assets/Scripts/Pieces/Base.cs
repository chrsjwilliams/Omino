using UnityEngine;
using System.Collections;

public class Base : Structure
{
    public bool mainBase { get; private set; }
    public Base(Player _player, bool _mainBase) : base(9, 0, _player)
    {
        mainBase = _mainBase;
        owner = _player;
        baseResourceIncrementPeriod = 5f;
        if (owner != null)
        {
            baseDrawPeriod = 15f;
            baseResourcesPerIncrement = 10;
        }
        else
        {
            baseDrawPeriod = 30f;
            baseResourcesPerIncrement = 5;
        }
    }

    public override void ToggleStructureActivation(Player player)
    {
        if (!mainBase) base.ToggleStructureActivation(player);
    }

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

    public override void OnClaim(Player player)
    {
        base.OnClaim(player);
        TogglePieceConnectedness(true);
    }
}

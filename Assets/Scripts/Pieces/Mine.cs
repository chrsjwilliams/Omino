using UnityEngine;
using System.Collections;

public class Mine : Blueprint
{
    public Mine(Player player_) : base(BuildingType.MINE, player_)
    {
        baseResourceIncrementPeriod = 5f;
        baseResourcesPerIncrement = 5;
    }

    public override void Update()
    {
        base.Update();
        UpdateResourceMeter();
    }

    protected override string GetName()
    {
        return "Mine";
    }

    protected override string GetDescription()
    {
        return "Produces " + "<color=green>" + resourcesPerIncrement + "</color>"
            + " resources every " + "<color=green>"
            + Mathf.RoundToInt(1 / resourceIncrementRate) + "</color>" + " s";
    }
}

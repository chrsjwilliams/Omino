

public class Combustion : TechBuilding
{
    public Combustion() : base(0)
    {
        buildingType = BuildingType.COMBUSTION;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleCombustion(true);
    }


    public override void OnLostEffect()
    {
        owner.ToggleCombustion(false);
    }

    public override void OnClaim(Player player)
    {
        OnClaimEffect(player);
        base.OnClaim(player);
    }

    public override void OnClaimLost()
    {
        OnLostEffect();
        base.OnClaimLost();
    }

    public override string GetName()
    {
        return "Combustion";
    }

    public override string GetDescription()
    {
        return "<color=red>Attack</color> pieces explode, destroying adjacent enemy " +
            "pieces and themselves.";
    }
}



public class Plunder : TechBuilding
{
    public Plunder() : base(0)
    {
        buildingType = BuildingType.PLUNDER;
    }

    public override void OnClaimEffect(Player player)
    {
        player.TogglePlunder(true);
    }

    public override void OnLostEffect()
    {
        owner.TogglePlunder(false);
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
        return "Plunder";
    }

    public override string GetDescription()
    {
        return "Whenever you destroy an opponent's piece, get a new piece.";
    }
}



public class Upsize : TechBuilding
{
    public Upsize() : base(0)
    {
        buildingType = BuildingType.UPSIZE;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleBiggerBricks(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleBiggerBricks(false);
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
        return "Upsize";
    }

    public override string GetDescription()
    {
        return "Draw bigger pieces.";
    }
}

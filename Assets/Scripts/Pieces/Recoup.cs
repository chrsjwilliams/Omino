
public class Recoup : TechBuilding
{
    public const float energyReward = 0.5f;
    public Recoup() : base(0)
    {
        buildingType = BuildingType.RECOUP;
    }

    public override void OnClaimEffect(Player player)
    {
        player.ToggleRecoup(true);
    }

    public override void OnLostEffect()
    {
        owner.ToggleRecoup(false);
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
        return "Recoup";
    }

    public override string GetDescription()
    {
        return "Whenever one of your pieces is destroyed, gain " + energyReward + " energy.";
    }
}

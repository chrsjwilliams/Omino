public class DisablePlayerMovement : GameEvent
{
    public readonly bool toggleMovement;
    public DisablePlayerMovement(bool _toggleMovement)
    {
        toggleMovement = _toggleMovement;
    }
}
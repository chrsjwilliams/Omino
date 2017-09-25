public class PlacePieceEvent : GameEvent
{
    public readonly Player player;
    public PlacePieceEvent(Player _player)
    {
        player = _player;
    }
}

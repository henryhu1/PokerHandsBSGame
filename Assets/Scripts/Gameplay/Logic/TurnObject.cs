public class TurnObject
{
    public ulong ClientId { get; set; }
    public TurnObject Next { get; set; }
    public TurnObject Previous { get; set; }

    public TurnObject(ulong clientId, TurnObject next, TurnObject previous)
    {
        ClientId = clientId;
        Next = next;
        Previous = previous;
    }
}

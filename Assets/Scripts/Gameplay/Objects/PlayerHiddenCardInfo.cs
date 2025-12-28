using Unity.Netcode;

public struct PlayerHiddenCardInfo : INetworkSerializable
{
    public int amountOfCards;
    public string playerName;
    public ulong clientId;

    public PlayerHiddenCardInfo(int amountOfCards, string playerName, ulong clientId)
    {
        this.amountOfCards = amountOfCards;
        this.playerName = playerName;
        this.clientId = clientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref amountOfCards);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref clientId);
    }
}

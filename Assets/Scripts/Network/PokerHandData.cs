using Unity.Netcode;

public struct PokerHandData : INetworkSerializable
{
    public HandType handType;
    public Rank rankPrimary;
    public Rank rankSecondary;
    public Suit suit;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref handType);
        serializer.SerializeValue(ref rankPrimary);
        serializer.SerializeValue(ref rankSecondary);
        serializer.SerializeValue(ref suit);
    }
}
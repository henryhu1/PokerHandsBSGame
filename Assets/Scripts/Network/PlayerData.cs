using Unity.Netcode;

public class PlayerData : INetworkSerializable
{
    private ulong lastUsedClientId;
    private string playerAuth;
    private string name;
    public PlayerState state;

    public PlayerData(ulong clientId, string authId, string playerName, PlayerState initState)
    {
        lastUsedClientId = clientId;
        playerAuth = authId;
        name = playerName;
        state = initState;
    }

    public ulong GetClientId() => lastUsedClientId;
    public string GetName() => name;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref lastUsedClientId);
    }
}

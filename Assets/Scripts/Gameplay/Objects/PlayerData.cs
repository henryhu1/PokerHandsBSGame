using Unity.Netcode;

// Player and client data, TODO: player manager? Single source of truth for players, ^ also look at m_numberOfPlayers ^
public class PlayerData : INetworkSerializable

{
    public bool IsConnected { get; set; }
    private ulong m_lastUsedClientId;
    public ulong LastUsedClientID { get { return m_lastUsedClientId; } set { m_lastUsedClientId = value; } }
    private string m_name;
    public string Name { get { return m_name; } set { m_name = value; } }
    public bool InPlay { get; set; }
    public PlayerState state;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_name);
        serializer.SerializeValue(ref m_lastUsedClientId);
    }
}

using System;
using System.IO;

public static class StreamUtils
{
    public static byte[] WritePlayerNameId(string playerName, string playerId)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(LobbyManager.Instance.PlayerName);
                writer.Write(PokerHandsBullshitGame.Instance.LocalPlayerId);
                return stream.ToArray();
            }
        }
    }

    public static (string, string) ReadPlayerNameId(byte[] data)
    {
        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                string playerName = reader.ReadString();
                string playerId = reader.ReadString();
                return (playerName, playerId);
            }
        }
    }
}

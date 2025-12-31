using System.IO;
using UnityEngine;

public static class LocalPlayerSaveSystem
{
    private static readonly string FilePath =
        Path.Combine(Application.persistentDataPath, "player.json");

    public static void SavePlayerName(string playerName)
    {
        var data = new LocalPlayerData
        {
            playerName = playerName
        };

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(FilePath, json);
    }

    public static string LoadPlayerName()
    {
        if (!File.Exists(FilePath))
            return string.Empty;

        string json = File.ReadAllText(FilePath);
        var data = JsonUtility.FromJson<LocalPlayerData>(json);

        return data.playerName;
    }
}

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class ServerPlayerManager
{
    private Dictionary<string, PlayerData> allPlayerData;
    public Dictionary<ulong, string> clientIdToPlayerIdMap;

    public void PlayerJoining(ulong clientId, string clientName, string playerId)
    {
        if (allPlayerData.TryGetValue(playerId, out PlayerData playerData))
        {
            playerData.LastUsedClientID = clientId;
            playerData.Name = clientName;
            playerData.IsConnected = false;
            playerData.state = PlayerState.SPECTATING;
            playerData.InPlay = false;
        }
        else
        {
            playerData = new()
            {
                LastUsedClientID = clientId,
                Name = clientName,
                IsConnected = false,
                state = PlayerState.SPECTATING,
                InPlay = false,
            };

            allPlayerData.Add(playerId, playerData);
        }
        clientIdToPlayerIdMap[clientId] = playerId;

        if (SceneTransitionHandler.Instance.IsInGameScene())
        {
            playerData.InPlay = false;
        }
    }

    public void AddPlayer(ulong clientId)
    {
        // if (IsServer)
        // {
            var notInPlayClientIds = allPlayerData.Values
                .Where(playerData => { return !playerData.InPlay; })
                .Select(playerData => { return playerData.LastUsedClientID; })
                .ToList();
            if (!notInPlayClientIds.Contains(clientId))
            {
                PlayerData playerData = allPlayerData[clientIdToPlayerIdMap[clientId]];
                playerData.InPlay = true;
            }
        // }
    }

    public void RemovePlayer(ulong clientId)
    {
        // if (IsServer)
        // {
            if (!clientIdToPlayerIdMap.ContainsKey(clientId)) return;
            if (!NetworkManager.Singleton.IsConnectedClient) return;

            string playerId = clientIdToPlayerIdMap[clientId];
            if (allPlayerData.TryGetValue(playerId, out PlayerData playerData))
            {
                if (SceneTransitionHandler.Instance.IsInGameScene())
                {
                    if (playerData.state == PlayerState.PLAYING || playerData.state == PlayerState.ELIMINATED)
                    {
                        // TODO: fix error occurring here when game ends in GameScene and despawns on the network
                        // inPlayClientIds.Remove(clientId);
                        // TODO: DO in ConnectionManager
                        if (!RoundManager.Instance.GetIsRoundOver())
                        {
                            // TODO: invoke two events instead
                            //   one to signal a player left
                            //   another to end the round
                            GameManager.Instance.SetLosingPlayer(playerData);
                            TurnManager.Instance.StopServerTurnCountdown();
                            CardManager.Instance.RevealAllCards();
                        }
                    }
                }

                playerData.IsConnected = false;
                playerData.InPlay = false;
                clientIdToPlayerIdMap.Remove(clientId);
            }
        // }
    }

    public int GetNumberOfInGamePlayers()
    {
        return allPlayerData.Values
            .Count(playerData =>
                {
                    return playerData.state == PlayerState.PLAYING || playerData.state == PlayerState.ELIMINATED;
                }
            );
    }
}

using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public async Task<string> CreateRelay(int numberOfPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberOfPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (!NetworkManager.Singleton.StartHost())
            {
                throw new Exception("Failed to start host");
            }

            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);

            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = StreamUtils.WritePlayerNameId(LobbyManager.Instance.PlayerName, PokerHandsBullshitGame.Instance.LocalPlayerId);

            // TODO: UI indication of 1. connecting 2. game starting 3. LobbyUI disappears 4. Scene transition
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client connected");
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    //public async void DeleteRelay(Allocation allocation)
    //{
    //    try
    //    {
    //        await RelayService.Instance.
    //    }
    //}
}

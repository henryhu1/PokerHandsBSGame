using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public async Task<string> CreateRelay(int numberOfPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberOfPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new(allocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (!NetworkManager.Singleton.StartHost())
            {
                throw new Exception("Failed to start host");
            }

            BootstrapNetwork();
            return joinCode;
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to create relay: {e.Message}");
        }
    }

    public async void JoinRelay(string joinCode)
    {
        if (joinCode == null) { return; }
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new(joinAllocation, "wss");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = StreamUtils.WritePlayerNameId(PlayerManager.Instance.GetLocalPlayerName(), AuthenticationService.Instance.PlayerId);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
#if UNITY_EDITOR
            Debug.LogError(e.Message);
#endif
        }
    }

    private void BootstrapNetwork()
    {
        ConnectionManager.Instance.RegisterNetworkCallbacks();
        SceneTransitionHandler.Instance.RegisterNetworkCallbacks();
    }
}

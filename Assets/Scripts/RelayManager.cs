using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    // private string m_InGameSceneName = "GameScene";
    private int m_numberOfPlayers;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // PokerHandsBullshitGame.Instance.m_allPlayersConnected.OnValueChanged += (oldValue, newValue) =>
        // {
        //     if (newValue)
        //     {
        //         SceneTransitionHandler.Instance.RegisterCallbacks();
        //         SceneTransitionHandler.Instance.SwitchScene(m_InGameSceneName);
        //     }
        // };
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        ulong clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        byte[] connectionData = request.Payload;

        string playerName = Encoding.ASCII.GetString(connectionData);
        if (string.IsNullOrEmpty(playerName)) // Host is joining first and does not send connection data
        {
            playerName = LobbyManager.Instance.GetPlayerName();
        }
        PokerHandsBullshitGame.Instance.PlayerJoining(clientId, playerName);

        // Your approval logic determines the following values
        response.Approved = true;
        response.CreatePlayerObject = false;

        // The Prefab hash value of the NetworkPrefab, if null the default NetworkManager player Prefab is used
        response.PlayerPrefabHash = null;

        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;

        // If response.Approved is false, you can provide a message that explains the reason why via ConnectionApprovalResponse.Reason
        // On the client-side, NetworkManager.DisconnectReason will be populated with this message via DisconnectReasonMessage
        // response.Reason = "Some reason for not approving the client";

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
    }

    public async Task<string> CreateRelay(int numberOfPlayers)
    {
        m_numberOfPlayers = numberOfPlayers;
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(m_numberOfPlayers);
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
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(LobbyManager.Instance.GetPlayerName());

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

using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    private Dictionary<ulong, ClientConnectionData> clientConnectionMap;

    [SerializeField] private float clientConnectionCheckInterval = 5.0f;
    private float clientConnectionCheckTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        clientConnectionMap = new();

        clientConnectionCheckTimer = clientConnectionCheckInterval;

        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    private void Update()
    {
        if (!SceneTransitionHandler.Instance.IsInMainMenuScene())
        {
            clientConnectionCheckTimer -= Time.deltaTime;
            if (clientConnectionCheckTimer <= 0)
            {
                if (!NetworkManager.Singleton.IsConnectedClient)
                {
                    clientConnectionMap.Clear();
                    SceneTransitionHandler.Instance.ExitAndLoadStartMenu();
                    clientConnectionCheckTimer = clientConnectionCheckInterval;
                }
            }
        }
    }

    public void PlayerJoining(ulong clientId, string clientName, string playerId)
    {
        clientConnectionMap.Add(clientId, new ClientConnectionData
        {
            DisplayName = clientName,
            LastUsedClientId = clientId,
            AuthId = playerId,
        });
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        ulong clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        byte[] connectionData = request.Payload;
        if (connectionData == null || connectionData.Length == 0)
        {
#if UNITY_EDITOR
            Debug.Log($"host {clientId} info: {PlayerManager.Instance.GetLocalPlayerName()}, {AuthenticationService.Instance.PlayerId}");
#endif
            PlayerJoining(
                NetworkManager.Singleton.LocalClientId,
                PlayerManager.Instance.GetLocalPlayerName(),
                AuthenticationService.Instance.PlayerId
            );
        }
        else
        {
            (string playerName, string playerId) = StreamUtils.ReadPlayerNameId(connectionData);
#if UNITY_EDITOR
            Debug.Log($"client {clientId} info: {playerName}, {playerId}");
#endif
            PlayerJoining(clientId, playerName, playerId);
        }

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

    private void AddPlayerConnection(ulong clientId)
    {
        if (clientConnectionMap.TryGetValue(clientId, out ClientConnectionData data))
        {
        }
    }

    private void RemovePlayerConnection(ulong clientId)
    {
        clientConnectionMap.Remove(clientId);
    }

    public Dictionary<ulong, ClientConnectionData> GetClientConnections()
    {
        return new(clientConnectionMap);
    }
}

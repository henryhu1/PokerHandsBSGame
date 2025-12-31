using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class ConnectionManager : NetworkBehaviour
{
    private ServerPlayerManager serverPlayerManager;

    [SerializeField] private float clientConnectionCheckInterval = 5.0f;
    private float clientConnectionCheckTimer;

    private void Awake()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        serverPlayerManager = new ServerPlayerManager();

        clientConnectionCheckTimer = clientConnectionCheckInterval;
    }

    private void Update()
    {
        if (IsClient && !IsServer && !SceneTransitionHandler.Instance.IsInMainMenuScene())
        {
            clientConnectionCheckTimer -= Time.deltaTime;
            if (clientConnectionCheckTimer <= 0)
            {
                if (!NetworkManager.Singleton.IsConnectedClient)
                {
                    SceneTransitionHandler.Instance.ExitAndLoadStartMenu();
                    clientConnectionCheckTimer = clientConnectionCheckInterval;
                }
            }
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        ulong clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        byte[] connectionData = request.Payload;
        if (connectionData == null || connectionData.Length == 0)
        {
            serverPlayerManager.PlayerJoining(
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
            serverPlayerManager.PlayerJoining(clientId, playerName, playerId);
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

    private void ClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        serverPlayerManager.AddPlayer(clientId);
    }

    private void ClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        serverPlayerManager.RemovePlayer(clientId);
    }
}

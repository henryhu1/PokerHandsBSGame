using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PokerHandsBullshitGame : NetworkBehaviour
{
    public static PokerHandsBullshitGame Instance { get; private set; }

    public enum GameType
    {
        Ascending,
        Descending
    }

    private const string k_InGameSceneName = "GameScene";

    // Game settings
    private GameType m_selectedGameType;
    public GameType SelectedGameType
    {
        get { return m_selectedGameType; }
        private set { m_selectedGameType = value; }
    }
    private int m_playerCardAmountChange;
    private int m_numberOfPlayers;
    public int NumberOfPlayers
    {
        get { return m_numberOfPlayers; }
        private set { m_numberOfPlayers = value; }
    }
    private float m_timeForTurn;

    // Player and client data
    public struct PlayerData
    {
        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }
        public string Name { get; set; }
    }
    private string m_localPlayerId;
    private Dictionary<string, PlayerData> m_playerData;
    private Dictionary<ulong, string> m_connectedPlayerIds;
    private List<ulong> m_notInPlayPlayerIds;
    // public NetworkVariable<List<Turn>> turns = new NetworkVariable<List<Turn>>();
    //private bool m_IsPlayerTurn;
    //public bool IsPlayerTurn
    //{
    //    get { return m_IsPlayerTurn; }
    //}

    // private NetworkVariable<List<PokerHand>> m_playedHandLog;
    private List<PlayedHandLogItem> m_playedHandLog;
    public class PlayedHandLogItem
    {
        public readonly PokerHand m_playedHand;
        public readonly ulong m_playerID;
        public readonly string m_name;

        public PlayedHandLogItem(PokerHand playedHand, ulong playerID, string name)
        {
            m_playedHand = playedHand;
            m_playerID = playerID;
            m_name = name;
        }

        public bool IsPokerHandBetter(PokerHand pokerHand)
        {
            return m_playedHand.CompareTo(pokerHand) < 0;
        }
    }

    // public NetworkVariable<bool> m_allPlayersConnected { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> m_hasGameStarted { get; } = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> m_isGameOver { get; } = new NetworkVariable<bool>(false);

    //These help to simplify checking server vs client
    //[NSS]: This would also be a great place to add a state machine and use networked vars for this
    private bool m_ClientGameOver;
    private bool m_ClientGameStarted;
    private bool m_ClientStartCountdown;

    private NetworkVariable<bool> m_CountdownStarted = new NetworkVariable<bool>(false);

    // the timer should only be synced at the beginning
    // and then let the client to update it in a predictive manner
    private bool m_ReplicatedTimeSent = false;
    private float m_TimeRemainingInCurrentTurn;

    [HideInInspector]
    public delegate void UpdatePlayableHandsDelegateHandler(PokerHand playedHand);
    [HideInInspector]
    public event UpdatePlayableHandsDelegateHandler OnUpdatePlayableHands;

    [HideInInspector]
    public delegate void AddToCardLogDelegateHandler(ulong playerId, string playerName, PokerHand playedHand);
    [HideInInspector]
    public event AddToCardLogDelegateHandler OnAddToCardLog;

    [HideInInspector]
    public delegate void ClearCardLogDelegateHandler();
    [HideInInspector]
    public event ClearCardLogDelegateHandler OnClearCardLog;

    [HideInInspector]
    public delegate void InvalidPlayDelegateHandler();
    [HideInInspector]
    public event InvalidPlayDelegateHandler OnInvalidPlay;

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
        PlayerJoining(clientId, playerName, m_localPlayerId);

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

    public void PlayerJoining(ulong clientId, string clientName, string playerId)
    {
        PlayerData playerData;
        if (m_playerData.TryGetValue(playerId, out playerData))
        {

        }
        else
        {
            playerData = new PlayerData();
            playerData.ClientID = clientId;
            playerData.Name = clientName;
            playerData.IsConnected = true;
            // TODO: use playerId
            if (Debug.isDebugBuild)
            {
                m_playerData.Add(clientName, playerData);
            }
            else
            {
                m_playerData.Add(playerId, playerData);
            }
        }
        if (Debug.isDebugBuild)
        {
            m_connectedPlayerIds.Add(clientId, clientName);
        }
        else
        {
            m_connectedPlayerIds.Add(clientId, playerId);
        }

        Debug.Log($"Player with ID {clientId}, Unity auth ID {playerId}, name {clientName} joined");
    }

    private void AddPlayer(ulong clientId)
    {
        if (IsServer)
        {
            // m_connectedPlayerIds.Add(clientId);
            Debug.Log($"connected players: {m_connectedPlayerIds.Count}");

            if (NetworkManager.Singleton.ConnectedClients.Count == m_numberOfPlayers)
            {
                SceneTransitionHandler.Instance.RegisterCallbacks();

                SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.InGame);

                SceneTransitionHandler.Instance.SwitchScene(k_InGameSceneName);
            }
            // m_allPlayersConnected.Value = m_connectedPlayerIds.Count == m_numberOfPlayers;
        }
    }

    private void RemovePlayer(ulong clientId)
    {
        if (IsServer)
        {
            PlayerData playerData = m_playerData[m_connectedPlayerIds[clientId]];
            playerData.IsConnected = false;
            m_connectedPlayerIds.Remove(clientId);

            // m_playerIDToNames.Remove(clientId);
            // m_allPlayersConnected.Value = false;
            // TODO: in-game handling when a player disconnects
        }
    }

    //public void SceneTransitionHandler_OnClientLoadedScene(ulong clientId)
    //{
    //    if (IsServer)
    //    {
    //        if (SceneTransitionHandler.Instance.IsInGameScene())
    //        {
    //            CardManager.Instance.StartingCardAmount = m_selectedGameType == GameType.Ascending ? 1 : 5;
    //            // m_currentTurnPlayer.Value = m_connectedPlayerIds[m_currentTurnPlayerIndex];
    //        }
    //    }
    //}

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= AddPlayer;
            NetworkManager.OnClientDisconnectCallback -= RemovePlayer;
            // SceneTransitionHandler.Instance.OnClientLoadedScene -= SceneTransitionHandler_OnClientLoadedScene;
        }
    }

    //internal static event Action OnSingletonReady;

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            m_ClientGameOver = false;
            m_ClientStartCountdown = false;
            m_ClientGameStarted = false;

            m_CountdownStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientStartCountdown = newValue;
                Debug.LogFormat("Client side we were notified the start count down state was {0}", newValue);
            };

            m_hasGameStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameStarted = newValue;
                //gameTimerText.gameObject.SetActive(!m_ClientGameStarted);
                Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
            };

            m_isGameOver.OnValueChanged += (oldValue, newValue) =>
            {
                m_ClientGameOver = newValue;
                Debug.LogFormat("Client side we were notified the game over state was {0}", newValue);
            };
        }

        if (IsServer)
        {
            m_hasGameStarted.Value = false;
            m_TimeRemainingInCurrentTurn = 0;
            m_ReplicatedTimeSent = false;

            // SceneTransitionHandler.Instance.OnClientLoadedScene += SceneTransitionHandler_OnClientLoadedScene;
            NetworkManager.OnClientConnectedCallback += AddPlayer;
            NetworkManager.OnClientDisconnectCallback += RemovePlayer;
        }

        base.OnNetworkSpawn();
    }

    public void RegisterTurnManagerCallbacks()
    {
        // TurnManager.Instance.
    }

    // TODO: when player gets out, move player to m_notInPlayPlayerIds and do other logic
    public void RegisterCardManagerCallbacks()
    {
        // CardManager.Instance.OnPlayerLoses += 
    }

    public bool IsHosting()
    {
        return IsHost;
    }

    public bool IsBeginningOfRound()
    {
        return m_playedHandLog.Count == 0;
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransitionHandler.Instance.ExitAndLoadStartMenu();
    }

    public void InitializeSettings(GameType gameType, int numberOfPlayers, float timeForTurn = 10f)
    {
        SelectedGameType = gameType;
        m_playerCardAmountChange = gameType == GameType.Ascending ? 1 : -1;
        NumberOfPlayers = numberOfPlayers;
        m_timeForTurn = timeForTurn;
    }

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        m_playerData = new Dictionary<string, PlayerData>();
        m_connectedPlayerIds = new Dictionary<ulong, string>();
        m_notInPlayPlayerIds = new List<ulong>();
        m_playedHandLog = new List<PlayedHandLogItem>();
    }

    public void LobbyManager_Authenticated(string authPlayerId)
    {
        m_localPlayerId = authPlayerId;
    }

    private void Start()
    {
        LobbyManager.Instance.OnAuthenticated += LobbyManager_Authenticated;
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    /*
    private bool ShouldStartCountDown()
    {
        //If the game has started, then don't bother with the rest of the count down checks.
        if (HasGameStarted()) return false;
        if (IsServer)
        {
            m_CountdownStarted.Value = SceneTransitionHandler.sceneTransitionHandler.AllClientsAreLoaded();

            //While we are counting down, continually set the replicated time remaining value for clients (client should only receive the update once)
            if (m_CountdownStarted.Value && !m_ReplicatedTimeSent)
            {
                SetReplicatedTimeRemainingClientRPC(m_DelayedStartTime);
                m_ReplicatedTimeSent = true;
            }

            return m_CountdownStarted.Value;
        }

        return m_ClientStartCountdown;
    } */

    [ServerRpc(RequireOwnership = false)]
    public void TryPlayingHandServerRpc(PokerHand playedHand, ServerRpcParams serverRpcParams = default)
    {
        PokerHand hand = PokerHandFactory.InferPokerHandType(playedHand);
        Debug.Log($"Server received play #{m_playedHandLog.Count}: {hand.GetStringRepresentation()}");

        ulong playingPlayer = serverRpcParams.Receive.SenderClientId;
        if (m_playedHandLog.Count == 0 || m_playedHandLog.Last().IsPokerHandBetter(hand))
        {
            PlayedHandClientRpc(playingPlayer, m_playerData[m_connectedPlayerIds[playingPlayer]].Name, hand);
            TurnManager.Instance.AdvanceTurn();
        }
        else
        {
            PlayedInvalidHandClientRpc(playingPlayer);
        }
    }

    [ClientRpc]
    public void PlayedInvalidHandClientRpc(ulong playerId)
    {
        if (NetworkManager.LocalClientId == playerId)
        {
            OnInvalidPlay?.Invoke();
        }
    }

    [ClientRpc]
    public void PlayedHandClientRpc(ulong playerId, string playerName, PokerHand playedHand)
    {
        PokerHand hand = PokerHandFactory.InferPokerHandType(playedHand);
        Debug.Log($"Client updated with play #{m_playedHandLog.Count}: {hand.GetStringRepresentation()}");

        m_playedHandLog.Add(new PlayedHandLogItem(hand, playerId, playerName));
        OnAddToCardLog?.Invoke(playerId, playerName, hand);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EvaluateLastPlayedHandServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong playerCallingBullshit = serverRpcParams.Receive.SenderClientId;

        PlayedHandLogItem lastLoggedHand = m_playedHandLog.Last();
        ulong lastPlayerToPlayHand = lastLoggedHand.m_playerID;
        PokerHand lastPlayedHand = lastLoggedHand.m_playedHand;

        bool isHandInPlay = CardManager.Instance.IsHandInPlay(lastPlayedHand);
        Debug.Log($"{lastPlayedHand.GetStringRepresentation()} is in play => {isHandInPlay}");

        // TODO: visual display of bullshit result
        ulong losingPlayer = isHandInPlay ? playerCallingBullshit : lastPlayerToPlayHand;
        EndOfRoundClientRpc();
        TurnManager.Instance.EndOfRound(losingPlayer);
        CardManager.Instance.EndOfRound(losingPlayer, m_playerCardAmountChange);
    }

    [ClientRpc]
    public void EndOfRoundClientRpc()
    {
        m_playedHandLog.Clear();
        OnClearCardLog?.Invoke();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
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

    // Game settings
    private GameType m_selectedGameType;
    public GameType SelectedGameType
    {
        get { return m_selectedGameType; }
        private set { m_selectedGameType = value; }
    }
    private int m_playerCardAmountChange;
    private int m_numberOfPlayers;
    public int NumberOfPlayers { get { return m_numberOfPlayers; } }
    private float m_timeForTurn;

    // Player and client data, TODO: player manager? ^ also look at m_numberOfPlayers ^
    public struct PlayerData : INetworkSerializable

    {
        public bool IsConnected { get; set; }
        private ulong m_lastUsedClientId;
        public ulong LastUsedClientID { get { return m_lastUsedClientId; } set { m_lastUsedClientId = value; } }
        private string m_name;
        public string Name { get { return m_name; } set { m_name = value; } }
        public bool InPlay { get; set; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_name);
            serializer.SerializeValue(ref m_lastUsedClientId);
        }
    }
    private string m_localPlayerId;
    public string LocalPlayerId
    {
        get { return m_localPlayerId; }
        private set { m_localPlayerId = value; }
    }
    private Dictionary<string, PlayerData> m_playerData;
    private Dictionary<ulong, string> m_connectedClientIds;
    //private HashSet<string> m_inPlayPlayerIds;
    //private HashSet<string> m_notInPlayPlayerIds;
    public NetworkList<ulong> m_inPlayClientIds { get; private set; }
    public List<ulong> m_eliminatedClientIds { get; private set; }
    public HashSet<ulong> m_notInPlayClientIds { get; private set; }
    private ulong m_lastRoundLosingClientId;

    // public NetworkVariable<List<Turn>> turns = new NetworkVariable<List<Turn>>();
    //private bool m_IsPlayerTurn;
    //public bool IsPlayerTurn
    //{
    //    get { return m_IsPlayerTurn; }
    //}

    // private NetworkVariable<List<PokerHand>> m_playedHandLog;
    // TODO: add a RoundManager? Played hands, next round ready, round events
    private List<PlayedHandLogItem> m_playedHandLog;
    public NetworkVariable<int> m_playersReadyForNextRound { get; } = new NetworkVariable<int>(0);
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
    public delegate void AddToCardLogDelegateHandler(PlayedHandLogItem playedHandLogItem);
    [HideInInspector]
    public event AddToCardLogDelegateHandler OnAddToCardLog;

    [HideInInspector]
    public delegate void NextRoundStartingDelegateHandler();
    [HideInInspector]
    public event NextRoundStartingDelegateHandler OnNextRoundStarting;

    [HideInInspector]
    public delegate void EndOfRoundDelegateHandler(List<bool> playedHandsPresent, List<PokerHand> allHandsInPlay);
    [HideInInspector]
    public event EndOfRoundDelegateHandler OnEndOfRound;

    [HideInInspector]
    public delegate void ClearCardLogDelegateHandler();
    [HideInInspector]
    public event ClearCardLogDelegateHandler OnClearCardLog;

    [HideInInspector]
    public delegate void InvalidPlayDelegateHandler();
    [HideInInspector]
    public event InvalidPlayDelegateHandler OnInvalidPlay;

    [HideInInspector]
    public delegate void GameWonDelegateHandler(int myPosition, List<PlayerData> eliminationOrder);
    [HideInInspector]
    public event GameWonDelegateHandler OnGameWon;

    [HideInInspector]
    public delegate void RestartGameDelegateHandler();
    [HideInInspector]
    public event RestartGameDelegateHandler OnRestartGame;

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        ulong clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        byte[] connectionData = request.Payload;
        if (connectionData == null || connectionData.Length == 0)
        {
            PlayerJoining(NetworkManager.Singleton.LocalClientId, LobbyManager.Instance.PlayerName, LocalPlayerId);
        }
        else
        {
            (string playerName, string playerId) = StreamUtils.ReadPlayerNameId(connectionData);
            Debug.Log($"client {clientId} info: {playerName}, {playerId}");
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

    public void PlayerJoining(ulong clientId, string clientName, string playerId)
    {
        PlayerData playerData;
        if (m_playerData.TryGetValue(playerId, out playerData))
        {
            playerData.LastUsedClientID = clientId;
        }
        else
        {
            playerData = new PlayerData();
            playerData.LastUsedClientID = clientId;
            playerData.Name = clientName;
            playerData.IsConnected = true;

            m_playerData.Add(playerId, playerData);
        }
        m_connectedClientIds.Add(clientId, playerId);

        Debug.Log($"Player with ID {clientId}, Unity auth ID {playerId}, name {clientName} joined");
    }

    private void AddPlayer(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log($"connected players: {m_connectedClientIds.Count}");
            if (SceneTransitionHandler.Instance.IsInGameScene())
            {
                m_notInPlayClientIds.Add(clientId);
                PlayerData playerData = m_playerData[m_connectedClientIds[clientId]];
                playerData.InPlay = false;
                m_playerData[m_connectedClientIds[clientId]] = playerData;
            }
            else
            {
                m_inPlayClientIds.Add(clientId);
                PlayerData playerData = m_playerData[m_connectedClientIds[clientId]];
                playerData.InPlay = true;
                m_playerData[m_connectedClientIds[clientId]] = playerData;
            }

            if (NetworkManager.Singleton.ConnectedClients.Count == m_numberOfPlayers)
            {
                SceneTransitionHandler.Instance.RegisterCallbacks();

                SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.InGame);

                SceneTransitionHandler.Instance.SwitchToGameScene();
            }
        }
    }

    private void RemovePlayer(ulong clientId)
    {
        if (IsServer)
        {
            string playerId = m_connectedClientIds[clientId];
            PlayerData playerData = m_playerData[playerId];
            playerData.IsConnected = false;
            playerData.InPlay = false;
            m_connectedClientIds.Remove(clientId);
            // m_inPlayPlayerIds.Remove(playerId);
            m_inPlayClientIds.Remove(clientId);
            m_eliminatedClientIds.Remove(clientId);
            // m_numberOfPlayersPlaying.Value = m_inPlayPlayerIds.Count;
            // m_notInPlayPlayerIds.Add(playerId);
            m_notInPlayClientIds.Remove(clientId);
            m_numberOfPlayers -= 1;

            // m_playerIDToNames.Remove(clientId);
            // m_allPlayersConnected.Value = false;
            PlayerLeftClientRpc(playerData.Name);
        }
    }

    //public PlayerData GetPlayerData(ulong clientId)
    //{
    //    if (IsServer)
    //    {
    //        if (m_playerData.TryGetValue(m_connectedClientIds[clientId], out PlayerData playerData))
    //        {
    //            return playerData;
    //        }
    //        else
    //        {
    //            throw new Exception($"Player with client ID {clientId} does not exist");
    //        }
    //    }
    //    else
    //    {
    //        throw new Exception("Only server can get player data");
    //    }
    //}

    //public void SceneTransitionHandler_OnClientLoadedScene(ulong clientId)
    //{
    //    if (IsServer)
    //    {
    //        if (SceneTransitionHandler.Instance.IsInGameScene())
    //        {
    //        // CardManager.Instance.StartingCardAmount = m_selectedGameType == GameType.Ascending ? 1 : 5;
    //        // m_currentTurnPlayer.Value = m_connectedPlayerIds[m_currentTurnPlayerIndex];
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

    public void RegisterCardManagerCallbacks()
    {
        CardManager.Instance.OnPlayerOut += CardManager_PlayerOut;
    }

    public void UnregisterCardManagerCallbacks()
    {
        CardManager.Instance.OnPlayerOut -= CardManager_PlayerOut;
    }

    private void CardManager_PlayerOut(ulong losingClientId)
    {
        if (IsServer)
        {
            //string losingPlayerId = m_connectedClientIds[losingClientId];
            //m_inPlayPlayerIds.Remove(losingPlayerId);
            //m_notInPlayPlayerIds.Add(losingPlayerId);
            m_inPlayClientIds.Remove(losingClientId);
            m_eliminatedClientIds.Add(losingClientId);
        }
    }

    public void RegisterNextRoundUIObservers()
    {
        m_playersReadyForNextRound.OnValueChanged += (oldValue, newValue) =>
        {
            NextRoundUI.Instance.SetNumberOfPlayersReadyText(newValue);
        };

        m_inPlayClientIds.OnListChanged += (NetworkListEvent<ulong> changeEvent) =>
        {
            NextRoundUI.Instance.SetTotalNumberOfPlayersToBeReady(m_inPlayClientIds.Count);
            if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Remove)
            {
                NextRoundUI.Instance.SetToggleEnabled(changeEvent.Value != NetworkManager.Singleton.LocalClientId);
            }
        };
    }

    public bool IsNotOut(ulong clientId)
    {
        return m_inPlayClientIds.Contains(clientId);
    }

    public bool IsNotOut()
    {
        return m_inPlayClientIds.Contains(NetworkManager.Singleton.LocalClientId);
    }

    public string GetClientName(ulong clientId)
    {
        if (IsServer)
        {
            return m_playerData[m_connectedClientIds[clientId]].Name;
        }
        return null;
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
        m_numberOfPlayers = numberOfPlayers;
        m_timeForTurn = timeForTurn;
    }

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        m_playerData = new Dictionary<string, PlayerData>();
        m_connectedClientIds = new Dictionary<ulong, string>();
        //m_inPlayPlayerIds = new HashSet<string>();
        //m_notInPlayPlayerIds = new HashSet<string>();
        m_inPlayClientIds = new NetworkList<ulong>();
        m_eliminatedClientIds = new List<ulong>();
        m_notInPlayClientIds = new HashSet<ulong>();
        m_playedHandLog = new List<PlayedHandLogItem>();
    }

    public void LobbyManager_Authenticated(string authPlayerId)
    {
        LocalPlayerId = authPlayerId;
    }

    public void EndOfGameUI_RestartGame(GameType gameType)
    {
        if (IsServer)
        {
            m_selectedGameType = gameType;
            List<ulong> backInPlayClientIds = new List<ulong>{ m_inPlayClientIds[0] };
            backInPlayClientIds.AddRange(m_eliminatedClientIds.Concat(m_notInPlayClientIds.ToList()).ToList());
            backInPlayClientIds.ForEach(i =>
            {
                PlayerData playerData = m_playerData[m_connectedClientIds[i]];
                playerData.InPlay = true;
                m_playerData[m_connectedClientIds[i]] = playerData;
            });
            m_inPlayClientIds.Dispose();
            m_inPlayClientIds = new NetworkList<ulong>(backInPlayClientIds);
            m_eliminatedClientIds.Clear();
            m_notInPlayClientIds.Clear();
            CardManager.Instance.NewGamePlayerCards(backInPlayClientIds);
            TurnManager.Instance.NewGamePlayerTurns(backInPlayClientIds);
            RestartGameClientRpc();
        }
    }

    private void Start()
    {
        EndOfGameUI.Instance.OnRestartGame += EndOfGameUI_RestartGame;
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

        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        if (m_playedHandLog.Count == 0 || m_playedHandLog.Last().IsPokerHandBetter(hand))
        {
            string playerId = m_connectedClientIds[senderClientId];
            PlayedHandClientRpc(senderClientId, playerId, m_playerData[playerId].Name, hand);
            TurnManager.Instance.AdvanceTurn();
        }
        else
        {
            PlayedInvalidHandClientRpc(senderClientId);
        }
    }

    [ClientRpc]
    public void PlayedInvalidHandClientRpc(ulong playerId)
    {
        if (NetworkManager.Singleton.LocalClientId == playerId)
        {
            OnInvalidPlay?.Invoke();
        }
    }

    [ClientRpc]
    public void PlayedHandClientRpc(ulong playedHandClientId, string playedHandPlayerId, string playerName, PokerHand playedHand)
    {
        PokerHand hand = PokerHandFactory.InferPokerHandType(playedHand);
        Debug.Log($"Client updated with play #{m_playedHandLog.Count}: {hand.GetStringRepresentation()}");

        PlayedHandLogItem playedHandLogItem = new PlayedHandLogItem(hand, playedHandClientId, playedHandPlayerId, playerName);
        m_playedHandLog.Add(playedHandLogItem);
        OnAddToCardLog?.Invoke(playedHandLogItem);
    }

    [ClientRpc]
    public void PlayerLeftClientRpc(string name)
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void EvaluateLastPlayedHandServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong callingBullshitClientId = serverRpcParams.Receive.SenderClientId;

        PlayedHandLogItem lastLoggedHand = m_playedHandLog.Last();
        string lastPlayerToPlayHand = lastLoggedHand.m_playerId;
        PokerHand lastPlayedHand = lastLoggedHand.m_playedHand;

        bool isHandInPlay = CardManager.Instance.IsHandInPlay(lastPlayedHand);
        Debug.Log($"{lastPlayedHand.GetStringRepresentation()} is in play => {isHandInPlay}");

        // TODO: visual display of bullshit result
        m_lastRoundLosingClientId = isHandInPlay ? callingBullshitClientId : m_playerData[lastPlayerToPlayHand].LastUsedClientID;
        IEnumerable<bool> playedHandsPresent = m_playedHandLog.Select(i => i.m_existsInRound);
        EndOfRoundClientRpc(playedHandsPresent.ToArray(), CardManager.Instance.GetAllHandsInPlay().ToArray());
        // CardManager.Instance.EndOfRound(m_lastRoundLosingClientId, m_playerCardAmountChange);
        TurnManager.Instance.EndOfRound(m_lastRoundLosingClientId);
    }

    [ClientRpc]
    public void EndOfRoundClientRpc(bool[] playedHandsPresent, PokerHand[] allHandsInPlay)
    {
        List<PokerHand> pokerHandsInPlay = allHandsInPlay.Select(i => PokerHandFactory.InferPokerHandType(i)).ToList();
        OnEndOfRound?.Invoke(playedHandsPresent.ToList(), pokerHandsInPlay);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyForNextRoundServerRpc(bool isPlayerReady)
    {
        int change = isPlayerReady ? 1 : -1;
        m_playersReadyForNextRound.Value += change;
        if (m_playersReadyForNextRound.Value == m_inPlayClientIds.Count)
        {
            if (m_inPlayClientIds.Count == 1) // if (m_inPlayPlayerIds.Count == 1) // m_notInPlayPlayerIds.Count + 1 == m_numberOfPlayersNetworkVariable.Value)
            {
                // string winner = m_inPlayPlayerIds.First();
                //string winner = m_connectedClientIds[m_inPlayClientIds[0]];
                //PlayerData winnerPlayerData = m_playerData[winner];
                List<PlayerData> elimatedPlayersOrder = m_eliminatedClientIds.Select(i => m_playerData[m_connectedClientIds[i]]).ToList();
                GameWinnerClientRpc(elimatedPlayersOrder.ToArray());
            }
            else
            {
                m_playersReadyForNextRound.Value = 0;
                CardManager.Instance.NextRound(m_lastRoundLosingClientId, m_playerCardAmountChange);
                NextRoundClientRpc();
                //ClientRpcParams playingClientRpcParams = new ClientRpcParams
                //{
                //    Send = new ClientRpcSendParams
                //    {
                //        TargetClientIds = m_inPlayPlayerIds.Select(i => m_playerData[i].LastUsedClientID).ToArray()
                //    }
                //};
                //TurnManager.Instance.PlayingNextRoundClientRpc(playingClientRpcParams);
                //ClientRpcParams notPlayingClientRpcParams = new ClientRpcParams
                //{
                //    Send = new ClientRpcSendParams
                //    {
                //        TargetClientIds = m_notInPlayPlayerIds.Select(i => m_playerData[i].LastUsedClientID).ToArray()
                //    }
                //};
                //TurnManager.Instance.NotPlayingNextRoundClientRpc(notPlayingClientRpcParams);
                TurnManager.Instance.NextRoundClientRpc();
            }
        }
    }

    [ClientRpc]
    public void NextRoundClientRpc()
    {
        OnNextRoundStarting?.Invoke();
        // TODO: should played hand log be a network list?
        m_playedHandLog.Clear();
        OnClearCardLog?.Invoke();
    }

    [ClientRpc]
    public void GameWinnerClientRpc(PlayerData[] eliminationOrder)
    {
        int myPosition = 0;
        for (int i = 0; i < eliminationOrder.Length; i++)
        {
            if (eliminationOrder[i].LastUsedClientID == NetworkManager.Singleton.LocalClientId)
            {
                myPosition = i + 1;
                break;
            }
        }
        OnGameWon?.Invoke(myPosition, eliminationOrder.ToList());
    }

    [ClientRpc]
    public void RestartGameClientRpc()
    {
        PlayUI.Instance.Hide();
        OnRestartGame?.Invoke();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

// TODO: Refactor to separate game logic with initial player networking
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private const float k_clientConnectionCheckInterval = 5.0f;
    private float m_clientConnectionCheckTimer;

    // Game settings
    private GameType selectedGameType;
    private int m_numberOfPlayers;
    public int NumberOfPlayers { get { return m_numberOfPlayers; } }
    private TimeForTurnType timeForTurn;

    private string m_localPlayerId;
    public string LocalPlayerId
    {
        get { return m_localPlayerId; }
        private set { m_localPlayerId = value; }
    }
    private string m_localPlayerName;
    public string LocalPlayerName
    {
        get { return m_localPlayerName; }
        private set { m_localPlayerName = value; }
    }
    private Dictionary<string, PlayerData> m_playerData;
    public Dictionary<ulong, string> m_connectedClientIds { get; private set; }
    //private HashSet<string> m_inPlayPlayerIds;
    //private HashSet<string> m_notInPlayPlayerIds;
    public NetworkList<ulong> m_inPlayClientIds { get; private set; }
    public List<PlayerData> m_eliminatedClientIds { get; private set; }
    public HashSet<ulong> m_notInPlayClientIds { get; private set; }
    private ulong m_lastRoundLosingClientId;

    // TODO: every player has their own play log... only the server should have the log
    //   and clients just populate their log UI with the new play
    //   or: make this a NetworkList since the played hands should be in sync across all players
    private List<PlayedHandLogItem> m_playedHandLog;
    public NetworkVariable<int> m_playersReadyForNextRound { get; } = new NetworkVariable<int>(0);

    ///////////////////////////////////////////////////////
    //public NetworkVariable<bool> m_hasGameStarted { get; } = new NetworkVariable<bool>(false);
    //public NetworkVariable<bool> m_isGameOver { get; } = new NetworkVariable<bool>(false);

    //These help to simplify checking server vs client
    //[NSS]: This would also be a great place to add a state machine and use networked vars for this
    //private bool m_ClientGameOver;
    //private bool m_ClientGameStarted;
    //private bool m_ClientStartCountdown;

    //private NetworkVariable<bool> m_CountdownStarted = new NetworkVariable<bool>(false);

    // the timer should only be synced at the beginning
    // and then let the client to update it in a predictive manner
    //private bool m_ReplicatedTimeSent = false;
    //private float m_TimeRemainingInCurrentTurn;
    ///////////////////////////////////////////////////////

    [HideInInspector]
    public delegate void AddToCardLogDelegateHandler(PlayedHandLogItem playedHandLogItem);
    [HideInInspector]
    public event AddToCardLogDelegateHandler OnAddToCardLog;

    [HideInInspector]
    public delegate void ClearCardLogDelegateHandler();
    [HideInInspector]
    public event ClearCardLogDelegateHandler OnClearCardLog;

    [Header("Listening Events")]
    [SerializeField] private UlongEventChannelSO OnPlayerOut;
    [SerializeField] private IntEventChannelSO OnRestartGame;

    [Header("Firing Events")]
    [SerializeField] private IntEventChannelSO OnInvalidPlay;
    [SerializeField] private PokerHandEventChannelSO OnUpdatePlayableHands;
    [SerializeField] private StringEventChannelSO OnPlayerLeft;
    [SerializeField] private VoidEventChannelSO OnInitializeNewGame;
    [SerializeField] private PokerHandListEventChannelSO OnDisplayAllHandsInPlay;
    [SerializeField] private BoolListEventChannelSO OnDisplayPlayedHandsPresent;
    [SerializeField] private VoidEventChannelSO OnGameWon;

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        ulong clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        byte[] connectionData = request.Payload;
        if (connectionData == null || connectionData.Length == 0)
        {
            PlayerJoining(NetworkManager.Singleton.LocalClientId, LocalPlayerName, LocalPlayerId);
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

    public void PlayerJoining(ulong clientId, string clientName, string playerId)
    {
        PlayerData playerData;
        if (m_playerData.TryGetValue(playerId, out playerData))
        {
            playerData.LastUsedClientID = clientId;
#if UNITY_EDITOR
            Debug.Log($"Player with ID {clientId}, Unity auth ID {playerId}, name {clientName} was previously in the game");
#endif
        }
        else
        {
            playerData = new PlayerData();
            playerData.LastUsedClientID = clientId;
            playerData.Name = clientName;
            playerData.IsConnected = true;

            m_playerData.Add(playerId, playerData);
#if UNITY_EDITOR
            Debug.Log($"new player with client ID {clientId}, Unity auth ID {playerId}, name {clientName} joined");
#endif
        }

        if (SceneTransitionHandler.Instance.IsInGameScene())
        {
#if UNITY_EDITOR
            Debug.Log($"client #{clientId} joined in the middle of a game");
#endif
            m_notInPlayClientIds.Add(clientId);
            playerData.InPlay = false;
            m_playerData[playerId] = playerData;
        }

        m_connectedClientIds.Add(clientId, playerId);
    }

    private void AddPlayer(ulong clientId)
    {
        if (IsServer)
        {
            if (!m_notInPlayClientIds.Contains(clientId))
            {
                m_inPlayClientIds.Add(clientId);
                PlayerData playerData = m_playerData[m_connectedClientIds[clientId]];
                playerData.InPlay = true;
                m_playerData[m_connectedClientIds[clientId]] = playerData;
            }

            if (NetworkManager.Singleton.ConnectedClients.Count == m_numberOfPlayers)
            {
                if (SceneTransitionHandler.Instance.IsInMainMenuScene())
                {
                    LobbyManager.Instance.DeleteLobby();
                    SceneTransitionHandler.Instance.SetSceneState(SceneStates.InGame);
                    SceneTransitionHandler.Instance.SwitchToGameScene();
                }
            }
        }
    }

    private void RemovePlayer(ulong clientId)
    {
        if (IsServer)
        {
            if (!m_connectedClientIds.ContainsKey(clientId)) return;
            if (!NetworkManager.Singleton.IsConnectedClient) return;

            string playerId = m_connectedClientIds[clientId];
            PlayerData playerData = m_playerData[playerId];
            playerData.IsConnected = false;
            playerData.InPlay = false;
            m_connectedClientIds.Remove(clientId);

            if (SceneTransitionHandler.Instance.IsInGameScene())
            {
                if (m_inPlayClientIds.Contains(clientId))
                {
                    // TODO: fix error occurring here when game ends in GameScene and despawns on the network
                    m_inPlayClientIds.Remove(clientId);
                    m_numberOfPlayers -= 1;
                    if (!RoundManager.Instance.GetIsRoundOver())
                    {
                        m_lastRoundLosingClientId = clientId;
                        PlayerLeftClientRpc(playerData.Name, CardManager.Instance.GetAllHandsInPlay().ToArray());
                        CardManager.Instance.RevealAllCards();
                    }
                }
                else if (m_eliminatedClientIds.Select(i => i.LastUsedClientID).Contains(clientId))
                {
                    m_numberOfPlayers -= 1;
                }
                else if (m_notInPlayClientIds.Contains(clientId))
                {
                    m_notInPlayClientIds.Remove(clientId);
                }
            }
        }
    }

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        m_playerData = new Dictionary<string, PlayerData>();
        m_connectedClientIds = new Dictionary<ulong, string>();
        m_inPlayClientIds = new NetworkList<ulong>();
        m_eliminatedClientIds = new List<PlayerData>();
        m_notInPlayClientIds = new HashSet<ulong>();
        m_playedHandLog = new List<PlayedHandLogItem>();

        m_clientConnectionCheckTimer = k_clientConnectionCheckInterval;

        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    private void Update()
    {
        if (IsClient && !IsServer && !SceneTransitionHandler.Instance.IsInMainMenuScene())
        {
            m_clientConnectionCheckTimer -= Time.deltaTime;
            if (m_clientConnectionCheckTimer <= 0)
            {
                if (!NetworkManager.Singleton.IsConnectedClient)
                {
                    SceneTransitionHandler.Instance.ExitAndLoadStartMenu();
                    m_clientConnectionCheckTimer = k_clientConnectionCheckInterval;
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            //m_ClientGameOver = false;
            //m_ClientStartCountdown = false;
            //m_ClientGameStarted = false;

            //m_CountdownStarted.OnValueChanged += (oldValue, newValue) =>
            //{
            //    m_ClientStartCountdown = newValue;
            //    Debug.LogFormat("Client side we were notified the start count down state was {0}", newValue);
            //};

            //m_hasGameStarted.OnValueChanged += (oldValue, newValue) =>
            //{
            //    m_ClientGameStarted = newValue;
            //    //gameTimerText.gameObject.SetActive(!m_ClientGameStarted);
            //    Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
            //};

            //m_isGameOver.OnValueChanged += (oldValue, newValue) =>
            //{
            //    m_ClientGameOver = newValue;
            //    Debug.LogFormat("Client side we were notified the game over state was {0}", newValue);
            //};
        }

        if (IsServer)
        {
            //m_hasGameStarted.Value = false;
            //m_TimeRemainingInCurrentTurn = 0;
            //m_ReplicatedTimeSent = false;

            m_inPlayClientIds.Clear();
            m_notInPlayClientIds.Clear();
            m_eliminatedClientIds.Clear();

            SceneTransitionHandler.Instance.RegisterCallbacks();
            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            SceneTransitionHandler.Instance.UnregisterCallbacks();
            NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayer;
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayer;
        }
        base.OnNetworkDespawn();
    }

    private void OnEnable()
    {
        OnRestartGame.OnEventRaised += RestartGame;
        OnPlayerOut.OnEventRaised += CardManager_PlayerOut;
    }

    private void OnDisable()
    {
        OnRestartGame.OnEventRaised -= RestartGame;
        OnPlayerOut.OnEventRaised -= CardManager_PlayerOut;
    }

    public GameType GetGameType() { return selectedGameType; }

    public TimeForTurnType GetTimeForPlayer() { return timeForTurn; }

    private void CardManager_PlayerOut(ulong losingClientId)
    {
        if (IsServer)
        {
            PlayerData losingPlayerData = m_playerData[m_connectedClientIds[losingClientId]];
            losingPlayerData.InPlay = false;
            m_playerData[m_connectedClientIds[losingClientId]] = losingPlayerData;
            m_inPlayClientIds.Remove(losingClientId);
#if UNITY_EDITOR
            Debug.Log($"players in play: {m_inPlayClientIds.Count}");
#endif
            m_eliminatedClientIds.Add(losingPlayerData);
        }
    }

    public void RegisterNextRoundUIObservers()
    {
        m_playersReadyForNextRound.OnValueChanged += (oldValue, newValue) =>
        {
            NextRoundUI.Instance.SetNumberOfPlayersReadyText(newValue);
        };

        m_inPlayClientIds.OnListChanged += changeEvent =>
        {
            if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Remove && changeEvent.Value == NetworkManager.Singleton.LocalClientId)
            {
                NextRoundUI.Instance.SetCanBeReady(false);
            }
            else if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Clear)
            {
                NextRoundUI.Instance.SetCanBeReady(false);
            }
            else if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Add && changeEvent.Value == NetworkManager.Singleton.LocalClientId)
            {
                NextRoundUI.Instance.SetCanBeReady(true);
            }
        };
    }

    public void RegisterActionsUIObservers()
    {
        m_inPlayClientIds.OnListChanged += changeEvent =>
        {
            if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Remove && changeEvent.Value == NetworkManager.Singleton.LocalClientId)
            {
                ActionsUI.Instance.SetPlayerOut();
            }
            else if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Clear)
            {
                ActionsUI.Instance.SetPlayerOut();
            }
            else if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Add && changeEvent.Value == NetworkManager.Singleton.LocalClientId)
            {
                ActionsUI.Instance.SetPlayerIn();
            }
        };
    }

    private void PlayUIObserver(NetworkListEvent<ulong> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Remove && changeEvent.Value == NetworkManager.Singleton.LocalClientId)
        {
            PlayUI.Instance.Hide();
        }
        else if (changeEvent.Type == NetworkListEvent<ulong>.EventType.Add && changeEvent.Value == NetworkManager.Singleton.LocalClientId)
        {
            PlayUI.Instance.Show();
        }
    }

    public void RegisterPlayUIObservers()
    {
        m_inPlayClientIds.OnListChanged += PlayUIObserver;
    }

    public void UnregisterPlayUIObservers()
    {
        m_inPlayClientIds.OnListChanged -= PlayUIObserver;
    }

    public void RegisterEndOfGameUICallbacks()
    {
        EndOfGameUI.Instance.OnExitGame += EndOfGameUI_ExitGame;
    }

    public void UnregisterEndOfGameUICallbacks()
    {
        EndOfGameUI.Instance.OnExitGame -= EndOfGameUI_ExitGame;
    }

    public void SetLocalPlayerId(string authPlayerId)
    {
        LocalPlayerId = authPlayerId;
    }

    public void SetLocalPlayerName(string playerDisplayName)
    {
        LocalPlayerName = playerDisplayName;
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

    public PokerHand GetLastPlayedHand()
    {
        if (m_playedHandLog.Count == 0) return null;

        return m_playedHandLog.Last().m_playedHand;
    }

    public bool IsHandLowerThanLastPlayed(PokerHand pokerHand)
    {
        return m_playedHandLog.Count != 0 && !m_playedHandLog.Last().IsPokerHandBetter(pokerHand);
    }

    public void InitializeSettings(GameType gameType, int numberOfPlayers, TimeForTurnType timeForTurn)
    {
#if UNITY_EDITOR
        Debug.Log($"creating game for {numberOfPlayers} players in {gameType} mode with {timeForTurn} time to play");
#endif
        selectedGameType = gameType;
        m_numberOfPlayers = numberOfPlayers;
        this.timeForTurn = timeForTurn;
    }

    private void EndGameCleanup()
    {
        if (IsServer)
        {
            m_playedHandLog.Clear();
            m_connectedClientIds.Clear();
            m_eliminatedClientIds.Clear();
            m_notInPlayClientIds.Clear();
            m_playerData.Clear();
            ExitGameClientRpc();
            NetworkManager.Singleton.Shutdown();
        }
    }

    public void RestartGame(int gameTypeValue)
    {
        if (IsServer)
        {
            GameType gameType = (GameType) gameTypeValue;
            ulong winnerClientId = m_inPlayClientIds[0];
            selectedGameType = gameType;
            CardManager.Instance.SetAmountOfCardsFromGameSetting();

            List<ulong> backInPlayClientIds = new List<ulong>(m_connectedClientIds.Keys.ToList());
            backInPlayClientIds.ForEach(i =>
            {
#if UNITY_EDITOR
                Debug.Log($"client #{i} is playing in the new game");
#endif
                PlayerData playerData = m_playerData[m_connectedClientIds[i]];
                playerData.InPlay = true;
                m_playerData[m_connectedClientIds[i]] = playerData;
            });

            m_inPlayClientIds.Clear();
            backInPlayClientIds.ForEach(i => m_inPlayClientIds.Add(i));

            m_eliminatedClientIds.Clear();
            m_notInPlayClientIds.Clear();
            m_playedHandLog.Clear();

            m_numberOfPlayers = backInPlayClientIds.Count;
#if UNITY_EDITOR
            Debug.Log($"the winner of the last game is {winnerClientId} and gets the first turn");
#endif
            // TODO: use events, one event for List<ulong>, another for ulong winnerClientId
            TurnManager.Instance.NewGamePlayerTurns(backInPlayClientIds, winnerClientId);
            CardManager.Instance.NewGamePlayerCards(backInPlayClientIds);
            RestartGameClientRpc();
        }
    }

    public void EndOfGameUI_ExitGame()
    {
        EndGameCleanup();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryPlayingHandServerRpc(PokerHand playedHand, ServerRpcParams serverRpcParams = default)
    {
        PokerHand hand = PokerHandFactory.InferPokerHandType(playedHand);
#if UNITY_EDITOR
        Debug.Log($"Server received play #{m_playedHandLog.Count}: {hand.GetStringRepresentation()} ({hand.GetHandType()} {hand.GetPrimaryRank()} {hand.GetSecondaryRank()} {hand.GetSuit()})");
#endif

        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        bool isHandTooLow = IsHandLowerThanLastPlayed(hand);
        bool isNotAllowedFlushPlay = hand.GetHandType() == HandType.Flush && !CardManager.Instance.IsFlushAllowedToBePlayed();
        if (isHandTooLow || isNotAllowedFlushPlay)
        {
            InvalidPlays invalidPlay = isHandTooLow ? InvalidPlays.HandTooLow : InvalidPlays.FlushNotAllowed;
            ClientRpcParams clientRpcParams = new()
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderClientId }
                }
            };
            PlayedInvalidHandClientRpc(invalidPlay, clientRpcParams);
        }
        else
        {
            string playerId = m_connectedClientIds[senderClientId];
            PlayedHandClientRpc(senderClientId, playerId, m_playerData[playerId].Name, hand);
            TurnManager.Instance.AdvanceTurn();
        }
    }

    [ClientRpc]
    public void PlayedInvalidHandClientRpc(InvalidPlays invalidPlay, ClientRpcParams clientRpcParams = default)
    {
        OnInvalidPlay.RaiseEvent((int)invalidPlay);
    }

    [ClientRpc]
    public void PlayedHandClientRpc(ulong playedHandClientId, string playedHandPlayerId, string playerName, PokerHand playedHand)
    {
        PokerHand hand = PokerHandFactory.InferPokerHandType(playedHand);
#if UNITY_EDITOR
        Debug.Log($"Client updated with play #{m_playedHandLog.Count}: {hand.GetStringRepresentation()}");
#endif

        PlayedHandLogItem playedHandLogItem = new(hand, playedHandClientId, playedHandPlayerId, playerName);
        m_playedHandLog.Add(playedHandLogItem);
        OnAddToCardLog?.Invoke(playedHandLogItem);
        OnUpdatePlayableHands.RaiseEvent(hand);
    }

    [ClientRpc]
    public void PlayerLeftClientRpc(string playerLeftName, PokerHand[] allHandsInPlay)
    {
        List<PokerHand> pokerHandsInPlay = allHandsInPlay.Select(i => PokerHandFactory.InferPokerHandType(i)).ToList();
        List<bool> playedHandsPresent = m_playedHandLog.Select(logItem => pokerHandsInPlay.Exists(hand => logItem.m_playedHand == hand)).ToList();
        OnDisplayPlayedHandsPresent.RaiseEvent(playedHandsPresent);
        OnDisplayAllHandsInPlay.RaiseEvent(pokerHandsInPlay);
        RoundManager.Instance.EndOfRound();
        OnPlayerLeft.RaiseEvent(playerLeftName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EvaluateLastPlayedHandServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong callingBullshitClientId = serverRpcParams.Receive.SenderClientId;

        PlayedHandLogItem lastLoggedHand = m_playedHandLog.Last();
        string lastPlayerToPlayHand = lastLoggedHand.m_playerId;
        ulong lastPlayerToPlayHandClientId = m_playerData[lastPlayerToPlayHand].LastUsedClientID;
        PokerHand lastPlayedHand = lastLoggedHand.m_playedHand;

        bool isHandInPlay = CardManager.Instance.IsHandInPlay(lastPlayedHand);
#if UNITY_EDITOR
        Debug.Log($"{lastPlayedHand.GetStringRepresentation()} is in play => {isHandInPlay}");
#endif

        m_lastRoundLosingClientId = isHandInPlay ? callingBullshitClientId : lastPlayerToPlayHandClientId;

        ClientRpcParams losingClientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { m_lastRoundLosingClientId }
            }
        };

        if (m_lastRoundLosingClientId != callingBullshitClientId)
        {
            ClientRpcParams bullshitterClientRpcParams = new()
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { callingBullshitClientId }
                }
            };
            RoundResultClientRpc(RoundResultTypes.CorrectBS, bullshitterClientRpcParams);
            RoundResultClientRpc(RoundResultTypes.CalledOut, losingClientRpcParams);
        }
        else
        {
            ClientRpcParams lastPlayedClientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { lastPlayerToPlayHandClientId }
                }
            };
            RoundResultClientRpc(RoundResultTypes.WrongBS, losingClientRpcParams);
            RoundResultClientRpc(RoundResultTypes.Safe, lastPlayedClientRpcParams);
        }

        EndOfRoundClientRpc(CardManager.Instance.GetAllHandsInPlay().ToArray());
        CardManager.Instance.RevealAllCards();
    }

    [ClientRpc]
    public void EndOfRoundClientRpc(PokerHand[] allHandsInPlay)
    {
        List<PokerHand> pokerHandsInPlay = allHandsInPlay.Select(i => PokerHandFactory.InferPokerHandType(i)).ToList();
        List<bool> playedHandsPresent = m_playedHandLog.Select(logItem => pokerHandsInPlay.Exists(hand => logItem.m_playedHand.Equals(hand))).ToList();
        OnDisplayPlayedHandsPresent.RaiseEvent(playedHandsPresent);
        OnDisplayAllHandsInPlay.RaiseEvent(pokerHandsInPlay);
        RoundManager.Instance.EndOfRound();
    }

    [ClientRpc]
    public void RoundResultClientRpc(RoundResultTypes roundResult, ClientRpcParams clientRpcParams = default)
    {
        RoundManager.Instance.EndOfRoundResult(roundResult);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyForNextRoundServerRpc(bool isPlayerReady, ServerRpcParams serverRpcParams = default)
    {
        if (!m_inPlayClientIds.Contains(serverRpcParams.Receive.SenderClientId)) { return; }

        int change = isPlayerReady ? 1 : -1;
        int playersNowReady = Mathf.Max(m_playersReadyForNextRound.Value + change, 0);
        m_playersReadyForNextRound.Value = playersNowReady;
        if (m_playersReadyForNextRound.Value >= m_inPlayClientIds.Count)
        {
            m_playersReadyForNextRound.Value = 0;
            if (m_connectedClientIds.ContainsKey(m_lastRoundLosingClientId))
            {
                CardManager.Instance.ChangeClientCardAmount(m_lastRoundLosingClientId);
            }
            if (m_inPlayClientIds.Count == 1)
            {
                List<PlayerData> playerStandings = m_eliminatedClientIds;
                playerStandings.Add(m_playerData[m_connectedClientIds[m_inPlayClientIds[0]]]);
                GameWinnerClientRpc(playerStandings.ToArray());
            }
            else
            {
                CardManager.Instance.NextRound();
                NextRoundClientRpc();
                TurnManager.Instance.NextRound(m_lastRoundLosingClientId);
            }
        }
    }

    [ClientRpc]
    public void NextRoundClientRpc()
    {
        RoundManager.Instance.StartNextRound();
        // TODO: should played hand log be a network list?
        m_playedHandLog.Clear();
        OnClearCardLog?.Invoke();
    }

    [ClientRpc]
    public void GameWinnerClientRpc(PlayerData[] playerStandings)
    {
        int myPosition = 0;
        for (int i = 0; i < playerStandings.Length; i++)
        {
            if (playerStandings[i].LastUsedClientID == NetworkManager.Singleton.LocalClientId)
            {
                myPosition = i + 1;
                break;
            }
        }
        EndOfGameUI.Instance.DisplayGameResults(myPosition, playerStandings.ToList());
        OnGameWon.RaiseEvent();
    }

    [ClientRpc]
    public void RestartGameClientRpc()
    {
        PlayUI.Instance.Show();
        // TODO: should played hand log be a network list?
        m_playedHandLog.Clear();
        OnClearCardLog?.Invoke();
        OnInitializeNewGame.RaiseEvent();
    }

    [ClientRpc]
    public void ExitGameClientRpc()
    {
        m_playedHandLog.Clear();
        SceneTransitionHandler.Instance.ExitAndLoadStartMenu();
    }
}

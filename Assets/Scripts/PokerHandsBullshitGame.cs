using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PokerHandsBullshitGame : NetworkBehaviour
{
    public static PokerHandsBullshitGame Instance { get; private set; }

    public const int k_AscendingGameModeCardLimit = 5;
    public const int k_DescendingGameModeCardLimit = 1;
    public enum GameType
    {
        Ascending,
        Descending
    }

    private const string k_InGameSceneName = "GameScene";

    // Game settings
    private GameType m_gameType;
    private int m_playerCardAmountChange;
    private int m_numberOfPlayers;
    public int NumberOfPlayers
    {
        get { return m_numberOfPlayers; }
        private set { m_numberOfPlayers = value; }
    }
    private float m_timeForTurn;

    private Guid m_localGuid;
    private List<ulong> m_connectedPlayerIds;
    private List<ulong> m_notInPlayPlayerIds;
    private Dictionary<ulong, string> m_playerIDToNames;
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
        public readonly int m_playerTurnIndex;

        public PlayedHandLogItem(PokerHand playedHand, ulong playerID, string name, int playerTurnIndex)
        {
            m_playedHand = playedHand;
            m_playerID = playerID;
            m_name = name;
            m_playerTurnIndex = playerTurnIndex;
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

    //[HideInInspector]
    //public delegate void NumberOfPlayersInitializedDelegateHandler(int numberOfPlayers);
    //[HideInInspector]
    //public event NumberOfPlayersInitializedDelegateHandler OnNumberOfPlayersInitialized;

    //[HideInInspector]
    //public delegate void TimeForTurnInitializedDelegateHandler(float timeForTurn);
    //[HideInInspector]
    //public event TimeForTurnInitializedDelegateHandler OnTimeForTurnInitialized;

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
    public delegate void NextPlayerTurnDelegateHandler(bool isPlayerTurn, bool wasPlayerTurnPreviously = false);
    [HideInInspector]
    public event NextPlayerTurnDelegateHandler OnNextPlayerTurn;

    [HideInInspector]
    public delegate void InvalidPlayDelegateHandler();
    [HideInInspector]
    public event InvalidPlayDelegateHandler OnInvalidPlay;


    public void PlayerJoining(ulong clientId, string clientName)
    {
        m_playerIDToNames.Add(clientId, clientName);
        Debug.Log($"Player with ID {clientId}, name {clientName} joined");
    }

    private void AddPlayer(ulong clientId)
    {
        if (IsServer)
        {
            m_connectedPlayerIds.Add(clientId);
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
            m_connectedPlayerIds.Remove(clientId);

            m_playerIDToNames.Remove(clientId);
            // m_allPlayersConnected.Value = false;
            // TODO: in-game handling when a player disconnects
        }
    }

    public int GetPlayerTurnIndex(ulong playerId)
    {
        if (IsServer)
        {
            return m_connectedPlayerIds.IndexOf(playerId);
        }
        return -1;
    }

    public string GetPlayerName(ulong playerId)
    {
        if (IsServer)
        {
            return m_playerIDToNames[playerId];
        }
        return null;
    }

    public void SceneTransitionHandler_OnClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            if (SceneTransitionHandler.Instance.IsInGameScene())
            {
                CardManager.Instance.StartingCardAmount = m_gameType == GameType.Ascending ? 1 : 5;
                // m_currentTurnPlayer.Value = m_connectedPlayerIds[m_currentTurnPlayerIndex];
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= AddPlayer;
            NetworkManager.OnClientDisconnectCallback -= RemovePlayer;
            SceneTransitionHandler.Instance.OnClientLoadedScene -= SceneTransitionHandler_OnClientLoadedScene;
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

        m_currentTurnPlayer.OnValueChanged += (oldValue, newValue) =>
        {
            OnNextPlayerTurn?.Invoke(NetworkManager.LocalClientId == newValue, NetworkManager.LocalClientId == oldValue);
        };

        if (IsServer)
        {
            m_currentTurnPlayerIndex = 0;
            m_hasGameStarted.Value = false;
            m_TimeRemainingInCurrentTurn = 0;
            m_ReplicatedTimeSent = false;

            SceneTransitionHandler.Instance.OnClientLoadedScene += SceneTransitionHandler_OnClientLoadedScene;
            NetworkManager.OnClientConnectedCallback += AddPlayer;
            NetworkManager.OnClientDisconnectCallback += RemovePlayer;
        }

        base.OnNetworkSpawn();
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
        m_gameType = gameType;
        m_playerCardAmountChange = gameType == GameType.Ascending ? 1 : -1;
        NumberOfPlayers = numberOfPlayers;
        m_timeForTurn = timeForTurn;
    }

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        m_localGuid = Guid.NewGuid();
        m_connectedPlayerIds = new List<ulong>();
        m_notInPlayPlayerIds = new List<ulong>();
        m_playerIDToNames = new Dictionary<ulong, string>();
        m_playedHandLog = new List<PlayedHandLogItem>();
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
            PlayedHandClientRpc(playingPlayer, m_playerIDToNames[playingPlayer], hand);
            AdvanceTurn();
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

        m_playedHandLog.Add(new PlayedHandLogItem(hand, playerId, playerName, m_currentTurnPlayerIndex));
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
        SetTurn(losingPlayer);
        EndOfRoundClientRpc(losingPlayer);
        CardManager.Instance.EndOfRound(losingPlayer, m_playerCardAmountChange);
    }

    [ClientRpc]
    public void EndOfRoundClientRpc(ulong losingPlayerID)
    {
        m_playedHandLog.Clear();
        OnClearCardLog?.Invoke();
        OnNextPlayerTurn?.Invoke(NetworkManager.LocalClientId == losingPlayerID);
    }
}

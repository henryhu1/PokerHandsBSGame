using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] GameRulesSO baseRules;

    [Header("Local Managers")]
    [SerializeField] private RoundManager roundManager;

    public GameState gameState = GameState.PREGAME;
    private Dictionary<ulong, PlayerData> allPlayerData = new();
    private readonly HashSet<ulong> readyClients = new();
    private List<PlayerData> eliminatedClients = new();

    // TODO: every player has their own play log... only the server should have the log
    //   and clients just populate their log UI with the new play
    //   or: make this a NetworkList since the played hands should be in sync across all players
    private List<PlayedHandLogItem> playedHandLog = new();
    private int playersReadyForNextRound = 0;

    [HideInInspector]
    public delegate void AddToCardLogDelegateHandler(PlayedHandLogItem playedHandLogItem);
    [HideInInspector]
    public event AddToCardLogDelegateHandler OnAddToCardLog;

    [HideInInspector]
    public delegate void ClearCardLogDelegateHandler();
    [HideInInspector]
    public event ClearCardLogDelegateHandler OnClearCardLog;

    [Header("Listening Events")]
    [SerializeField] private StringEventChannelSO OnAllClientsLoadedScene;
    [SerializeField] private UlongEventChannelSO OnPlayerOut;
    [SerializeField] private VoidEventChannelSO OnRestartGame;
    [SerializeField] private UlongEventChannelSO OnServerPlayerTurnTimeout;

    [Header("Firing Events")]
    [SerializeField] private IntEventChannelSO OnInvalidPlay;
    [SerializeField] private PokerHandEventChannelSO OnUpdatePlayableHands;
    [SerializeField] private StringEventChannelSO OnPlayerLeft;
    [SerializeField] private StringEventChannelSO OnPlayerRanOutOfTime;
    [SerializeField] private VoidEventChannelSO OnInitializeNewGame;
    [SerializeField] private PokerHandListEventChannelSO OnDisplayAllHandsInPlay;
    [SerializeField] private BoolListEventChannelSO OnDisplayPlayedHandsPresent;
    [SerializeField] private VoidEventChannelSO OnGameWon;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            OnAllClientsLoadedScene.OnEventRaised += AllClientsLoadedScene;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;
        }

        base.OnNetworkSpawn();

        ReportReadyServerRpc();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        OnAllClientsLoadedScene.OnEventRaised -= AllClientsLoadedScene;

        NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayer;

        base.OnNetworkDespawn();
    }

    private void OnEnable()
    {
        OnRestartGame.OnEventRaised += RestartGame;
        OnPlayerOut.OnEventRaised += CardManager_PlayerOut;
        OnServerPlayerTurnTimeout.OnEventRaised += PlayerTurnTimeout;
    }

    private void OnDisable()
    {
        OnRestartGame.OnEventRaised -= RestartGame;
        OnPlayerOut.OnEventRaised -= CardManager_PlayerOut;
        OnServerPlayerTurnTimeout.OnEventRaised -= PlayerTurnTimeout;
    }

    // TODO: invoke events on game state change
    private void AdvanceGameState()
    {
        switch (gameState)
        {
            case GameState.PREGAME:
                gameState = GameState.ROUNDS;
                break;
            case GameState.ROUNDS:
                gameState = GameState.RESULT;
                break;
            case GameState.RESULT:
                if (GetNumberOfInGamePlayers() == 1)
                    gameState = GameState.DONE;
                else
                    gameState = GameState.ROUNDS;
                break;
            case GameState.DONE:
                gameState = GameState.PREGAME;
                break;
        }
    }

    private void PopulatePlayerDataFromConnections()
    {
        allPlayerData.Clear();

        var clients = ConnectionManager.Instance.GetClientConnections();
        foreach (var kvp in clients)
        {
#if UNITY_EDITOR
            Debug.Log($"{kvp.Key} {kvp.Value.DisplayName} is being initialized in all player data");
#endif
            allPlayerData.Add(
                kvp.Key,
                new PlayerData(
                    clientId: kvp.Key,
                    authId: kvp.Value.AuthId,
                    playerName: kvp.Value.DisplayName,
                    initState: PlayerState.SPECTATING
                )
            );
        }
    }

    private void AllClientsLoadedScene(string sceneName)
    {
        if (sceneName != SceneTransitionHandler.k_InGameSceneName) return;

#if UNITY_EDITOR
        Debug.Log($"all clients loaded scene {sceneName}");
#endif
        PopulatePlayerDataFromConnections();
        TryStartGame();
    }

    private void MarkClientReady(ulong clientId)
    {
        if (!IsServer) return;
        if (gameState != GameState.PREGAME) return;

#if UNITY_EDITOR
        Debug.Log($"{clientId} is ready to play");
#endif
        readyClients.Add(clientId);
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (!IsServer) return;
        if (gameState != GameState.PREGAME) return;

        if (readyClients.Count != allPlayerData.Count) return;
        StartGame();
    }

    private void StartGame()
    {
        if (gameState != GameState.PREGAME) return;

        foreach (var playerData in allPlayerData.Values.ToList())
        {
            playerData.state = PlayerState.PLAYING;
        }
        readyClients.Clear();

        AdvanceGameState();
        TurnManager.Instance.DecideTurnOrder(allPlayerData.Keys.ToArray());
        CardManager.Instance.SetUpPlayerHands(allPlayerData.Keys.ToArray());
        StartNextRoundClientRpc();
    }

    private void RemovePlayer(ulong clientId)
    {
        if (!IsServer) return;

        if (!NetworkManager.Singleton.IsConnectedClient) return;

        if (allPlayerData.TryGetValue(clientId, out PlayerData playerData))
        {
            if (playerData.state == PlayerState.PLAYING || playerData.state == PlayerState.ELIMINATED)
            {
                // TODO: fix error occurring here when game ends in GameScene and despawns on the network
                // inPlayClientIds.Remove(clientId);
                // TODO: DO in ConnectionManager
                if (gameState != GameState.ROUNDS)
                {
                    // TODO: invoke two events instead
                    //   one to signal a player left
                    //   another to end the round
                    AdvanceGameState();
                    PlayerLeftClientRpc(playerData.GetName(), CardManager.Instance.GetAllHandsInPlay().ToArray());
                    CardManager.Instance.RevealAllCards();
                }
            }

            allPlayerData.Remove(clientId);
        }

#if UNITY_EDITOR
            Debug.Log($"removed {clientId} from turns");
#endif
    }

    public int GetNumberOfInGamePlayers()
    {
        return allPlayerData.Values
            .Count(playerData =>
                {
                    return playerData.state == PlayerState.PLAYING;
                }
            );
    }

    private void CardManager_PlayerOut(ulong losingClientId)
    {
        if (IsServer)
        {
            PlayerData losingPlayerData = allPlayerData[losingClientId];
            losingPlayerData.state = PlayerState.ELIMINATED;
#if UNITY_EDITOR
            Debug.Log($"players in play: {GetNumberOfInGamePlayers()}");
#endif
            eliminatedClients.Add(losingPlayerData);
        }
    }

    private void PlayerTurnTimeout(ulong timedOutClientId)
    {
        if (!IsServer) return;

        PlayerData removedPlayerData = allPlayerData[timedOutClientId];
        PlayerRanOutOfTimeClientRpc(removedPlayerData.GetName(), CardManager.Instance.GetAllHandsInPlay().ToArray());
    }

    public bool IsClientInPlay(ulong clientId)
    {
        if (allPlayerData.TryGetValue(clientId, out PlayerData playerData))
        {
            return playerData.state == PlayerState.PLAYING;
        }
        return false;
    }

    public bool IsClientInPlay()
    {
        return IsClientInPlay(NetworkManager.Singleton.LocalClientId);
    }

    public string GetClientName(ulong clientId)
    {
        if (IsServer)
        {
            if (allPlayerData.TryGetValue(clientId, out PlayerData playerData))
            {
                return playerData.GetName();
            }
        }
        return null;
    }

    public bool IsBeginningOfRound()
    {
        return playedHandLog.Count == 0;
    }

    public PokerHand GetLastPlayedHand()
    {
        if (playedHandLog.Count == 0) return null;

        return playedHandLog.Last().m_playedHand;
    }

    public bool IsHandLowerThanLastPlayed(PokerHand pokerHand)
    {
        return playedHandLog.Count != 0 && !playedHandLog.Last().IsPokerHandBetter(pokerHand);
    }

    private void EndGameCleanup()
    {
        if (IsServer)
        {
            playedHandLog.Clear();
            allPlayerData.Clear();
            ExitGameClientRpc();
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void RestartGame()
    {
        if (IsServer)
        {
            PopulatePlayerDataFromConnections();

            var rules = GameSession.Instance.ActiveRules;
            GameRulesSO selectedRules = GameRulesFactory.CreateRuntime(baseRules);
            selectedRules.selectedGameType = rules.selectedGameType;
            // selectedRules.timeForTurn = timeForPlayer;
            GameSession.Instance.SetRules(selectedRules);

            ulong winnerClientId = allPlayerData.First(playerData => playerData.Value.state == PlayerState.PLAYING).Key;
            if (GetNumberOfInGamePlayers() == 1)
            {
                PlayerData[] lonePlayer = { allPlayerData[winnerClientId] };
                GameWinnerClientRpc(lonePlayer);
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"the winner of the last game is {winnerClientId} and gets the first turn");
#endif
                AdvanceGameState();
                TurnManager.Instance.DecideTurnOrder(allPlayerData.Keys.ToArray());
                CardManager.Instance.SetUpPlayerHands(allPlayerData.Keys.ToArray());
                RestartGameClientRpc();
            }
        }
    }

    public void EndOfGameUI_ExitGame()
    {
        EndGameCleanup();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReportReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        if (gameState != GameState.PREGAME) return;

        MarkClientReady(rpcParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryPlayingHandServerRpc(PokerHand playedHand, ServerRpcParams serverRpcParams = default)
    {
        if (gameState != GameState.ROUNDS) return;

        PokerHand hand = PokerHandFactory.InferPokerHandType(playedHand);
#if UNITY_EDITOR
        Debug.Log($"Server received play #{playedHandLog.Count}: {hand.GetStringRepresentation()} ({hand.GetHandType()} {hand.GetPrimaryRank()} {hand.GetSecondaryRank()} {hand.GetSuit()})");
#endif

        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        bool isHandTooLow = IsHandLowerThanLastPlayed(hand);
        bool isNotAllowedFlushPlay = hand.GetHandType() == HandType.Flush && !CardManager.Instance.IsFlushAllowedToBePlayed();
        if (isHandTooLow || isNotAllowedFlushPlay)
        {
            InvalidPlays invalidPlay = isHandTooLow ? InvalidPlays.HandTooLow : InvalidPlays.FlushNotAllowed;
            ClientRpcParams clientRpcParams = new()
            {
                Send = new() { TargetClientIds = new ulong[] { senderClientId } }
            };
            PlayedInvalidHandClientRpc(invalidPlay, clientRpcParams);
        }
        else
        {
            PlayerData playingPlayer = allPlayerData[senderClientId];
            PlayedHandClientRpc(senderClientId, playingPlayer.GetName(), hand);
            TurnManager.Instance.AdvanceTurn();
        }
    }

    [ClientRpc]
    public void PlayedInvalidHandClientRpc(InvalidPlays invalidPlay, ClientRpcParams clientRpcParams = default)
    {
        OnInvalidPlay.RaiseEvent((int)invalidPlay);
    }

    [ClientRpc]
    public void PlayedHandClientRpc(ulong playedHandClientId, string playerName, PokerHand playedHand)
    {
        PokerHand hand = PokerHandFactory.InferPokerHandType(playedHand);
#if UNITY_EDITOR
        Debug.Log($"Client updated with play #{playedHandLog.Count}: {hand.GetStringRepresentation()}");
#endif

        PlayedHandLogItem playedHandLogItem = new(hand, playedHandClientId, playerName);
        playedHandLog.Add(playedHandLogItem);
        OnAddToCardLog?.Invoke(playedHandLogItem);
        OnUpdatePlayableHands.RaiseEvent(hand);
    }

    [ClientRpc]
    public void PlayerRanOutOfTimeClientRpc(string playerRanOutOfTimeName, PokerHand[] allHandsInPlay)
    {
        List<PokerHand> pokerHandsInPlay = allHandsInPlay.Select(i => PokerHandFactory.InferPokerHandType(i)).ToList();
        List<bool> playedHandsPresent = playedHandLog.Select(logItem => pokerHandsInPlay.Exists(hand => logItem.m_playedHand == hand)).ToList();
        OnDisplayPlayedHandsPresent.RaiseEvent(playedHandsPresent);
        OnDisplayAllHandsInPlay.RaiseEvent(pokerHandsInPlay);
        roundManager.EndOfRound();
        OnPlayerRanOutOfTime.RaiseEvent(playerRanOutOfTimeName);
    }

    [ClientRpc]
    public void PlayerLeftClientRpc(string playerLeftName, PokerHand[] allHandsInPlay)
    {
        List<PokerHand> pokerHandsInPlay = allHandsInPlay.Select(i => PokerHandFactory.InferPokerHandType(i)).ToList();
        List<bool> playedHandsPresent = playedHandLog.Select(logItem => pokerHandsInPlay.Exists(hand => logItem.m_playedHand == hand)).ToList();
        OnDisplayPlayedHandsPresent.RaiseEvent(playedHandsPresent);
        OnDisplayAllHandsInPlay.RaiseEvent(pokerHandsInPlay);
        roundManager.EndOfRound();
        OnPlayerLeft.RaiseEvent(playerLeftName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EvaluateLastPlayedHandServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (gameState != GameState.ROUNDS) return;

        ulong callingBullshitClientId = serverRpcParams.Receive.SenderClientId;

        PlayedHandLogItem lastLoggedHand = playedHandLog.Last();
        ulong lastClientToPlayHand = lastLoggedHand.m_clientId;
        PokerHand lastPlayedHand = lastLoggedHand.m_playedHand;

        bool isHandInPlay = CardManager.Instance.IsHandInPlay(lastPlayedHand);
#if UNITY_EDITOR
        Debug.Log($"{lastPlayedHand.GetStringRepresentation()} is in play => {isHandInPlay}");
#endif

        ulong lastRoundLosingClientId = isHandInPlay ? callingBullshitClientId : lastClientToPlayHand;

        ClientRpcParams losingClientRpcParams = new()
        {
            Send = new() { TargetClientIds = new ulong[] { lastRoundLosingClientId } }
        };

        if (lastRoundLosingClientId != callingBullshitClientId)
        {
            ClientRpcParams bullshitterClientRpcParams = new()
            {
                Send = new() { TargetClientIds = new ulong[] { callingBullshitClientId } }
            };
            RoundResultClientRpc(RoundResultTypes.CorrectBS, bullshitterClientRpcParams);
            RoundResultClientRpc(RoundResultTypes.CalledOut, losingClientRpcParams);
        }
        else
        {
            ClientRpcParams lastPlayedClientRpcParams = new()
            {
                Send = new() { TargetClientIds = new ulong[] { lastClientToPlayHand } }
            };
            RoundResultClientRpc(RoundResultTypes.WrongBS, losingClientRpcParams);
            RoundResultClientRpc(RoundResultTypes.Safe, lastPlayedClientRpcParams);
        }

        if (allPlayerData.ContainsKey(lastRoundLosingClientId))
        {
            CardManager.Instance.ChangeClientCardAmount(lastRoundLosingClientId);
        }

        AdvanceGameState();
        EndOfRoundClientRpc(CardManager.Instance.GetAllHandsInPlay().ToArray());
        CardManager.Instance.RevealAllCards();
    }

    [ClientRpc]
    public void EndOfRoundClientRpc(PokerHand[] allHandsInPlay)
    {
        List<PokerHand> pokerHandsInPlay = allHandsInPlay.Select(i => PokerHandFactory.InferPokerHandType(i)).ToList();
        List<bool> playedHandsPresent = playedHandLog.Select(logItem => pokerHandsInPlay.Exists(hand => logItem.m_playedHand.Equals(hand))).ToList();
        OnDisplayPlayedHandsPresent.RaiseEvent(playedHandsPresent);
        OnDisplayAllHandsInPlay.RaiseEvent(pokerHandsInPlay);
        roundManager.EndOfRound();
    }

    [ClientRpc]
    public void RoundResultClientRpc(RoundResultTypes roundResult, ClientRpcParams clientRpcParams = default)
    {
        roundManager.EndOfRoundResult(roundResult);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyForNextRoundServerRpc(bool isPlayerReady, ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        if (gameState != GameState.RESULT) return;
        if (!allPlayerData.ContainsKey(senderClientId)) return;
        if (allPlayerData[senderClientId].state != PlayerState.PLAYING) return;

        if (isPlayerReady)
        {
            readyClients.Add(senderClientId);
        }
        else
        {
            readyClients.Remove(senderClientId);
        }

#if UNITY_EDITOR
        Debug.Log($"players who were ready: {playersReadyForNextRound} / {GetNumberOfInGamePlayers()} => now ready: {readyClients.Count()} / {GetNumberOfInGamePlayers()}");
#endif
        playersReadyForNextRound = readyClients.Count();
        if (playersReadyForNextRound >= GetNumberOfInGamePlayers())
        {
            playersReadyForNextRound = 0;
            readyClients.Clear();

            AdvanceGameState();
            if (GetNumberOfInGamePlayers() == 1)
            {
                List<PlayerData> playerStandings = eliminatedClients;
                playerStandings.Add(allPlayerData.Values.First(playerData => playerData.state == PlayerState.PLAYING));
                GameWinnerClientRpc(playerStandings.ToArray());
            }
            else
            {
                CardManager.Instance.DistributeCards();
                TurnManager.Instance.ResetTurnForNewRound();
                StartNextRoundClientRpc();
            }
        }
        else
        {
            UpdateNumberOfReadyClientsClientRpc(playersReadyForNextRound);
        }
    }

    [ClientRpc]
    public void UpdateNumberOfReadyClientsClientRpc(int playersReadyCount)
    {
        NextRoundUI.Instance.SetNumberOfPlayersReadyText(playersReadyCount);
    }

    [ClientRpc]
    public void StartNextRoundClientRpc()
    {
        roundManager.StartNextRound();
        // TODO: should played hand log be a network list?
        playedHandLog.Clear();
        OnClearCardLog?.Invoke();
    }

    [ClientRpc]
    public void GameWinnerClientRpc(PlayerData[] playerStandings)
    {
        int myPosition = 0;
        for (int i = 0; i < playerStandings.Length; i++)
        {
            if (playerStandings[i].GetClientId() == NetworkManager.Singleton.LocalClientId)
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
        // TODO: should played hand log be a network list?
        playedHandLog.Clear();
        OnClearCardLog?.Invoke();
        OnInitializeNewGame.RaiseEvent();
        ReportReadyServerRpc();
    }

    [ClientRpc]
    public void ExitGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        playedHandLog.Clear();
        SceneTransitionHandler.Instance.ExitAndLoadStartMenu();
    }
}

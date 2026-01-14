using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_CHARACTER = "Character";
    public const string KEY_GAME_MODE = "GameType";
    public const string KEY_TIME_FOR_PLAYER = "TimeForPlayer";
    public const string KEY_START_GAME = "Start";

    public static string k_DefaultLobbyName = "Lobby";
    private const float k_refreshLobbyListInterval = 5.0f;
    private const float k_lobbyPollInterval = 1.1f;
    private const float k_lobbyHeartbeatInterval = 15f;

    [SerializeField] GameRulesSO baseRules;

    [Header("Firing Events")]
    [SerializeField] private LobbyEventChannelSO OnJoinedLobby;
    [SerializeField] private LobbyEventChannelSO OnJoinedLobbyUpdate;
    [SerializeField] private LobbyEventChannelSO OnKickedFromLobby;
    [SerializeField] private LobbyEventChannelSO OnLobbyChanged;
    [SerializeField] private LobbyListEventChannelSO OnLobbyListChanged;
    [SerializeField] private StringEventChannelSO OnFailedToJoinLobbyByCode;
    [SerializeField] private VoidEventChannelSO OnLeftLobby;
    [SerializeField] private VoidEventChannelSO OnGameStarted;
    [SerializeField] private VoidEventChannelSO OnGameFailedToStart;

    [Header("Listening Events")]
    [SerializeField] private StringEventChannelSO OnAllClientsLoadedScene;
    [SerializeField] private StringEventChannelSO OnUpdatePlayerDisplayName;

    private float m_lobbyHeartbeatTimer = 15f;
    private float m_lobbyPollTimer = 1.1f;
    private float m_refreshLobbyListTimer = 5f;
    private Lobby m_joinedLobby;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        m_joinedLobby = null;

        RefreshLobbyList();
    }

    private void OnEnable()
    {
        OnAllClientsLoadedScene.OnEventRaised += DeleteLobby;
        OnUpdatePlayerDisplayName.OnEventRaised += UpdatePlayerName;
    }

    private void OnDisable()
    {
        OnAllClientsLoadedScene.OnEventRaised -= DeleteLobby;
        OnUpdatePlayerDisplayName.OnEventRaised -= UpdatePlayerName;
    }

    private void Update()
    {
        if (m_joinedLobby == null && SceneTransitionHandler.Instance.IsInMainMenuScene())
        {
            HandleRefreshLobbyList();
        }
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    private void HandleRefreshLobbyList()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            m_refreshLobbyListTimer -= Time.deltaTime;
            if (m_refreshLobbyListTimer <= 0f)
            {
                m_refreshLobbyListTimer = k_refreshLobbyListInterval;

                RefreshLobbyList();
            }
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            m_lobbyHeartbeatTimer -= Time.deltaTime;
            if (m_lobbyHeartbeatTimer < 0f)
            {
                m_lobbyHeartbeatTimer = k_lobbyHeartbeatInterval;

                await LobbyService.Instance.SendHeartbeatPingAsync(m_joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (m_joinedLobby != null)
        {
            m_lobbyPollTimer -= Time.deltaTime;
            if (m_lobbyPollTimer < 0f)
            {
                m_lobbyPollTimer = k_lobbyPollInterval;

                m_joinedLobby = await LobbyService.Instance.GetLobbyAsync(m_joinedLobby.Id);

                OnJoinedLobbyUpdate.RaiseEvent(m_joinedLobby);

                if (!IsPlayerInLobby())
                {
                    OnKickedFromLobby.RaiseEvent(m_joinedLobby);

                    m_joinedLobby = null;
                }

                if (m_joinedLobby != null && m_joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        GameType gameType = Enum.Parse<GameType>(m_joinedLobby.Data[KEY_GAME_MODE].Value);
                        TimeForTurnType timeForPlayer = Enum.Parse<TimeForTurnType>(m_joinedLobby.Data[KEY_TIME_FOR_PLAYER].Value);

                        GameRulesSO selectedRules = GameRulesFactory.CreateRuntime(baseRules);
                        selectedRules.selectedGameType = gameType;
                        selectedRules.timeForTurn = timeForPlayer;
#if UNITY_EDITOR
                        Debug.Log($"server rules: {gameType}, {timeForPlayer}");
#endif
                        GameSession.Instance.SetRules(selectedRules);

                        RelayManager.Instance.JoinRelay(m_joinedLobby.Data[KEY_START_GAME].Value);
                        SceneTransitionHandler.Instance.SetSceneState(SceneStates.InGame);
                        OnGameStarted.RaiseEvent();
                    }

                    m_joinedLobby = null;
                }
            }
        }
    }

    public Lobby GetJoinedLobby()
    {
        return m_joinedLobby;
    }

    public bool IsLobbyHost()
    {
        return m_joinedLobby != null && m_joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (m_joinedLobby != null && m_joinedLobby.Players != null)
        {
            foreach (Player player in m_joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // This player is in this lobby
                    return true;
                }
            }
        }
        return false;
    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerManager.Instance.GetLocalPlayerName()) },
        });
    }

    public void ChangeGameType()
    {
        if (IsLobbyHost())
        {
            GameType gameType = Enum.Parse<GameType>(m_joinedLobby.Data[KEY_GAME_MODE].Value);

            if (gameType == GameType.Ascending)
            {
                gameType = GameType.Descending;
            }
            else
            {
                gameType = GameType.Ascending;
            }

            UpdateLobbyGameType(gameType);
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, LobbyType lobbyType, GameType gameType, TimeForTurnType playerTimer)
    {
        Player player = GetPlayer();

        CreateLobbyOptions options = new()
        {
            Player = player,
            IsPrivate = lobbyType == LobbyType.Private,
            Data = new Dictionary<string, DataObject> {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameType.ToString()) },
                { KEY_TIME_FOR_PLAYER, new DataObject(DataObject.VisibilityOptions.Public, playerTimer.ToString()) },
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") },
            }
        };

        try
        {
            if (string.IsNullOrEmpty(lobbyName))
            {
                if (PlayerManager.Instance.GetLocalPlayerName().Last() == 's')
                {
                    lobbyName = $"{PlayerManager.Instance.GetLocalPlayerName()}' {k_DefaultLobbyName}";
                }
                else
                {
                    lobbyName = $"{PlayerManager.Instance.GetLocalPlayerName()}'s {k_DefaultLobbyName}";
                }
            }
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            m_joinedLobby = lobby;

            OnJoinedLobby.RaiseEvent(lobby);

#if UNITY_EDITOR
            Debug.Log("Created Lobby " + lobby.Name);
#endif
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.Log(e.Message);
#endif
        }
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            OnLobbyListChanged.RaiseEvent(lobbyListQueryResponse.Results);
        }
        catch (LobbyServiceException e)
        {
#if UNITY_EDITOR
            Debug.LogError(e.Message);
#endif
        }
    }

    public async void JoinLobbyByCode(string joinCode)
    {
        try
        {
            Player player = GetPlayer();

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, new JoinLobbyByCodeOptions
            {
                Player = player
            });

            m_joinedLobby = lobby;

            // TODO: when event occurs that Lobby.Data is changed, check if changes include Relay joinCode
            // await LobbyService.Instance.SubscribeToLobbyEventsAsync(m_joinedLobby.Id, new LobbyEventCallbacks { });

            OnJoinedLobby.RaiseEvent(lobby);
        }
        catch (LobbyServiceException e)
        {
#if UNITY_EDITOR
            Debug.LogError(e.Message);
#endif
            OnFailedToJoinLobbyByCode.RaiseEvent("Unable to join lobby");
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            Player player = GetPlayer();

            m_joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
            {
                Player = player
            });

            // TODO: when event occurs that Lobby.Data is changed, check if changes include Relay joinCode
            // await LobbyService.Instance.SubscribeToLobbyEventsAsync(m_joinedLobby.Id, new LobbyEventCallbacks { });

            OnJoinedLobby.RaiseEvent(lobby);
        }
        catch (LobbyServiceException e)
        {
#if UNITY_EDITOR
            Debug.LogError(e.Message);
#endif
        }
    }

    private async void UpdatePlayerName(string playerName)
    {
        if (m_joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new()
                {
                    Data = new Dictionary<string, PlayerDataObject>() {
                        {
                            KEY_PLAYER_NAME, new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: playerName
                            )
                        }
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(m_joinedLobby.Id, playerId, options);
                m_joinedLobby = lobby;

                OnJoinedLobbyUpdate.RaiseEvent(m_joinedLobby);
            }
            catch (LobbyServiceException e)
            {
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
            }
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            m_joinedLobby = lobby;

            OnJoinedLobby.RaiseEvent(lobby);
        }
        catch (LobbyServiceException e)
        {
#if UNITY_EDITOR
            Debug.LogError(e.Message);
#endif
        }
    }

    public async void LeaveLobby()
    {
        if (m_joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(m_joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                m_joinedLobby = null;

                OnLeftLobby.RaiseEvent();
            }
            catch (LobbyServiceException e)
            {
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
            }
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(m_joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
            }
        }
    }

    // TODO: generalize update lobby call
    private async void UpdateLobbyGameType(GameType gameType)
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(m_joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameType.ToString()) }
                }
            });

            m_joinedLobby = lobby;

            OnLobbyChanged.RaiseEvent(m_joinedLobby);
        }
        catch (LobbyServiceException e)
        {
#if UNITY_EDITOR
            Debug.LogError(e.Message);
#endif
        }
    }

    public async void UpdateTimeForTurn(TimeForTurnType timeForTurnType)
    {
        if (!IsLobbyHost()) return;

        try
        {
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(m_joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { KEY_TIME_FOR_PLAYER, new DataObject(DataObject.VisibilityOptions.Public, timeForTurnType.ToString()) }
                }
            });

            m_joinedLobby = lobby;

            OnLobbyChanged.RaiseEvent(m_joinedLobby);
        }
        catch (LobbyServiceException e)
        {
#if UNITY_EDITOR
            Debug.LogError(e.Message);
#endif
        }
    }

    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                OnGameStarted.RaiseEvent();

                GameType gameType = Enum.Parse<GameType>(m_joinedLobby.Data[KEY_GAME_MODE].Value);
                TimeForTurnType timeForPlayer = Enum.Parse<TimeForTurnType>(m_joinedLobby.Data[KEY_TIME_FOR_PLAYER].Value);

                GameRulesSO selectedRules = GameRulesFactory.CreateRuntime(baseRules);
                selectedRules.selectedGameType = gameType;
                selectedRules.timeForTurn = timeForPlayer;
#if UNITY_EDITOR
                Debug.Log($"rules: {gameType}, {timeForPlayer}");
#endif
                GameSession.Instance.SetRules(selectedRules);

                string relayCode = await RelayManager.Instance.CreateRelay(m_joinedLobby.Players.Count);

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(m_joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                m_joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
#if UNITY_EDITOR
                Debug.Log($"Could not start relay, {e.Message}");
#endif
                OnGameFailedToStart.RaiseEvent();
            }
        }
    }

    private async void DeleteLobby(string sceneName)
    {
        if (IsLobbyHost())
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(m_joinedLobby.Id);
                m_joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
#if UNITY_EDITOR
                Debug.Log($"Could not delete lobby, {e.Message}");
#endif
            }
        }
    }
}

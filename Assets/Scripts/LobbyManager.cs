using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
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
    public const string KEY_START_GAME = "Start";

    public static string k_DefaultLobbyName = "Lobby";

    // TODO: maybe refactor events to be delegate (arg type) instead of EventHandler
    public event EventHandler OnLeftLobby;


    [HideInInspector]
    public delegate void AuthenticatedDelegateHandler(string authPlayerId);
    [HideInInspector]
    public event AuthenticatedDelegateHandler OnAuthenticated;

    public event EventHandler<EventArgs> OnGameStarted;

    public event EventHandler<CustomGenericEventArgs.EventFailureArgs> OnFailedToJoinLobbyByCode;

    public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
    public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
    public event EventHandler<LobbyEventArgs> OnLobbyGameTypeChanged;
    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public enum LobbyType
    {
        Public,
        Private
    }

    private float m_heartbeatTimer = 15f;
    private float m_lobbyPollTimer = 1.1f;
    private float m_refreshLobbyListTimer = 5f;
    private Lobby m_joinedLobby;
    private string m_playerName;
    public string PlayerName
    {
        get { return m_playerName; }
        private set { m_playerName = value; }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //HandleRefreshLobbyList(); // Disabled Auto Refresh for testing with multiple builds
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public async void Authenticate(string playerName)
    {
        try
        {
            m_playerName = playerName;
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(playerName);

            await UnityServices.InitializeAsync(initializationOptions);

            AuthenticationService.Instance.SignedIn += () => {
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
                OnAuthenticated?.Invoke(AuthenticationService.Instance.PlayerId);
                RefreshLobbyList();
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void HandleRefreshLobbyList()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            m_refreshLobbyListTimer -= Time.deltaTime;
            if (m_refreshLobbyListTimer < 0f)
            {
                float refreshLobbyListTimerMax = 5f;
                m_refreshLobbyListTimer = refreshLobbyListTimerMax;

                RefreshLobbyList();
            }
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (IsLobbyHost())
        {
            m_heartbeatTimer -= Time.deltaTime;
            if (m_heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                m_heartbeatTimer = heartbeatTimerMax;

                Debug.Log("Heartbeat");
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
                float lobbyPollTimerMax = 1.1f;
                m_lobbyPollTimer = lobbyPollTimerMax;

                m_joinedLobby = await LobbyService.Instance.GetLobbyAsync(m_joinedLobby.Id);

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = m_joinedLobby });

                if (!IsPlayerInLobby())
                {
                    // Player was kicked out of this lobby
                    Debug.Log("Kicked from Lobby!");

                    OnKickedFromLobby?.Invoke(this, new LobbyEventArgs { lobby = m_joinedLobby });

                    m_joinedLobby = null;
                }

                if (m_joinedLobby != null && m_joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    if (!IsLobbyHost())
                    {
                        RelayManager.Instance.JoinRelay(m_joinedLobby.Data[KEY_START_GAME].Value);
                        OnGameStarted?.Invoke(this, EventArgs.Empty);
                    }

                    m_joinedLobby = null; // leaves current lobby so when game is over no lobby...
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
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerName) },
        });
    }

    public void ChangeGameType()
    {
        if (IsLobbyHost())
        {
            PokerHandsBullshitGame.GameType gameType =
                Enum.Parse<PokerHandsBullshitGame.GameType>(m_joinedLobby.Data[KEY_GAME_MODE].Value);

            if (gameType == PokerHandsBullshitGame.GameType.Ascending)
            {
                gameType = PokerHandsBullshitGame.GameType.Descending;
            }
            else
            {
                gameType = PokerHandsBullshitGame.GameType.Ascending;
            }

            UpdateLobbyGameType(gameType);
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, LobbyType lobbyType, PokerHandsBullshitGame.GameType gameType)
    {
        Player player = GetPlayer();

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Player = player,
            IsPrivate = lobbyType == LobbyType.Private,
            Data = new Dictionary<string, DataObject> {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameType.ToString()) },
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") },
            }
        };

        try
        {
            if (string.IsNullOrEmpty(lobbyName))
            {
                if (m_playerName.Last() == 's')
                {
                    lobbyName = $"{m_playerName}' {k_DefaultLobbyName}";
                }
                else
                {
                    lobbyName = $"{m_playerName}'s {k_DefaultLobbyName}";
                }
            }
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            m_joinedLobby = lobby;

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            Debug.Log("Created Lobby " + lobby.Name);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
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

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
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

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
            OnFailedToJoinLobbyByCode?.Invoke(this, new CustomGenericEventArgs.EventFailureArgs { failureString = "Unable to join lobby" });
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

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async void UpdatePlayerName(string playerName)
    {
        PlayerName = playerName;

        if (m_joinedLobby != null)
        {
            try
            {
                UpdatePlayerOptions options = new UpdatePlayerOptions();

                options.Data = new Dictionary<string, PlayerDataObject>() {
                    {
                        KEY_PLAYER_NAME, new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: playerName)
                    }
                };

                string playerId = AuthenticationService.Instance.PlayerId;

                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(m_joinedLobby.Id, playerId, options);
                m_joinedLobby = lobby;

                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = m_joinedLobby });
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
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

            OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
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

                OnLeftLobby?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e.Message);
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
                Debug.LogError(e.Message);
            }
        }
    }

    public async void UpdateLobbyGameType(PokerHandsBullshitGame.GameType gameType)
    {
        try
        {
            Debug.Log("UpdateLobbyGameType " + gameType);

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(m_joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameType.ToString()) }
                }
            });

            m_joinedLobby = lobby;

            OnLobbyGameTypeChanged?.Invoke(this, new LobbyEventArgs { lobby = m_joinedLobby });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            try
            {
                OnGameStarted?.Invoke(this, EventArgs.Empty);

                PokerHandsBullshitGame.GameType gameType =
                    Enum.Parse<PokerHandsBullshitGame.GameType>(m_joinedLobby.Data[KEY_GAME_MODE].Value);

                PokerHandsBullshitGame.Instance.InitializeSettings(gameType, m_joinedLobby.Players.Count);

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
                Debug.LogError(e.Message);
            }
        }
    }

}
